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
        public const string USER_ID_REQUIRED = "User Id không được trống.";
        public const string USER_ID_INVALID = "User Id không hợp lệ.";
        public const string INVALID_USER = "Người dùng không tồn tại.";
        public const string UNAUTHENTICATED = "Chưa xác thực.";
        public const string PASSWORD_NOT_MATCH = "Mật khẩu xác nhận không khớp.";
        public const string PASSWORD_WRONG = "Mật khẩu không đúng.";
        public const string EXISTED_USER = "Tên người dùng đã tồn tại.";
        public const string EXISTED_EMAIL = "Email đã tồn tại.";
        public const string EXISTED_PHONE = "Số điện thoại đã tồn tại.";
        public const string OTP_INVALID = "Mã OTP không hợp lệ.";
        public const string OTP_EXPIRED = "Mã OTP không hợp lệ hoặc đã hết hạn.";
        public const string OTP_INVALID_OR_EXPIRED = "Token không hợp lệ hoặc đã hết hạn.";
        public const string GOOGLE_TOKEN_INVALID = "Token Google không hợp lệ.";
        public const string EMAIL_VALIDATED = "Email đã được xác thực.";
        public const string PHONE_VALIDATED = "Số điện thoại đã được xác thực.";
        public const string ROLE_INVALID = "Vai trò không hợp lệ.";
        public const string BIRTHDATE_INVALID = "Ngày sinh không hợp lệ, phải ở trong quá khứ.";
        public const string CLAIM_NOTFOUND = "Không tìm thấy quyền.";
        public const string EXISTED_ROLE = "Vai trò đã tồn tại.";
        public const string REFRESH_TOKEN_INVALID = "Refresh token không hợp lệ hoặc đã được sử dụng";
        public const string REFRESH_TOKEN_EXPIRED = "Refresh token đã hết hạn.";

        public const string USERNAME_REQUIRED = "Tên người dùng không được để trống.";
        public const string USERNAME_INVALID = "Tên người dùng không được chứa ký tự đặc biệt hoặc khoảng trắng.";
        public const string NAME_REQUIRED = "Tên không được để trống.";
        public const string NAME_INVALID = "Tên không được chứa số.";
        public const string USERCODE_REQUIRED = "UserId không được để trống.";
        public const string PASSWORD_REQUIRED = "Mật khẩu không được để trống.";
        public const string PASSSWORD_LENGTH = "Mật khẩu phải có ít nhất 8 ký tự.";
        public const string CONFIRM_PASSWORD_REQUIRED = "Mật khẩu xác nhận không được để trống.";
        public const string EMAIL_REQUIRED = "Email không được để trống.";
        public const string EMAIL_INVALID = "Email không hợp lệ.";
        public const string PHONENUMBER_REQUIRED = "Số điện thoại không được để trống.";
        public const string PHONENUMBER_INVALID = "Số điện thoại không hợp lệ.";
        public const string PHONENUMBER_LENGTH_INVALID = "Số điện thoại phải đúng 10 chữ số.";
        public const string ROLES_REQUIRED = "Vai trò không được để trống.";
        public const string USER_NOT_ALLOWED = "Bạn không có quyền truy cập mục này.";
        public const string EMAIL_VALIDATION_REQUIRED = "Vui lòng nhập mã OTP gửi đến email để kích hoạt tài khoản.";

        public const string BANKID_REQUIRED = "BankId không được để trống.";
        public const string BANKID_INVALID = "BankId không hợp lệ.";
        public const string ACCOUNTNO_INVALID = "Số tài khoản không hợp lệ.";
        public const string ACCOUNTNAME_INVALID = "Tên tài khoản viết hoa không dấu.";
        public const string ACCOUNTNO_REQUIRED = "Số tài khoản không được để trống.";
        public const string ACCOUNTNAME_REQUIRED = "Tên tài khoản không được để trống.";
        public const string ACCOUNTNO_LENGTH_INVALID = "Số tài khoản từ 10 đến 19 ký tự.";
        public const string ACCOUNTNAME_LENGTH_INVALID = "Tên tài khoản không được vượt quá 255 ký tự. ";
    }

    public class ResponseMessageIdentitySuccess
    {
        public const string REGIST_USER_SUCCESS = "Đăng ký tài khoản thành công! Vui lòng xác minh email để kích hoạt tài khoản.";
        public const string VERIFY_PHONE_SUCCESS = "Xác thực số điện thoại thành công!";
        public const string VERIFY_EMAIL_SUCCESS = "Xác thực email thành công!";
        public const string FORGOT_PASSWORD_SUCCESS = "Yêu cầu đặt lại mật khẩu thành công, vui lòng kiểm tra email.";
        public const string RESET_PASSWORD_SUCCESS = "Đặt lại mật khẩu thành công!";
        public const string CHANGE_PASSWORD_SUCCESS = "Thay đổi mật khẩu thành công!";
        public const string RESEND_EMAIL_SUCCESS = "Đã Gửi lại mã xác thực qua email.";
        public const string UPDATE_USER_SUCCESS = "Cập nhật thông tin người dùng thành công!";
        public const string DELETE_USER_SUCCESS = "Xóa người dùng thành công!";
        public const string ADD_ROLE_SUCCESS = "Thêm vai trò thành công!";
        public const string UPDATE_ROLE_SUCCESS = "Cập nhật vai trò thành công!";
        public const string DELETE_ROLE_SUCCESS = "Xóa vai trò thành công!";
    }

    public class ResponseMessageConstantsUser
    {
        public const string USER_NOT_FOUND = "Không tìm thấy người dùng.";
        public const string USER_EXISTED = "Người dùng đã tồn tại";
        public const string CREATE_USER_SUCCESS = "Thêm người dùng thành công.";
        public const string UPDATE_USER_SUCCESS = "Cập nhật người dùng thành công.";
        public const string BAN_USER_SUCCESS = "Khóa người dùng thành công.";
        public const string ADMIN_NOT_FOUND = "Không tìm thấy quản trị viên.";
        public const string CUSTOMER_NOT_FOUND = "Không tìm thấy khách hàng.";
        public const string ASSIGN_ROLE_SUCCESS = "Gán vai trò thành công.";
        public const string UPDATE_BANK_INFO_SUCCESS = "Cập nhật thông tin ngân hàng thành công.";
        public const string STAFF_CANNOT_CONFIRM_AT_STATION = "Nhân viên không thể xác nhận bưu kiện tại ga ";
    }

    public class ResponseMessageConstrantsImage
    {
        public const string INVALID_IMAGE = "Hình ảnh không hợp lệ.";
        public const string INVALID_SIZE = "Kích thước hình ảnh không hợp lệ.";
        public const string INVALID_FORMAT = "Định dạng hình ảnh không hợp lệ.";
        public const string INVALID_URL = "Đường dẫn không hợp lệ.";
    }

    public static class ResponseMessageConstantsParcelCategory
    {
        public const string NOT_FOUND = "Không tìm thấy loại bưu kiện.";
        public const string UPDATE_SUCCESS = "Cập nhật loại bưu kiện thành công.";
        public const string DELETE_SUCCESS = "Xóa loại bưu kiện thành công.";
        public const string CREATE_SUCCESS = "Tạo loại bưu kiện thành công.";
    }

    public class ResponseMessageShipment
    {
        public const string SHIPMENT_NOT_FOUND = "Không tìm thấy đơn hàng.";
        public const string SHIPMENT_EXISTED = "Đơn hàng đã tồn tại.";
        public const string SHIPMENT_UPDATE_SUCCESS = "Cập nhật đơn hàng thành công.";
        public const string SHIPMENT_DELETE_SUCCESS = "Xóa đơn hàng thành công.";
        public const string SHIPMENT_CREATE_SUCCESS = "Tạo đơn hàng thành công.";
        public const string SHIPMENT_CANCEL_SUCCESS = "Hủy đơn hàng thành công.";
        public const string SENDER_ID_REQUIRED = "Yêu cầu userId người gửi.";
        public const string SENDER_NAME_REQUIRED = "Yêu cầu tên người gửi.";
        public const string SENDER_PHONE_REQUIRED = "Yêu cầu số điện thoại người gửi.";
        public const string RECIPIENT_NAME_REQUIRED = "Yêu cầu tên người nhận.";
        public const string RECIPIENT_PHONE_REQUIRED = "Yêu cầu số điện thoại người nhận.";
        public const string RECIPIENT_NATIONAL_ID_REQUIRED = "Yêu cầu CCCD người nhận.";
        public const string RECIPIENT_NATIONAL_ID_INAVLID = "CCCD người nhận phải từ 9 đến 12 chữ số.";
        public const string TOTAL_COST_VND_REQUIRED = "Yêu cầu tổng chi phí (VND).";
        public const string TOTAL_COST_VND_INVALID = "Tổng chi phí phải lớn hơn hoặc bằng 0.";
        public const string INSURANCE_FEE_VND_INVALID = "Phí bảo hiểm (VND) không hợp lệ.";
        public const string SHIPPING_FEE_VND_REQUIRED = "Yêu cầu phí vận chuyển (VND).";
        public const string SHIPPING_FEE_VND_INVALID = "Phí vận chuyển (VND) phải lớn hơn hoặc bằng 0.";
        public const string SHIPMENT_STATUS_INVALID = "Trạng thái đơn hàng không hợp lệ.";
        public const string SHIPMENT_STATUS_NOT_FOUND = "Không tìm thấy trạng thái đơn hàng.";
        public const string SHIPMENT_STATUS_UPDATE_SUCCESS = "Cập nhật trạng thái đơn hàng thành công.";
        public const string START_RECEIVE_AT_REQUIRED = "Yêu cầu thời gian bắt đầu nhận.";
        public const string START_RECEIVE_AT_INVALID = "Thời gian bắt đầu nhận hàng phải trước ngày giờ hạn chót gửi hàng.";
        public const string SHIPMENT_DATE_REQUIRED = "Yêu cầu ngày giờ hạn chót gửi hàng.";
        public const string DEPARTURE_STATION_ID_REQUIRED = "Yêu cầu StationId ga đi.";
        public const string DEPARTURE_STATION_ID_INVALID = "StationId ga đi không hợp lệ.";
        public const string DEPARTURE_STATION_NOT_FOUND = "Không tìm thấy ga đi.";
        public const string DESTINATION_STATION_ID_REQUIRED = "Yêu cầu stationId ga đến.";
        public const string DESTINATION_STATION_ID_INVALID = "StationId ga đến không hợp lệ.";
        public const string DESTINATION_STATION_NOT_FOUND = "Không tìm thấy ga đến.";
        public const string PATH_NOT_FOUND = "Không tìm thấy đường đi giữa hai ga.";
        public const string USER_COORDINATE_REQUIRED = "Yêu cầu tọa độ người dùng (vĩ độ và kinh độ) hoặc StationId ga đi.";
        public const string USER_COORDINATE_INVALID = "Tọa độ người dùng (vĩ độ và kinh độ) không hợp lệ.";
        public const string SHIFT_REQUIRED = "Yêu cầu ca làm việc.";
        public const string SHIFT_INVALID = "Ca làm việc không hợp lệ.";
        public const string SHIPMENT_ALREADY_CONFIRMED = "Đơn hàng phải ở trạng thái 'Chờ gửi hàng' để xác nhận. Vui lòng kiểm tra trạng thái đơn hàng trước khi xác nhận.";
        public const string SHIPMENT_CANNOT_CANCEL = "Đơn hàng phải ở trạng thái 'Chờ gửi hàng' hoặc 'Chờ thanh toán' để hủy.";
        public const string SHIPMENT_ITINERARY_NOT_SCHEDULED = "Lịch trình đơn hàng chưa được lên lịch. Vui lòng lên lịch trước khi xác nhận đơn hàng.";
        public const string SCHEDULED_SHIFT_INVALID = "Ca làm việc lên lịch không hợp lệ. Vui lòng chọn ca phù hợp cho đơn hàng.";
        public const string TIME_SLOT_NOT_FOUND = "Không tìm thấy khung giờ.";
        public const string TOTAL_KM_REQUIRED = "Yêu cầu tổng số km.";
        public const string TOTAL_KM_INVALID = "Tổng số km phải lớn hơn 0.";
        public const string PICKED_UP_IMAGE_LINK_REQUIRED = "Yêu cầu đường dẫn ảnh lấy hàng.";
        public const string PICKED_UP_IMAGE_LINK_INVALID = "Đường dẫn ảnh lấy hàng không hợp lệ.";
        public const string DELIVERED_IMAGE_LINK_REQUIRED = "Yêu cầu đường dẫn ảnh giao hàng.";
        public const string DELIVERED_IMAGE_LINK_INVALID = "Đường dẫn ảnh giao hàng không hợp lệ.";
        public const string PICKED_UP_SUCCESS = "Lấy hàng thành công.";
        public const string DELIVERED_SUCCESS = "Giao hàng thành công.";
        public const string REJECTED_SUCCESS = "Từ chối đơn hàng thành công.";
        public const string CANCELLED_SUCCESS = "Hủy đơn hàng thành công.";
        public const string SHIPMENT_NOT_COMPLETED = "Đơn hàng chưa hoàn thành.";
        public const string FEEDBACK_TEXT_TOO_LONG = "Nội dung phản hồi không được vượt quá 500 ký tự.";
        public const string RATING_INVALID = "Đánh giá phải từ 1 đến 5.";
        public const string SHIPMENT_ID_REQUIRED = "Yêu cầu ShipmentId.";
        public const string SHIPMENT_ID_INVALID = "ShipmentId không hợp lệ.";
        public const string RATE_SUCCESS = "Gửi đánh giá thành công.";
        public const string COMPLETED_SUCCESS = "Hoàn thành đơn hàng thành công.";
        public const string SHIPMENT_NOT_AWAITING_DELIVERY = "Đơn hàng không ở trạng thái 'Chờ giao hàng'.";
        public const string SHIPMENT_PICKUP_OUT_OF_TIME_RANGE = "Đơn hàng phải được xác nhận trong khung thời gian Gửi hàng";
        public const string SHIPMENT_CANNOT_BE_RETURNED = "Đơn hàng ở trạng thái chờ giao hàng hoặc đang tính phụ phí mới có thể yêu cầu trả hàng. ";
        public const string SHIPMENT_ALREADY_TO_RETURN = "Đơn hàng đã được lên lịch trả hàng. ";
        public const string SURCHARGE_APPLIED_SUCCESS = "Áp dụng phụ phí thành công.";
        public const string EXPIRED_APPLIED_SUCCESS = "Áp dụng quá hạn thành công.";
    }

    public class ResponseMessageItinerary
    {
        public const string ITINERARY_REQUIRED = "Yêu cầu gửi lộ trình.";
        public const string ROUTE_ID_REQUIRED = "Yêu cầu RouteId.";
        public const string LEG_ORDER_REQUIRED = "Yêu cầu thứ tự chặng.";
        public const string LEG_ORDER_INVALID = "Thứ tự chặng phải lớn hơn 0";
        public const string EST_MINUTES_INVALID = "Số phút dự kiến phải lớn hơn 0";
        public const string BASE_PRICE_VND_PER_KM_REQUIRED = "Yêu cầu giá cơ bản mỗi km (VND).";
        public const string BASE_PRICE_VND_PER_KM_INVALID = "Giá cơ bản mỗi km (VND) phải lớn hơn hoặc bằng 0.";
        public const string ITINERARY_NOT_FOUND = "Không tìm thấy lộ trình.";
        public const string INVALID_NEW_TIME_SLOT = "Ngày hoặc khung giờ mục tiêu không hợp lệ. Không được sớm hơn lịch ban đầu";
    }

    public class ResponseMessageParcel
    {
        public const string PARCEL_REQUIRED = "Yêu cầu bưu kiện";
        public const string PARCEL_NOT_FOUND = "Không tìm thấy bưu kiện.";
        public const string PARCEL_EXISTED = "Bưu kiện đã tồn tại.";
        public const string PARCEL_UPDATE_SUCCESS = "Cập nhật bưu kiện thành công.";
        public const string PARCEL_DELETE_SUCCESS = "Xóa bưu kiện thành công.";
        public const string PARCEL_CREATE_SUCCESS = "Tạo bưu kiện thành công.";
        public const string PARCEL_CANCEL_SUCCESS = "Hủy bưu kiện thành công.";
        public const string PARCEL_STATUS_INVALID = "Trạng thái bưu kiện không hợp lệ.";
        public const string PARCEL_STATUS_NOT_FOUND = "Không tìm thấy trạng thái bưu kiện.";
        public const string PARCEL_STATUS_UPDATE_SUCCESS = "Cập nhật trạng thái bưu kiện thành công.";
        public const string PARCEL_CATEGORY_ID_REQUIRED = "Yêu cầu parcelCategoryId.";
        public const string WIDTH_REQUIRED = "Yêu cầu chiều rộng.";
        public const string HEIGHT_REQUIRED = "Yêu cầu chiều cao.";
        public const string LENGTH_REQUIRED = "Yêu cầu chiều dài.";
        public const string WEIGHT_REQUIRED = "Yêu cầu trọng lượng.";
        public const string WEIGHT_INVALID = "Trọng lượng phải lớn hơn 0.";
        public const string HEIGHT_INVALID = "Chiều cao phải lớn hơn 0.";
        public const string WIDTH_INVALID = "Chiều rộng phải lớn hơn 0.";
        public const string LENGTH_INVALID = "Chiều dài phải lớn hơn 0.";
        public const string IS_BULK_REQUIRED = "Yêu cầu thông tin hàng cồng kềnh";
        public const string IS_BULK_INVALID = "Thông tin hàng cồng kềnh phải là true hoặc false";
        public const string PARCEL_CATEGORY_ID_INVALID = "ParcelCategoryId không hợp lệ.";
        public const string CHARGEABLE_WEIGHT_INVALID = "Trọng lượng tính phí phải lớn hơn 0.";
        public const string SHIPPING_FEE_VND_INVALID = "Phí vận chuyển (VND) phải lớn hơn hoặc bằng 0.";
        public const string INSURANCE_FEE_VND_INVALID = "Phí bảo hiểm (VND) phải lớn hơn hoặc bằng 0.";
        public const string PRICE_VND_INVALID = "Giá (VND) phải lớn hơn hoặc bằng 0.";
        public const string VALUE_VND_INVALID = "Giá trị (VND) phải lớn hơn 0.";
        public const string CATEGORY_INSURANCE_ID_REQUIRED = "Yêu cầu CategoryInsuranceId.";
        public const string CATEGORY_INSURANCE_ID_INVALID = "CategoryInsuranceId không hợp lệ.";
        public const string PARCEL_ALREADY_LOADED = "Bưu kiện đã lên tàu tại ga này.";
    }

    public class ResponseMessageStation
    {
        public const string STATION_NOT_FOUND = "Không tìm thấy ga.";
        public const string STATION_EXISTED = "Ga đã tồn tại.";
        public const string STATION_UPDATE_SUCCESS = "Cập nhật ga thành công.";
        public const string STATION_DELETE_SUCCESS = "Xóa ga thành công.";
        public const string STATION_CREATE_SUCCESS = "Tạo ga thành công.";
        public const string NO_STATION_NEAR_USER = "Không có ga nào gần người dùng trong bán kính "; 
    }

    public class ResponseMessageRoute
    {
        public const string ROUTE_NOT_FOUND = "Không tìm thấy tuyến.";
        public const string ROUTE_EXISTED = "Tuyến đã tồn tại.";
        public const string ROUTE_UPDATE_SUCCESS = "Cập nhật tuyến thành công.";
        public const string ROUTE_DELETE_SUCCESS = "Xóa tuyến thành công.";
        public const string ROUTE_CREATE_SUCCESS = "Tạo tuyến thành công.";
    }

    public class ResponseMessageTransaction
    {
        public const string TRANSACTION_NOT_FOUND = "Không tìm thấy giao dịch.";
        public const string TRANSACTION_EXISTED = "Giao dịch đã tồn tại.";
        public const string TRANSACTION_UPDATE_SUCCESS = "Cập nhật giao dịch thành công.";
        public const string TRANSACTION_DELETE_SUCCESS = "Xóa giao dịch thành công.";
        public const string TRANSACTION_CREATE_SUCCESS = "Tạo giao dịch thành công.";
    }

    public class ResponseMessageTrain
    {
        public const string TRAIN_NOT_FOUND = "Không tìm thấy tàu.";
        public const string TRAIN_EXISTED = "Mã tàu đã tồn tại: ";
        public const string TRAIN_UPDATE_SUCCESS = "Cập nhật tàu thành công.";
        public const string TRAIN_DELETE_SUCCESS = "Xóa tàu thành công.";
        public const string TRAIN_CREATE_SUCCESS = "Tạo tàu thành công.";
        public const string LINE_ID_REQUIRED = "Yêu cầu LineId.";
        public const string TIME_SLOT_ID_REQUIRED = "Yêu cầu TimeSlotId.";
        public const string DATE_REQUIRED = "Yêu cầu ngày.";
        public const string LINE_ID_INVALID = "LineId không hợp lệ.";
        public const string TIME_SLOT_ID_INVALID = "TimeSlotId không hợp lệ.";
        public const string DATE_INVALID = "Ngày không hợp lệ. Định dạng: yyyy-mm-dd";
        public const string TRAIN_FULL = "Tàu đã đầy, không thể thêm đơn hàng.";
        public const string MODEL_NAME_TOO_LONG = "Tên mẫu tàu không được vượt quá 100 ký tự.";
        public const string IS_AVAILABLE_INVALID = "Thông tin khả dụng phải là true hoặc false.";
        public const string TRAIN_ID_REQUIRED = "Yêu cầu TrainId.";
        public const string TRAIN_ID_INVALID = "TrainId không hợp lệ.";
        public const string ITINERARY_IDS_REQUIRED = "Yêu cầu ShipmentItineraryId.";
        public const string ITINERARY_IDS_INVALID = "ShipmentItineraryId không hợp lệ.";
        public const string TRAIN_ALREADY_ASSIGNED_TO_SLOT_ON_DATE = "Tàu đã được gán vào khung giờ này trong ngày đã chọn.";
        public const string SHIPMENT_ITINERARIES_NOT_FOUND = "Không tìm thấy lịch trình đơn hàng cho tàu đã chọn.";
        public const string TRAIN_MUST_BE_SAME_LINE = "Tàu phải thuộc cùng tuyến với tuyến đường.";
        public const string TRAIN_CODE_INVALID = "Train code không hợp lệ.";
        public const string TRAIN_CODE_TOO_LONG = "Train code không được vượt quá 20 ký tự.";
        public const string MAX_TRAIN_PER_LINE_REACHED = "Tuyến đã đạt đến số lượng tàu tối đa: ";
    }

    public class RegionMessageConstants
    {
        public const string REGION_NOT_FOUND = "Không tìm thấy vùng.";
        public const string REGION_EXISTED = "Vùng đã tồn tại.";
        public const string REGION_UPDATE_SUCCESS = "Cập nhật vùng thành công.";
        public const string REGION_DELETE_SUCCESS = "Xóa vùng thành công.";
        public const string REGION_CREATE_SUCCESS = "Tạo vùng thành công.";
        public const string REGION_CODE_REQUIRED = "Yêu cầu RegionId.";
        public const string REGION_NAME_REQUIRED = "Yêu cầu tên vùng.";
        public const string REGION_CODE_INVALID = "RegionId không hợp lệ.";
        public const string REGION_NAME_INVALID = "Tên vùng không hợp lệ.";
        public const string REGION_NAME_EXISTED = "Tên vùng đã tồn tại.";
    }

    public class MetroRouteMessageConstants
    {
        public const string METROROUTE_EXISTED = "Tuyến đã tồn tại.";
        public const string ROUTE_UPDATE_SUCCESS = "Cập nhật tuyến thành công.";
        public const string ROUTE_DELETE_SUCCESS = "Xóa tuyến thành công.";
        public const string ROUTE_CREATE_SUCCESS = "Tạo tuyến thành công.";
        public const string ROUTE_CODE_REQUIRED = "Yêu cầu RouteId.";
        public const string ROUTE_NAME_REQUIRED = "Yêu cầu tên tuyến.";
        public const string ROUTE_CODE_INVALID = "RouteId không hợp lệ.";
        public const string ROUTE_NAME_INVALID = "Tên tuyến không hợp lệ.";
        public const string METROROUTE_STATION_COUNT_LESS_THAN_2 = "Tuyến metro phải có ít nhất 2 ga.";
        public const string METROROUTE_NOT_FOUND = "Không tìm thấy tuyến metro.";
        public const string METROROUTE_NOT_ENOUGH_TRAINS = "Tuyến yêu cầu ít nhất 2 tàu để sẵn sàng giao hàng";
        public const string METROROUTE_ACTIVATE_SUCCESS = "Kích hoạt tuyến metro thành công.";
        public const string METROROUTE_DEACTIVATE_SUCCESS = "Vô hiệu hóa tuyến metro thành công.";
        public const string METROROUTE_ALREADY_ACTIVATED = "Tuyến metro không tồn tại hoặc đã được kích hoạt.";
    }

    public class ResponseMessageSupportTicket
    {
        public const string TICKET_NOT_FOUND = "Không tìm thấy phiếu hỗ trợ.";
        public const string TICKET_EXISTED = "Phiếu hỗ trợ đã tồn tại.";
        public const string TICKET_UPDATE_SUCCESS = "Cập nhật phiếu hỗ trợ thành công.";
        public const string TICKET_DELETE_SUCCESS = "Xóa phiếu hỗ trợ thành công.";
        public const string TICKET_CREATE_SUCCESS = "Tạo phiếu hỗ trợ thành công.";
        public const string TICKET_CANCEL_SUCCESS = "Hủy phiếu hỗ trợ thành công.";
        public const string TICKET_STATUS_INVALID = "Trạng thái phiếu hỗ trợ không hợp lệ.";
        public const string TICKET_STATUS_NOT_FOUND = "Không tìm thấy trạng thái phiếu hỗ trợ.";
        public const string TICKET_STATUS_UPDATE_SUCCESS = "Cập nhật trạng thái phiếu hỗ trợ thành công.";
    }

    public class ResponseMessageInsurancePolicy
    {
        public const string INSURANCE_POLICY_NOT_FOUND = "Không tìm thấy chính sách bảo hiểm.";
        public const string INSURANCE_POLICY_EXISTED = "Chính sách bảo hiểm đã tồn tại.";
        public const string INSURANCE_POLICY_UPDATE_SUCCESS = "Cập nhật chính sách bảo hiểm thành công.";
        public const string INSURANCE_POLICY_DELETE_SUCCESS = "Xóa chính sách bảo hiểm thành công.";
        public const string INSURANCE_POLICY_CREATE_SUCCESS = "Tạo chính sách bảo hiểm thành công.";
        public const string NAME_REQUIRED = "Tên chính sách bảo hiểm không được để trống.";
        public const string DESCRIPTION_REQUIRED = "Yêu cầu mô tả chính sách bảo hiểm.";
        public const string BASE_FEE_VND_INVALID = "Phí cơ bản (VND) phải lớn hơn hoặc bằng 0.";
        public const string MAX_PARCEL_VALUE_VND_INVALID = "Giá trị hàng hóa tối đa (VND) phải lớn hơn 0.";
        public const string INSURANCE_FEE_RATE_ON_VALUE_INVALID = "Tỷ lệ phí bảo hiểm trên giá trị phải từ 0 đến 1.";
        public const string STANDARD_COMPENSATION_VALUE_VND_INVALID = "Giá trị bồi thường tiêu chuẩn (VND) phải lớn hơn hoặc bằng 0.";
        public const string MAX_COMPENSATION_RATE_ON_VALUE_INVALID = "Tỷ lệ bồi thường tối đa trên giá trị phải từ 0 đến 1.";
        public const string MIN_COMPENSATION_RATE_ON_VALUE_INVALID = "Tỷ lệ bồi thường tối thiểu trên giá trị phải từ 0 đến 1.";
        public const string MIN_COMPENSATION_RATE_ON_SHIPPING_FEE_INVALID = "Tỷ lệ bồi thường tối thiểu trên phí vận chuyển phải lớn hơn 4.";
        public const string VALID_FROM_REQUIRED = "Yêu cầu ngày bắt đầu hiệu lực.";
        public const string VALID_TO_REQUIRED = "Yêu cầu ngày kết thúc hiệu lực.";
        public const string INSURANCE_POLICY_ALREADY_ACTIVATED = "Chính sách bảo hiểm đang được kích hoạt";
        public const string INSURANCE_POLICY_ACTIVATE_SUCCESS = "Kích hoạt chính sách bảo hiểm thành công.";
        public const string INSURANCE_POLICY_DEACTIVATE_SUCCESS = "Vô hiệu hóa chính sách bảo hiểm thành công.";
        public const string INSURANCE_POLICY_ALREADY_DEACTIVATED = "Chính sách bảo hiểm đang được vô hiệu hóa";
        public const string INSURANCE_POLICY_IN_USE = "Chính sách bảo hiểm đang được sử dụng bởi một hoặc nhiều loại bưu kiện. Vui lòng gỡ bỏ liên kết trước khi vô hiệu hóa chính sách.";
        public const string INSURANCE_POLICY_EXPIRED = "Chính sách bảo hiểm đã hết hạn và không thể kích hoạt lại.";
        public const string INSURANCE_POLICY_ACTIVE_CANNOT_DELETE = "Chính sách bảo hiểm đang được kích hoạt và không thể xóa.";
    }

    public class ResponseMessageSystemConfig
    {
        public const string CONFIG_KEY_REQUIRED = "Yêu cầu ConfigKey.";
        public const string CONFIG_KEY_INVALID = "ConfigKey không hợp lệ.";
        public const string CONFIG_NOT_FOUND = "Không tìm thấy cấu hình hệ thống.";
        public const string CONFIG_EXISTED = "Cấu hình hệ thống đã tồn tại.";
        public const string CONFIG_UPDATE_SUCCESS = "Cập nhật cấu hình hệ thống thành công.";
        public const string CONFIG_DELETE_SUCCESS = "Xóa cấu hình hệ thống thành công.";
        public const string CONFIG_CREATE_SUCCESS = "Tạo cấu hình hệ thống thành công.";
        public const string DESCRIPTION_REQUIRED = "Yêu cầu mô tả.";
        public const string CONFIG_TYPE_INVALID = "Loại cấu hình không hợp lệ.";
        public const string MAX_DISTANCE_INVALID = "Khoảng cách tối đa phải là một số nguyên dương.";
        public const string MAX_CAPACITY_INVALID = "Sức chứa tối đa phải là một số nguyên dương ";
        public const string MAX_COUNT_STATION_INVALID = "Số lượng ga tối đa phải là một số nguyên dương.";
        public const string MAX_SCHEDULE_DAY_INVALID = "Số ngày lên lịch tối đa phải là một số nguyên dương.";
        public const string CONFIG_VALUE_SAME = "Giá trị cấu hình không thay đổi.";
        public const string MAX_SHIFT_ATTEMPTS_INVALID = "Số lần thử ca làm việc tối đa phải là một số nguyên dương.";
    }

    public class ResponseMessagePricingConfig
    {
        public const string PRICING_CONFIG_NOT_FOUND = "Không tìm thấy cấu hình giá.";
        public const string PRICING_CONFIG_EXISTED = "Cấu hình giá đã tồn tại.";
        public const string PRICING_CONFIG_UPDATE_SUCCESS = "Cập nhật cấu hình giá thành công.";
        public const string PRICING_CONFIG_DELETE_SUCCESS = "Xóa cấu hình giá thành công.";
        public const string PRICING_CONFIG_CREATE_SUCCESS = "Tạo cấu hình giá thành công.";
        public const string NAME_REQUIRED = "Yêu cầu tên cấu hình giá.";
        public const string DESCRIPTION_REQUIRED = "Yêu cầu mô tả cấu hình giá.";
        public const string BASE_FEE_VND_PER_KM_INVALID = "Phí cơ bản (VND) phải lớn hơn hoặc bằng 0.";
        public const string WEIGHT_FEE_VND_PER_KG_INVALID = "Phí theo trọng lượng (VND) phải lớn hơn hoặc bằng 0.";
        public const string VOLUME_FEE_VND_PER_M3_INVALID = "Phí theo thể tích (VND) phải lớn hơn hoặc bằng 0.";
        public const string BULK_FEE_VND_INVALID = "Phí hàng cồng kềnh (VND) phải lớn hơn hoặc bằng 0.";
        public const string FRAGILE_FEE_VND_INVALID = "Phí hàng dễ vỡ (VND) phải lớn hơn hoặc bằng 0.";
        public const string EXPRESS_FEE_RATE_ON_SHIPPING_FEE_INVALID = "Tỷ lệ phí chuyển phát nhanh trên phí vận chuyển phải từ 0 đến 1.";
        public const string VALID_FROM_REQUIRED = "Yêu cầu ngày bắt đầu hiệu lực.";
        public const string VALID_TO_REQUIRED = "Yêu cầu ngày kết thúc hiệu lực.";
        public const string PRICING_CONFIG_ALREADY_ACTIVATED = "Cấu hình giá đã được kích hoạt";
        public const string PRICING_CONFIG_IN_USE = "Cấu hình giá đã được kích hoạt và không thể chỉnh sửa.";
        public const string PRICING_CONFIG_EXPIRED = "Cấu hình giá đã hết hạn và không thể kích hoạt lại.";
        public const string PRICING_CONFIG_ACTIVATE_SUCCESS = "Kích hoạt cấu hình giá thành công.";
        public const string PRICING_CONFIG_CANNOT_ACTIVATE_ON_UPDATE = "Cấu hình giá không thể kích hoạt trong khi đang sửa đổi.";
    }

    public class ResponseMessageTimeSlot
    {
        public const string TIMESLOT_NOT_FOUND = "Không tìm thấy khung giờ.";
        public const string TIMESLOT_EXISTED = "Khung giờ đã tồn tại.";
        public const string TIMESLOT_UPDATE_SUCCESS = "Cập nhật khung giờ thành công.";
        public const string TIMESLOT_DELETE_SUCCESS = "Xóa khung giờ thành công.";
        public const string TIMESLOT_CREATE_SUCCESS = "Tạo khung giờ thành công.";
        public const string START_TIME_REQUIRED = "Yêu cầu thời gian bắt đầu.";
        public const string END_TIME_REQUIRED = "Yêu cầu thời gian kết thúc.";
        public const string START_TIME_INVALID = "Thời gian bắt đầu không hợp lệ. Định dạng: HH:mm:ss";
        public const string END_TIME_INVALID = "Thời gian kết thúc không hợp lệ. Định dạng: HH:mm:ss";
        public const string OPEN_TIME_MUST_BE_BEFORE_CLOSE_TIME = "Giờ mở ca phải sớm hơn giờ đóng ca.";
        public const string OVERLAPPING_TIMESLOT = "Khung giờ bị trùng lặp với khung giờ hiện có.";
        public const string START_TIME_MUST_BE_BEFORE_CUT_OFF_TIME = "Giờ bắt đầu nhận hàng phải sớm hơn giờ cắt hàng.";
        public const string CUT_OFF_TIME_MUST_BE_BEFORE_OPEN_TIME = "Giờ cắt hàng phải sớm hơn giờ mở ca.";
        public const string START_TIME_MUST_BE_BEFORE_OPEN_TIME = "Giờ bắt đầu nhận hàng phải sớm hơn giờ mở ca.";
        public const string INVALID_NIGHT_SHIFT_TIMES = "Khung giờ ca đêm không hợp lệ. Giờ mở ca phải sau 18:00 và giờ đóng ca phải trước 06:00.";
    }

    public class ResponseMessageAssignment
    {
        public const string ASSIGNMENT_NOT_FOUND = "Không tìm thấy phân công.";
        public const string ASSIGNMENT_EXISTED = "Phân công đã tồn tại.";
        public const string ASSIGNMENT_UPDATE_SUCCESS = "Cập nhật phân công thành công.";
        public const string ASSIGNMENT_DELETE_SUCCESS = "Xóa phân công thành công.";
        public const string ASSIGNMENT_CREATE_SUCCESS = "Phân công thành công.";
        public const string ASSIGNMENT_ALREADY_INACTIVE = "Phân công đã không còn hiệu lực.";
        public const string ASSIGNMENT_DEACTIVATE_SUCCESS = "Hủy phân công thành công.";
        public const string ASSIGNMENT_STATION_ID_REQUIRED = "Yêu cầu StationId.";
        public const string ASSIGNMENT_STAFF_ID_REQUIRED = "Yêu cầu StaffId.";
        public const string ASSIGNMENT_TRAIN_ID_REQUIRED = "Yêu cầu TrainId.";
        public const string ASSIGNMENT_STATION_ID_NOT_ALLOWED = "StationId không được phép.";
        public const string ASSIGNMENT_TRAIN_ID_NOT_ALLOWED = "StaffId không được phép.";
    }
}