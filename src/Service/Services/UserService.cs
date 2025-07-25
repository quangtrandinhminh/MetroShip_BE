using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using MetroShip.Service.Validations;
using Microsoft.EntityFrameworkCore;
using Twilio.Rest.Api.V2010.Account;
using MetroShip.Service.Utils;

namespace MetroShip.Service.Services;

public class UserService(IServiceProvider serviceProvider) : IUserService
{
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly UserManager<UserEntity> _userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly UserValidator _userValidator = new();
    private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();
    private readonly RoleManager<RoleEntity> _roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();

    public async Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRoleEnum? role)
    {
        _logger.Information($"Get all users by role {role.ToString()}");
        Expression<Func<UserEntity, bool>> predicate = v => v.DeletedTime == null;

        List<RoleEntity> roleEntity;
        if (role != null)
        {
            predicate = predicate.And(x => x.UserRoles.Any(y => y.Role.Name.Equals(role.ToString())));
            roleEntity = await _roleManager.Roles.Where(x => x.Name == role.ToString()).ToListAsync();
        }
        else
        {
            roleEntity = await _roleManager.Roles.ToListAsync();
        }

        var users = await _userRepository.GetAllPaginatedQueryable(
            pageNumber, pageSize,
            predicate, null, e => e.UserRoles, _ => _.StaffAssignments);

        // Keep only IsActive StaffAssignments in users
        foreach (var user in users.Items)
        {
            user.StaffAssignments = user.StaffAssignments
                .Where(sa => sa.IsActive && sa.DeletedAt == null)
                .ToList();
        }

        // get station for staff
        var stationIds = users.Items
            .SelectMany(u => u.StaffAssignments)
            .Select(sa => sa.StationId)
            .Distinct()
            .ToList();

        var stationList = _stationRepository.GetAll()
        .Where(x => x.DeletedAt == null && stationIds.Contains(x.Id))
        .Select(x => new { x.Id, x.StationNameVi });

        var userResponse = _mapper.MapToUserResponsePaginatedList(users);
        foreach (var user in userResponse.Items)
        {
            user.Role = _mapper.MapRoleToRoleName(roleEntity);

            if (user.StaffAssignments != null && user.StaffAssignments.Any())
            {
                foreach (var assignment in user.StaffAssignments)
                {
                    // get station name for staff
                    var station = stationList.FirstOrDefault(x => x.Id == assignment.StationId);
                    if (station != null)
                    {
                        assignment.StationName = station.StationNameVi;
                    }
                }
            }
        }
        return userResponse;
    }

    public async Task<string> CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.Information("Create user {@request}", request);
        _userValidator.ValidateUserCreateRequest(request);
        // parse role from request
        if (!Enum.TryParse<UserRoleEnum>(request.Role.ToString(), out var role))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, ResponseMessageIdentity.ROLE_INVALID,
                StatusCodes.Status400BadRequest);
        }
        // check role is valid in system
        var roleEntity = await _roleManager.FindByNameAsync(role.ToString());
        if (roleEntity == null)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageIdentity.ROLE_INVALID, StatusCodes.Status400BadRequest);
        }

        // get user by name
        var validateUser = await _userRepository.IsExistAsync(
            u => u.UserName.Equals(request.UserName) 
            || u.NormalizedUserName.Equals(request.UserName.Normalize()));
        if (validateUser)
        {
            throw new AppException(HttpResponseCodeConstants.EXISTED,
                ResponseMessageIdentity.EXISTED_USER, StatusCodes.Status400BadRequest);
        }

        var existingUserWithEmail = await _userRepository.IsExistAsync(
            x => x.Email == request.Email || x.NormalizedEmail == request.Email.Normalize());
        if (existingUserWithEmail)
        {
            throw new AppException(HttpResponseCodeConstants.EXISTED,
                ResponseMessageIdentity.EXISTED_EMAIL, StatusCodes.Status400BadRequest);
        }

        var existingUserWithPhone = await _userManager.Users.FirstOrDefaultAsync(
            x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (existingUserWithPhone != null)
        {
            throw new AppException(HttpResponseCodeConstants.EXISTED,
                ResponseMessageIdentity.EXISTED_PHONE, StatusCodes.Status400BadRequest);
        }

        try
        {
            var account = _mapper.MapToUserEntity(request);
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            account.SecurityStamp = Guid.NewGuid().ToString();
            account.Verified = CoreHelper.SystemTimeNow;
            account.NormalizedUserName = _userManager.NormalizeName(request.UserName);
            account.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
            await _userRepository.CreateUserAsync(account, cancellationToken);
            var roleIds = new List<string> { roleEntity.Id };
            await _userRepository.AddUserToRoleAsync(account.Id, roleIds, cancellationToken);
            var count = await _userRepository.SaveChangeAsync();
            if (count > 0)
            {
                // send email to user
                _logger.Information("Send email to user {@email}", account.Email);
                var sendMailModel = new SendMailModel
                {
                    Email = account.Email,
                    Type = MailTypeEnum.Account,
                    Name = account.FullName,
                    UserName = account.UserName,
                    Password = request.Password,
                    Role = role.ToString()
                };
                _emailService.SendMail(sendMailModel);
            }
            return account.Id;
        }
        catch (Exception e)
        {
            throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
        }
    }

    public async Task UpdateUserAsync(UserUpdateRequest request)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Update user {@request} for user {userId}", request, userId);
        _userValidator.ValidateUserUpdateRequest(request);
        var user = await GetUserById(userId);
        if (user.UserName is not null && user.UserName != request.UserName)
        {
            var validateUser = await _userManager.FindByNameAsync(request.UserName);
            if (validateUser != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED,
                    ResponseMessageIdentity.EXISTED_USER,
                    StatusCodes.Status409Conflict);
            }
            /*var existingUser = await _userRepository.GetSingleAsync(u => u.UserName == request.UserName);
            if (existingUser != null)
            {
                throw new AppException(
                HttpResponseCodeConstants.DUPLICATE,
                ResponseMessageConstantsUser.USER_EXISTED,
                StatusCodes.Status409Conflict);
            }*/
        }

        _mapper.MapUserRequestToEntity(request, user);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    public async Task<UserResponse> GetByIdAsync(object id)
    {
        _logger.Information($"Get user by id {id}");
        var user = await _userRepository.GetSingleAsync(e => e.Id == id,
            x => x.UserRoles, _ => _.StaffAssignments
            );
        if (user == null)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND,
                ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        }

        var stationIds = user.StaffAssignments
            .Select(sa => sa.StationId)
            .Distinct()
            .ToList();

        var stationList = _stationRepository.GetAll()
            .Where(x => x.DeletedAt == null && stationIds.Contains(x.Id))
            .Select(x => new { x.Id, x.StationNameVi });

        var response = _mapper.MapToUserResponse(user);
        var roles = await _userManager.GetRolesAsync(user);
        response.Role = roles;
        if (response.StaffAssignments != null && response.StaffAssignments.Any())
        {
            foreach (var assignment in response.StaffAssignments)
            {
                // get station name for staff
                var station = stationList.FirstOrDefault(x => x.Id == assignment.StationId);
                if (station != null)
                {
                    assignment.StationName = station.StationNameVi;
                }
            }

            // orderByDescending by IsActive and FromTime
            response.StaffAssignments = response.StaffAssignments
                .OrderByDescending(sa => sa.IsActive)
                .ThenByDescending(sa => sa.FromTime)
                .ToList();
        }
        return response;
    }

    public async Task BanUserAsync(object id)
    {
        _logger.Information($"Delete user by id {id}");
        var user = await GetUserById(id);
        user.DeletedTime = CoreHelper.SystemTimeNow;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    private async Task<UserEntity> GetUserById(object id)
    {
        var user = await _userRepository.GetSingleAsync(e => e.Id == id);
        if (user == null)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND,
                ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        }

        return user;
    }
}