using Moq;
using Microsoft.AspNetCore.Http;
using MetroShip.Service.ApiModels.User;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Base;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Services;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using MetroShip.Utility.Constants;

namespace MetroShip.Test
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<UserEntity>> _userManagerMock;
        private readonly Mock<RoleManager<RoleEntity>> _roleManagerMock;
        private readonly Mock<IBaseRepository<RefreshToken>> _refreshTokenRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapperlyMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock; // Added mock for IEmailService
        private readonly AuthService _authService;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public AuthServiceTests()
        {
            // Setup mocks and services
            _serviceProviderMock = new Mock<IServiceProvider>();
            _userRepositoryMock = new Mock<IUserRepository>();

            // Mock dependencies for UserManager
            var userStoreMock = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<UserEntity>>(),
                Array.Empty<IUserValidator<UserEntity>>(),
                Array.Empty<IPasswordValidator<UserEntity>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<UserEntity>>>()
            );

            // Mock dependencies for RoleManager
            var roleStoreMock = new Mock<IRoleStore<RoleEntity>>();
            _roleManagerMock = new Mock<RoleManager<RoleEntity>>(
                roleStoreMock.Object,
                Array.Empty<IRoleValidator<RoleEntity>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<ILogger<RoleManager<RoleEntity>>>()
            );

            _refreshTokenRepositoryMock = new Mock<IBaseRepository<RefreshToken>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapperlyMapper>();
            _emailServiceMock = new Mock<IEmailService>();

            // Configure the service provider mock to return the mocked services
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmailService)))
                .Returns(_emailServiceMock.Object);

            _authService = new AuthService(
                _serviceProviderMock.Object,
                _userRepositoryMock.Object,
                _roleManagerMock.Object,
                _userManagerMock.Object,
                _refreshTokenRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Authenticate_InvalidUsername_ThrowsAppException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "invalidUser", Password = "password123" };

            _userRepositoryMock.Setup(repo => repo.GetSingleAsync(
                It.IsAny<Expression<Func<UserEntity, bool>>>(),
                It.IsAny<Expression<Func<UserEntity, object>>[]>()
            )).ReturnsAsync((UserEntity?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _authService.Authenticate(loginRequest));
            Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
            Assert.Equal(ErrorCode.UserInvalid, exception.Code);
        }

        [Fact]
        public async Task Authenticate_InvalidPassword_ThrowsAppException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "validUser", Password = "wrongPassword" };
            var userEntity = new UserEntity
            {
                UserName = loginRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                Verified = CoreHelper.SystemTimeNow,
            };

            _userRepositoryMock.Setup(repo => repo.GetSingleAsync(
                It.IsAny<Expression<Func<UserEntity, bool>>>(),
                It.IsAny<Expression<Func<UserEntity, object>>[]>()
            )).ReturnsAsync(userEntity);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _authService.Authenticate(loginRequest));
            Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
            Assert.Equal(ErrorCode.UserPasswordWrong, exception.Code);
        }

        [Fact]
        public async Task Authenticate_InactiveAccount_ThrowsAppException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "inactiveUser", Password = "password123" };
            var userEntity = new UserEntity
            {
                UserName = loginRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password),
                Verified = null,
                DeletedTime = null,
            };

            _userRepositoryMock.Setup(repo => repo.GetSingleAsync(
                It.IsAny<Expression<Func<UserEntity, bool>>>(),
                It.IsAny<Expression<Func<UserEntity, object>>[]>()
            )).ReturnsAsync(userEntity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _authService.Authenticate(loginRequest));
            Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
            Assert.Equal(ErrorCode.UserInActive, exception.Code);
        }

        [Fact]
        public async Task Authenticate_WithDeletedAccount_ThrowsAppException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "deletedUser", Password = "validPassword123!" };
            var userEntity = new UserEntity { UserName = "deletedUser", DeletedTime = DateTime.Now };

            _userRepositoryMock.Setup(repo => repo.GetSingleAsync(
                It.IsAny<Expression<Func<UserEntity, bool>>>(),
                It.IsAny<Expression<Func<UserEntity, object>>[]>()
            )).ReturnsAsync(userEntity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _authService.Authenticate(loginRequest));
            Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
            Assert.Equal(ErrorCode.UserInvalid, exception.Code);
        }
    }


}