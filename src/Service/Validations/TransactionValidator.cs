using FluentValidation;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.Helpers;

namespace MetroShip.Service.Validations;

public class TransactionValidator
{
    private readonly IValidator<TransactionRequest> _transactionRequestValidator;
    public TransactionValidator()
    {
        _transactionRequestValidator = new TransactionRequestValidator();
    }

    public void ValidateTransactionRequest(TransactionRequest request)
    {
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