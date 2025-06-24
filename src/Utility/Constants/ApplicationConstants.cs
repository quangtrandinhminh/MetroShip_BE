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
        public const string USER_ID_REQUIRED = "User ID is required.";
        public const string USER_ID_INVALID = "User ID is invalid.";
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
        public const string BIRTHDATE_INVALID = "Birth date is invalid, must be in the past.";
        public const string CLAIM_NOTFOUND = "Claim not found.";
        public const string EXISTED_ROLE = "Role already exists.";

        public const string USERNAME_REQUIRED = "Username cannot be empty.";
        public const string USERNAME_INVALID = "Username cannot contain special characters or spaces.";
        public const string NAME_REQUIRED = "Name cannot be empty.";
        public const string NAME_INVALID = "Name cannot contain numbers.";
        public const string USERCODE_REQUIRED = "User code cannot be empty.";
        public const string PASSWORD_REQUIRED = "Password cannot be empty.";
        public const string PASSSWORD_LENGTH = "Password must be at least 8 characters.";
        public const string CONFIRM_PASSWORD_REQUIRED = "Confirm password cannot be empty.";
        public const string EMAIL_REQUIRED = "Email cannot be empty.";
        public const string EMAIL_INVALID = "Email is invalid.";
        public const string PHONENUMBER_REQUIRED = "Phone number cannot be empty.";
        public const string PHONENUMBER_INVALID = "Phone number is invalid.";
        public const string PHONENUMBER_LENGTH_INVALID = "Phone number must be exactly 10 digits.";
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
        public const string USER_EXISTED = "User already exists";
        public const string CREATE_USER_SUCCESS = "User added successfully.";
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
    public class ResponseMessageShipment
    {
        public const string SHIPMENT_NOT_FOUND = "Shipment not found.";
        public const string SHIPMENT_EXISTED = "Shipment already exists.";
        public const string SHIPMENT_UPDATE_SUCCESS = "Shipment updated successfully.";
        public const string SHIPMENT_DELETE_SUCCESS = "Shipment deleted successfully.";
        public const string SHIPMENT_CREATE_SUCCESS = "Shipment created successfully.";
        public const string SHIPMENT_CANCEL_SUCCESS = "Shipment canceled successfully.";
        public const string SENDER_ID_REQUIRED = "Sender ID is required.";
        public const string SENDER_NAME_REQUIRED = "Sender name is required.";
        public const string SENDER_PHONE_REQUIRED = "Sender phone number is required.";
        public const string RECIPIENT_NAME_REQUIRED = "Recipient name is required.";
        public const string RECIPIENT_PHONE_REQUIRED = "Recipient phone number is required.";
        public const string RECIPIENT_NATIONAL_ID_REQUIRED = "Recipient national ID is required.";
        public const string RECIPIENT_NATIONAL_ID_INAVLID = "Recipient national ID must be 9 to 12 digits.";
        public const string TOTAL_COST_VND_REQUIRED = "Total cost in VND is required.";
        public const string TOTAL_COST_VND_INVALID = "Total cost must be greater than or equal to 0.";
        public const string INSURANCE_FEE_VND_INVALID = "Insurance fee in VND is invalid.";
        public const string SHIPPING_FEE_VND_REQUIRED = "Shipping fee in VND is required.";
        public const string SHIPPING_FEE_VND_INVALID = "Shipping fee in VND must be greater than or equal to 0.";
        public const string SHIPMENT_STATUS_INVALID = "Shipment status is invalid.";
        public const string SHIPMENT_STATUS_NOT_FOUND = "Shipment status not found.";
        public const string SHIPMENT_STATUS_UPDATE_SUCCESS = "Shipment status updated successfully.";
        public const string SHIPMENT_DATE_REQUIRED = "ScheduledDateTime is required.";
        public const string DEPARTURE_STATION_ID_REQUIRED = "Departure station ID is required.";
        public const string DEPARTURE_STATION_ID_INVALID = "Departure station ID is invalid.";
        public const string DEPARTURE_STATION_NOT_FOUND = "Departure station not found.";
        public const string DESTINATION_STATION_ID_REQUIRED = "Destination station ID is required.";
        public const string DESTINATION_STATION_ID_INVALID = "Destination station ID is invalid.";
        public const string DESTINATION_STATION_NOT_FOUND = "Destination station not found.";
        public const string PATH_NOT_FOUND = "Path not found between two stations.";
        public const string USER_COORDINATE_REQUIRED = "User coordinates (latitude and longitude) " +
            "or Departure station ID are required.";
        public const string USER_COORDINATE_INVALID = "User coordinates (latitude and longitude) are invalid.";
        public const string SHIFT_REQUIRED = "Shift is required.";
        public const string SHIFT_INVALID = "Shift is invalid.";
        public const string SHIPMENT_ALREADY_CONFIRMED = "Shipment has already been confirmed.";
        public const string SHIPMENT_ITINERARY_NOT_SCHEDULED = "Shipment itinerary is not scheduled. " +
            "Please schedule the itinerary before confirming the shipment.";
        public const string SCHEDULED_SHIFT_INVALID = "Scheduled shift is invalid. " +
            "Please select a valid shift for the shipment.";
        public const string TIME_SLOT_NOT_FOUND = "Time slot not found.";
        public const string TOTAL_KM_REQUIRED = "Total kilometers are required.";
        public const string TOTAL_KM_INVALID = "Total kilometers must be greater than to 0.";
    }

    public class ResponseMessageItinerary
    {
        public const string ITINERARY_REQUIRED = "Itinerary is required.";
        public const string ROUTE_ID_REQUIRED = "Route ID is required.";
        public const string LEG_ORDER_REQUIRED = "Leg order is required.";
        public const string LEG_ORDER_INVALID = "Leg order must be large than 0";
        public const string EST_MINUTES_INVALID = "Estimated minutes must be large than 0";
        public const string BASE_PRICE_VND_PER_KM_REQUIRED = "Base price per kilometer in VND is required.";
        public const string BASE_PRICE_VND_PER_KM_INVALID = "Base price per kilometer in VND must be greater than or equal to 0.";
    }

    public class ResponseMessageParcel
    {
        public const string PARCEL_REQUIRED = "Parcel is required";
        public const string PARCEL_NOT_FOUND = "Parcel not found.";
        public const string PARCEL_EXISTED = "Parcel already exists.";
        public const string PARCEL_UPDATE_SUCCESS = "Parcel updated successfully.";
        public const string PARCEL_DELETE_SUCCESS = "Parcel deleted successfully.";
        public const string PARCEL_CREATE_SUCCESS = "Parcel created successfully.";
        public const string PARCEL_CANCEL_SUCCESS = "Parcel canceled successfully.";
        public const string PARCEL_STATUS_INVALID = "Parcel status is invalid.";
        public const string PARCEL_STATUS_NOT_FOUND = "Parcel status not found.";
        public const string PARCEL_STATUS_UPDATE_SUCCESS = "Parcel status updated successfully.";
        public const string PARCEL_CATEGORY_ID_REQUIRED = "Parcel category ID is required.";
        public const string WIDTH_REQUIRED = "Width is required.";
        public const string HEIGHT_REQUIRED = "Height is required.";
        public const string LENGTH_REQUIRED = "Length is required.";
        public const string WEIGHT_REQUIRED = "Weight is required.";
        public const string WEIGHT_INVALID = "Weight must be greater than 0.";
        public const string HEIGHT_INVALID = "Height must be greater than 0.";
        public const string WIDTH_INVALID = "Width must be greater than 0.";
        public const string LENGTH_INVALID = "Length must be greater than 0.";
        public const string IS_BULK_REQUIRED = "Is Bulk is required";
        public const string IS_BULK_INVALID = "Is Bulk must be true or false";
        public const string PARCEL_CATEGORY_ID_INVALID = "Parcel category ID is invalid.";
        public const string CHARGEABLE_WEIGHT_INVALID = "Chargeable weight must be greater than 0.";
        public const string SHIPPING_FEE_VND_INVALID = "Shipping fee in VND must be greater than or equal to 0.";
        public const string INSURANCE_FEE_VND_INVALID = "Insurance fee in VND must be greater than or equal to 0.";
        public const string PRICE_VND_INVALID = "Price in VND must be greater than or equal to 0.";
        public const string VALUE_VND_INVALID = "Value in VND must be greater than to 0.";
    }

    public class ResponseMessageStation
    {
        public const string STATION_NOT_FOUND = "Station not found.";
        public const string STATION_EXISTED = "Station already exists.";
        public const string STATION_UPDATE_SUCCESS = "Station updated successfully.";
        public const string STATION_DELETE_SUCCESS = "Station deleted successfully.";
        public const string STATION_CREATE_SUCCESS = "Station created successfully.";
    }

    public class ResponseMessageRoute
    {
        public const string ROUTE_NOT_FOUND = "Route not found.";
        public const string ROUTE_EXISTED = "Route already exists.";
        public const string ROUTE_UPDATE_SUCCESS = "Route updated successfully.";
        public const string ROUTE_DELETE_SUCCESS = "Route deleted successfully.";
        public const string ROUTE_CREATE_SUCCESS = "Route created successfully.";
    }

    public class ResponseMessageTransaction
    {
        public const string TRANSACTION_NOT_FOUND = "Transaction not found.";
        public const string TRANSACTION_EXISTED = "Transaction already exists.";
        public const string TRANSACTION_UPDATE_SUCCESS = "Transaction updated successfully.";
        public const string TRANSACTION_DELETE_SUCCESS = "Transaction deleted successfully.";
        public const string TRANSACTION_CREATE_SUCCESS = "Transaction created successfully.";
    }
}