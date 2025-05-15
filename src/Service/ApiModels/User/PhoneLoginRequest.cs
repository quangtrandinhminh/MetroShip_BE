namespace MetroShip.Service.ApiModels.User;

public sealed class PhoneLoginRequest
{
    public string PhoneNumber { get; set; }
    public string? OTP { get; set; }
}