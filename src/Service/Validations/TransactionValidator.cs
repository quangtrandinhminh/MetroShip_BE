using FluentValidation;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.Helpers;

namespace MetroShip.Service.Validations;

public static class TransactionValidator
{
    public static void ValidateTransactionRequest(TransactionRequest request)
    {
        var _transactionRequestValidator = new TransactionRequestValidator();
        _transactionRequestValidator.ValidateApiModel(request);
    }
}

public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty()
            .WithMessage("ShipmentId is required.");
    }
}