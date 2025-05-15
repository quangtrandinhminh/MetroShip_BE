namespace MetroShip.Utility.Constants
{
    public class ErrorCode
    {
        public const string BadRequest = "Bad Request";
        public const string UnAuthenticated = "Un-Authenticate";
        public const string UnAuthorized = "Forbidden";
        public const string NotFound = "Not Found";
        public const string Unknown = "Oops! Something went wrong, please try again later.";
        public const string NotUnique = "The resource is already, please try another.";
        public const string TokenExpired = "The Token is already expired.";
        public const string TokenInvalid = "The Token is invalid.";
        public const string Validated = "Validated.";

        // Client
        public const string GrantTypeInValid = "Grant Type is invalid.";

        public const string SecretInValid = "Secret is invalid.";

        // User
        public const string UserPasswordWrong = "Password is wrong.";

        public const string UserBanned = "User is banned.";
        public const string UserInActive = "User is in-active.";

        // Access Token
        public const string AuthCodeInValid = "The Code is invalid.";

        // App
        public const string AppBanned = "App is banned.";

        // Update callback Balance
        public const string UserInvalid = "Username invalid.";

        //Time
        public const string EndTimeGreaterStartTimeInvalid = "EndDate must be greater than StartDate";

        // Role
        public const string RoleInvalid = "Role invalid.";

    }
}
