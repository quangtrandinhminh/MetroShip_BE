using FluentValidation;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Helpers;
using RestSharp.Extensions;

namespace MetroShip.Service.Validations;

public static class MetroLineValidator
{
    public static void ValidateMetroLineCreateRequest (this MetroRouteRequest request)
    {
        var validator = new MetroLineCreateValidator();
        validator.ValidateApiModel(request);
    }

    /*private static void ValidateCreateStationRequest(CreateStationRequest request)
    {
        var validator = new StationCreateValidator();
        validator.ValidateApiModel(request);
    }*/
}

public class MetroLineCreateValidator : AbstractValidator<MetroRouteRequest>
{
    public MetroLineCreateValidator()
    {
        RuleFor(x => x.LineNameVi)
            .NotEmpty().WithMessage("Tên tuyến bằng tiếng Việt không được để trống.")
            .MaximumLength(100).WithMessage("Tên tuyến bằng tiếng Việt không được vượt quá 100 ký tự.");

        RuleFor(x => x.LineNameEn)
            .NotEmpty().WithMessage("Tên tuyến bằng tiếng Anh không được để trống.")
            .MaximumLength(100).WithMessage("Tên tuyến bằng tiếng Anh không được vượt quá 100 ký tự.");

        RuleFor(x => x.RegionId)
            .NotEmpty().WithMessage("Vùng không được để trống.")
            .Must(regionId => Guid.TryParse(regionId, out var _))
            .WithMessage("Vùng phải là một GUID hợp lệ.");

        RuleFor(x => x.LineNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Số tuyến phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.LineCode)
            .MaximumLength(20).WithMessage("Mã tuyến không được vượt quá 20 ký tự.")
            //.Matches(@"^[A-Za-z0-9]+$").WithMessage("Mã tuyến chỉ được gồm chữ cái và số.")
            .When(x => !string.IsNullOrEmpty(x.LineCode));

        RuleFor(x => x.LineType)
            .MaximumLength(100).WithMessage("Loại tuyến không được vượt quá 100 ký tự.")
            //.Matches(@"^[A-Za-z\s]+$").WithMessage("Loại tuyến chỉ được gồm chữ cái và khoảng trắng.")
            .When(x => !string.IsNullOrEmpty(x.LineType));

        RuleFor(x => x.LineOwner)
            .MaximumLength(100).WithMessage("Chủ sở hữu tuyến không được vượt quá 100 ký tự.")
            //.Matches(@"^[A-Za-z\s]+$").WithMessage("Chủ sở hữu tuyến chỉ được gồm chữ cái và khoảng trắng.")
            .When(x => !string.IsNullOrEmpty(x.LineOwner));

        RuleFor(x => x.ColorHex)
            .Matches(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")
            .WithMessage("Mã màu phải là một mã màu hex hợp lệ. Dạng #xxx hoặc #123456.")
            .When(x => !string.IsNullOrEmpty(x.ColorHex));

        RuleFor(x => x.RouteTimeMin)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian hành trình phải lớn hơn hoặc bằng 0.")
            .When(x => x.RouteTimeMin.HasValue);

        RuleFor(x => x.DwellTimeMin)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian dừng phải lớn hơn hoặc bằng 0.")
            .When(x => x.DwellTimeMin.HasValue);

        RuleFor(x => x.Stations)
            .NotEmpty().WithMessage("Cần phải có ít nhất một ga.")
            .Must(stations => stations.Count >= 2)
            .WithMessage("Cần phải có ít nhất hai ga cho một tuyến metro.");

        RuleForEach(x => x.Stations).SetValidator(new StationCreateValidator());
    }
}

public class StationCreateValidator : AbstractValidator<CreateStationWithMetroRouteRequest>
{
    public StationCreateValidator()
    {
        // Nếu Id được cung cấp, phải là GUID hợp lệ (nhưng có thể null)
        RuleFor(x => x.Id)
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out var _))
            .WithMessage("ID ga phải là một GUID hợp lệ nếu được cung cấp.");

        /*RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Trạng thái hoạt động phải được chỉ định khi tạo ga mới.");*/

        RuleFor(x => x.ToNextStationKm)
            .NotNull().WithMessage("Khoảng cách tới ga kế tiếp là bắt buộc khi tạo ga mới.")
            .GreaterThanOrEqualTo(0).WithMessage("Khoảng cách tới ga kế tiếp phải là số không âm.");

        // Nếu Id là null/rỗng, các trường khác là bắt buộc
        When(x => string.IsNullOrEmpty(x.Id), () => {
            RuleFor(x => x.StationNameVi)
                .NotEmpty().WithMessage("Tên ga bằng tiếng Việt là bắt buộc khi tạo ga mới.")
                .MaximumLength(100).WithMessage("Tên ga bằng tiếng Việt không được vượt quá 100 ký tự.");

            RuleFor(x => x.StationNameEn)
                .NotEmpty().WithMessage("Tên ga bằng tiếng Anh là bắt buộc khi tạo ga mới.")
                .MaximumLength(100).WithMessage("Tên ga bằng tiếng Anh không được vượt quá 100 ký tự.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Địa chỉ là bắt buộc khi tạo ga mới.")
                .MaximumLength(200).WithMessage("Địa chỉ không được vượt quá 200 ký tự.");

            RuleFor(x => x.IsUnderground)
                .NotNull().WithMessage("Thông tin ga ngầm phải được chỉ định khi tạo ga mới.");

            RuleFor(x => x.RegionId)
                //.NotEmpty().WithMessage("Mã vùng là bắt buộc khi tạo ga mới.")
                .Must(regionId => Guid.TryParse(regionId, out var _))
                .WithMessage("Mã vùng phải là một GUID hợp lệ khi tạo ga mới.")
                .When(x => !string.IsNullOrEmpty(x.RegionId));

            RuleFor(x => x.Latitude)
                .NotNull().WithMessage("Vĩ độ là bắt buộc khi tạo ga mới.")
                .InclusiveBetween(-90, 90).WithMessage("Vĩ độ phải nằm trong khoảng từ -90 đến 90 độ.");

            RuleFor(x => x.Longitude)
                .NotNull().WithMessage("Kinh độ là bắt buộc khi tạo ga mới.")
                .InclusiveBetween(-180, 180).WithMessage("Kinh độ phải nằm trong khoảng từ -180 đến 180 độ.");
        });

        // When Id is provided, other fields should be null/empty 
        When(x => !string.IsNullOrEmpty(x.Id), () => {
            RuleFor(x => x.StationNameVi)
                .Empty().WithMessage("Không được cung cấp tên ga khi sử dụng ID ga đã có.")
                .When(x => !string.IsNullOrEmpty(x.StationNameVi));

            // Other validations for fields when Id is provided
        });
    }
}