namespace MetroShip.Service.ApiModels.User
{
    public sealed record LoginResponse : UserResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiredTime { get; set; }
    }
}
