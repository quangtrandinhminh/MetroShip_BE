namespace MetroShip.Service.ApiModels.User
{
    public sealed record RegisterRequest
    {
        /*[Required(ErrorMessage = ResponseMessageIdentity.USERNAME_REQUIRED)]
        [MaxLength(100)]*/
        public string UserName { get; set; }

        /*[Required(ErrorMessage = ResponseMessageIdentity.NAME_REQUIRED)]
        [MaxLength(100)]
        [RegularExpression("^[^0-9]+$", ErrorMessage = "Name cannot contain number")]*/
        public string FullName { get; set; }

        /*[Required(ErrorMessage = ResponseMessageIdentity.EMAIL_REQUIRED)]
        [EmailAddress(ErrorMessage = "Invalid email format")]*/
        public string Email { get; set; }

        /*[Required(ErrorMessage = ResponseMessageIdentity.PHONENUMBER_REQUIRED)]
        [Phone(ErrorMessage = ResponseMessageIdentity.PHONENUMBER_INVALID)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = ResponseMessageIdentity.PHONENUMBER_LENGTH)]*/
        public string PhoneNumber { get; set; }

        /*
        [Required(ErrorMessage = ResponseMessageIdentity.PASSWORD_REQUIRED)]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = ResponseMessageIdentity.PASSSWORD_LENGTH)]
        [MaxLength(100)]*/
        public string Password { get; set; }

        /*[Required(ErrorMessage = ResponseMessageIdentity.CONFIRM_PASSWORD_REQUIRED)]
        [Compare(nameof(Password), ErrorMessage = ResponseMessageIdentity.PASSWORD_NOT_MATCH)]
        [DataType(DataType.Password)]
        [MaxLength(100)]*/
        public string ConfirmPassword { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}