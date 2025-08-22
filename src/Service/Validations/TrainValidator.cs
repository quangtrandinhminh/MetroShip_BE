using FluentValidation;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.Validations;

public class TrainValidator
{
    public void ValidateLineSlotDateFilterRequest(LineSlotDateFilterRequest request)
    {
        var trainLineSlotDateValidator = new LineSlotDateFilterRequestValidator();
        trainLineSlotDateValidator.ValidateApiModel(request);
    }

    public void ValidateTrainListFilterRequest(TrainListFilterRequest request)
    {
        var validator = new TrainListFilterRequestValidator();
        validator.ValidateApiModel(request);
    }

    public void ValidateAddTrainToItinerariesRequest(AddTrainToItinerariesRequest request)
    {
        var validator = new ValidateAddTrainToItinerariesRequest();
        validator.ValidateApiModel(request);
    }

    public void ValidateCreateTrainRequest(CreateTrainRequest request)
    {
        var validator = new ValidateCreateTrainRequest();
        validator.ValidateApiModel(request);
    }
}

public class LineSlotDateFilterRequestValidator : AbstractValidator<LineSlotDateFilterRequest>
{
    public LineSlotDateFilterRequestValidator()
    {
        RuleFor(x => x.LineId)
            .NotEmpty().WithMessage(ResponseMessageTrain.LINE_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.LINE_ID_INVALID);

        RuleFor(x => x.TimeSlotId)
            .NotEmpty().WithMessage(ResponseMessageTrain.TIME_SLOT_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.TIME_SLOT_ID_INVALID);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage(ResponseMessageTrain.DATE_REQUIRED)
            .Must(date => DateOnly.TryParse(date.ToString(), out _) && date > DateOnly.MinValue)
            .WithMessage(ResponseMessageTrain.DATE_INVALID);
    }
}

public class TrainListFilterRequestValidator : AbstractValidator<TrainListFilterRequest>
{
    public TrainListFilterRequestValidator()
    {
        RuleFor(x => x.LineId)
            .Must(x => string.IsNullOrEmpty(x) || Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageTrain.LINE_ID_INVALID);

        RuleFor(x => x.TimeSlotId)
            .Must(x => string.IsNullOrEmpty(x) || Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageTrain.TIME_SLOT_ID_INVALID);

        RuleFor(x => x.Date)
            .Must(date => !date.HasValue || (DateOnly.TryParse(date.ToString(), out _) 
            && date > DateOnly.MinValue))
            .WithMessage(ResponseMessageTrain.DATE_INVALID);

        RuleFor(x => x.ModelName)
            .Must(x => string.IsNullOrEmpty(x) || x.Length <= 100)
            .WithMessage(ResponseMessageTrain.MODEL_NAME_TOO_LONG);

        RuleFor(x => x.IsAvailable)
            .Must(x => !x.HasValue || (x.Value == true || x.Value == false))
            .WithMessage(ResponseMessageTrain.IS_AVAILABLE_INVALID);
    }
}

public class ValidateAddTrainToItinerariesRequest : AbstractValidator<AddTrainToItinerariesRequest>
{
    public ValidateAddTrainToItinerariesRequest()
    {
        RuleFor(x => x.TrainId)
            .NotEmpty().WithMessage(ResponseMessageTrain.TRAIN_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.TRAIN_ID_INVALID);

        /*RuleFor(x => x.LineId)
            .NotEmpty().WithMessage(ResponseMessageTrain.LINE_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.LINE_ID_INVALID);*/

        RuleFor(x => x.TimeSlotId)
            .NotEmpty().WithMessage(ResponseMessageTrain.TIME_SLOT_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.TIME_SLOT_ID_INVALID);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage(ResponseMessageTrain.DATE_REQUIRED)
            // valid date only
            .Must(date => DateOnly.TryParse(date.ToString(), out _) && date > DateOnly.MinValue)
            .WithMessage(ResponseMessageTrain.DATE_INVALID);
    }
}

public class ValidateCreateTrainRequest : AbstractValidator<CreateTrainRequest>
{
    public ValidateCreateTrainRequest()
    {
        RuleFor(x => x.TrainCode)
            .MaximumLength(20).WithMessage(ResponseMessageTrain.TRAIN_CODE_TOO_LONG)
            .Matches(@"^[A-Z0-9]+$").WithMessage(ResponseMessageTrain.TRAIN_CODE_INVALID)
            .When(x => !string.IsNullOrEmpty(x.TrainCode));

        RuleFor(x => x.ModelName)
            .MaximumLength(50).WithMessage(ResponseMessageTrain.MODEL_NAME_TOO_LONG)
            .When(x => !string.IsNullOrEmpty(x.ModelName));

        RuleFor(x => x.LineId)
            .NotEmpty().WithMessage(ResponseMessageTrain.LINE_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.LINE_ID_INVALID);

        RuleFor(x => x.TrainNumber)
            .GreaterThan(0).WithMessage("Mã số tàu phải lớn hơn 0");

        RuleFor(x => x.CurrentStationId)
            .Must(x => string.IsNullOrEmpty(x) || Guid.TryParse(x, out _))
            .WithMessage("StationId không hợp lệ")
            .When(x => !string.IsNullOrEmpty(x.CurrentStationId));

        RuleFor(x => x.NumberOfCarriages)
            .NotEmpty().WithMessage("Số lượng toa tàu không được để trống")
            .GreaterThan(0).WithMessage("Số lượng toa tàu phải lớn hơn 0");

        RuleFor(x => x.MaxWeightPerCarriageKg)
            .GreaterThan(0).When(x => x.MaxWeightPerCarriageKg.HasValue)
            .WithMessage("Trọng lượng tối đa trên mỗi toa tàu (kg) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.MaxVolumePerCarriageM3)
            .GreaterThan(0).When(x => x.MaxVolumePerCarriageM3.HasValue)
            .WithMessage("Thể tích tối đa trên mỗi toa tàu (m3) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.CarriageLengthMeter)
            .GreaterThan(0).When(x => x.CarriageLengthMeter.HasValue)
            .WithMessage("Chiều dài toa tàu (m) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.CarriageWidthMeter)
            .GreaterThan(0).When(x => x.CarriageWidthMeter.HasValue)
            .WithMessage("Chiều rộng toa tàu (m) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.CarriageHeightMeter)
            .GreaterThan(0).When(x => x.CarriageHeightMeter.HasValue)
            .WithMessage("Chiều cao toa tàu (m) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.TopSpeedKmH)
            .GreaterThan(0).When(x => x.TopSpeedKmH.HasValue)
            .WithMessage("Tốc độ tối đa (km/h) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.TopSpeedUdgKmH)
            .GreaterThan(0).When(x => x.TopSpeedUdgKmH.HasValue)
            .WithMessage("Tốc độ tối đa UDG (km/h) không hợp lệ. Nó phải lớn hơn 0.");

        RuleFor(x => x.Latitude)
            .Must(lat => !lat.HasValue || (lat.Value >= -90 && lat.Value <= 90))
            .WithMessage("Vĩ độ không hợp lệ. Nó phải nằm trong khoảng từ -90 đến 90 độ.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .Must(lng => !lng.HasValue || (lng.Value >= -180 && lng.Value <= 180))
            .WithMessage("Kinh độ không hợp lệ. Nó phải nằm trong khoảng từ -180 đến 180 độ.")
            .When(x => x.Longitude.HasValue);
    }
}