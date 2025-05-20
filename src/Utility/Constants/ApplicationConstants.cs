namespace MetroShip.Utility.Constants
{
    public class ApplicationConstants
    {
        public const string KEYID_EXISTED = "KeyId {0} already exists.";
        public const string KeyId = "KeyId";
        public const string DUPLICATE = "System_id is duplicated.";
    }

    public class HttpResponseCodeConstants
    {
        public const string NOT_FOUND = "Not found!";
        public const string BAD_REQUEST = "Bad request!";
        public const string SUCCESS = "Success!";
        public const string FAILED = "Failed!";
        public const string EXISTED = "Existed!";
        public const string DUPLICATE = "Duplicate!";
        public const string INTERNAL_SERVER_ERROR = "Internal server error!";
        public const string INVALID_INPUT = "Invalid input!";
        public const string UNAUTHORIZED = "Unauthorized!";
        public const string FORBIDDEN = "Forbidden!";
        public const string EXPIRED = "Expired!";
    }
    public class ResponseMessageConstantsCommon
    {
        public const string NOT_FOUND = "Data not found.";
        public const string EXISTED = "Already existed!";
        public const string SUCCESS = "Operation successful.";
        public const string NO_DATA = "No data returned.";
        public const string SERVER_ERROR = "Server error, please contact the development team.";
        public const string DATE_WRONG_FORMAT = "Date format is incorrect, expected yyyy-mm-dd.";
        public const string DATA_NOT_ENOUGH = "Input data is incomplete.";
    }

    public class ResponseMessageIdentity
    {
        public const string INVALID_USER = "User does not exist.";
        public const string UNAUTHENTICATED = "Unauthenticated.";
        public const string PASSWORD_NOT_MATCH = "Passwords do not match.";
        public const string PASSWORD_WRONG = "Incorrect password.";
        public const string EXISTED_USER = "User already exists.";
        public const string EXISTED_EMAIL = "Email already exists.";
        public const string EXISTED_PHONE = "Phone number already exists.";
        public const string OTP_INVALID = "OTP is invalid.";
        public const string OTP_EXPIRED = "OTP is invalid or expired.";
        public const string OTP_INVALID_OR_EXPIRED = "Token is invalid or expired.";
        public const string GOOGLE_TOKEN_INVALID = "Invalid Google token.";
        public const string EMAIL_VALIDATED = "Email has been validated.";
        public const string PHONE_VALIDATED = "Phone number has been validated.";
        public const string ROLE_INVALID = "Roles are invalid.";
        public const string CLAIM_NOTFOUND = "Claim not found.";
        public const string EXISTED_ROLE = "Role already exists.";

        public const string USERNAME_REQUIRED = "Username cannot be empty.";
        public const string NAME_REQUIRED = "Name cannot be empty.";
        public const string USERCODE_REQUIRED = "User code cannot be empty.";
        public const string PASSWORD_REQUIRED = "Password cannot be empty.";
        public const string PASSSWORD_LENGTH = "Password must be at least 5 characters.";
        public const string CONFIRM_PASSWORD_REQUIRED = "Confirm password cannot be empty.";
        public const string EMAIL_REQUIRED = "Email cannot be empty.";
        public const string PHONENUMBER_REQUIRED = "Phone number cannot be empty.";
        public const string PHONENUMBER_INVALID = "Phone number is invalid.";
        public const string PHONENUMBER_LENGTH = "Phone number must be exactly 10 digits.";
        public const string ROLES_REQUIRED = "Role cannot be empty.";
        public const string USER_NOT_ALLOWED = "You do not have permission to access this section.";
        public const string EMAIL_VALIDATION_REQUIRED = "Please enter the OTP code sent to your email to activate your account.";
    }

    public class ResponseMessageIdentitySuccess
    {
        public const string REGIST_USER_SUCCESS = "Account registration successful! Please verify your email to activate your account.";
        public const string VERIFY_PHONE_SUCCESS = "Phone number verification successful!";
        public const string VERIFY_EMAIL_SUCCESS = "Email verification successful!";
        public const string FORGOT_PASSWORD_SUCCESS = "Password reset request successful, please check your email.";
        public const string RESET_PASSWORD_SUCCESS = "Password reset successful!";
        public const string CHANGE_PASSWORD_SUCCESS = "Password change successful!";
        public const string RESEND_EMAIL_SUCCESS = "Resent verification email successfully!";
        public const string UPDATE_USER_SUCCESS = "User information updated successfully!";
        public const string DELETE_USER_SUCCESS = "User deleted successfully!";
        public const string ADD_ROLE_SUCCESS = "Role added successfully!";
        public const string UPDATE_ROLE_SUCCESS = "Role updated successfully!";
        public const string DELETE_ROLE_SUCCESS = "Role deleted successfully!";
    }

    // Response message constants for entities: not found, existed, update success, delete success
    public class ResponseMessageConstantsUser
    {
        public const string USER_NOT_FOUND = "User not found.";
        public const string USER_EXISTED = "User already exists.";
        public const string ADD_USER_SUCCESS = "User added successfully.";
        public const string UPDATE_USER_SUCCESS = "User updated successfully.";
        public const string DELETE_USER_SUCCESS = "User deleted successfully.";
        public const string ADMIN_NOT_FOUND = "Administrator not found.";
        public const string CUSTOMER_NOT_FOUND = "Customer not found.";
    }

    public class ResponseMessageConstrantsImage
    {
        public const string INVALID_IMAGE = "Hình ảnh không hợp lệ. ";
        public const string INVALID_SIZE = "Kích thước hình ảnh không hợp lệ. ";
        public const string INVALID_FORMAT = "Định dạng hình ảnh không hợp lệ. ";
        public const string INVALID_URL = "Đường dẫn hình ảnh không hợp lệ. ";
    }

    public static class ResponseMessageConstantsParcelCategory
    {
        public const string NOT_FOUND = "Parcel category not found.";
        public const string UPDATE_SUCCESS = "Parcel category updated successfully.";
        public const string DELETE_SUCCESS = "Parcel category deleted successfully.";
    }
}