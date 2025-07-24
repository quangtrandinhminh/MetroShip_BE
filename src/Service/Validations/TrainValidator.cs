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
            .Must(date => DateTimeOffset.TryParse(date.ToString(), out _) && date > DateTimeOffset.MinValue)
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
            .Must(date => !date.HasValue || (DateTimeOffset.TryParse(date.ToString(), out _) 
            && date > DateTimeOffset.MinValue))
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

        RuleFor(x => x.LineId)
            .NotEmpty().WithMessage(ResponseMessageTrain.LINE_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.LINE_ID_INVALID);

        RuleFor(x => x.TimeSlotId)
            .NotEmpty().WithMessage(ResponseMessageTrain.TIME_SLOT_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageTrain.TIME_SLOT_ID_INVALID);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage(ResponseMessageTrain.DATE_REQUIRED)
            .Must(date => DateTimeOffset.TryParse(date.ToString(), out _) && date > DateTimeOffset.MinValue)
            .WithMessage(ResponseMessageTrain.DATE_INVALID);
    }
}