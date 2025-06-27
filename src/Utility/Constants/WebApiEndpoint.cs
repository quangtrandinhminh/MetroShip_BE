namespace MetroShip.Utility.Constants
{
    public static class WebApiEndpoint
    {
        private const string AreaName = "api";

        public static class Authentication
        {
            private const string BaseEndpoint = "/" + AreaName + "/auth";
            public const string Hello = BaseEndpoint + "/hello";
            public const string Register = BaseEndpoint + "/register";
            public const string Login = BaseEndpoint + "/authentication";
            public const string GetAdminId = BaseEndpoint + "/admin-id";
        }

        public static class User
        {
            private const string BaseEndpoint = "/" + AreaName + "/users";
            public const string GetUsers = BaseEndpoint;
            public const string GetUser = BaseEndpoint + "/{id}";
            public const string CreateUser = BaseEndpoint;
            public const string UpdateUser = BaseEndpoint;
            public const string DeleteUser = BaseEndpoint + "/{id}";
            public const string GetUserRoles = BaseEndpoint + "/roles";
        }

        public static class ParcelCategory
        {
            private const string BaseEndpoint = "/" + AreaName + "/parcel-category";
            public const string GetCategories = BaseEndpoint;
            public const string GetCategory = BaseEndpoint + "/{id}";
            public const string CreateCategory = BaseEndpoint;
            public const string UpdateCategory = BaseEndpoint;
            public const string DeleteCategory = BaseEndpoint + "/{id}";
        }

        public static class ParcelEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/parcels";
            public const string GetParcels = BaseEndpoint;
            public const string GetParcelById = BaseEndpoint + "/{id}";
            public const string CreateShipmentItem = BaseEndpoint;
            public const string UpdateShipmentItem = BaseEndpoint;
            public const string DeleteShipmentItem = BaseEndpoint + "/{id}";
            public const string ConfirmParcel = BaseEndpoint + "/staff/confirmation/{parcelId}";
            public const string RejectParcel = BaseEndpoint + "/staff/rejection/{parcelId}";
        }

        public static class ShipmentEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/shipments";
            public const string GetShipments = BaseEndpoint;
            public const string GetShipmentByTrackingCode = BaseEndpoint + "/{shipmentTrackingCode}";
            public const string GetShipmentsHistory = BaseEndpoint + "/customer/history";
            public const string CreateShipment = BaseEndpoint;
            public const string GetShipmentItinerary = BaseEndpoint + "/itinerary";
            public const string UpdateShipmentStatus = BaseEndpoint + "/{shipmentId}/status";
            public const string GetShipmentsByStatus = BaseEndpoint + "/status";
            public const string CustomerChangeShipmentStatus = BaseEndpoint + "customer/{shipmentId}";
            public const string CustomerCancelShipment = BaseEndpoint + "customer/{shipmentId}";
            public const string CreateTransactionVnPay = BaseEndpoint + "/vnpay/payment-url";
            public const string VnpayExecute = BaseEndpoint + "/vnpay/payment-execute";
            public const string GetTotalPrice = BaseEndpoint + "/total-price-itinerary";
            public const string GetShipmentsByLineAndDate = BaseEndpoint + "/metroline/{lineCode}/date/{date}";
            public const string GetAvailableTimeSlots = BaseEndpoint + "/available-time-slots";
        }

        public static class TransactionEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/transactions";
            public const string GetTransactions = BaseEndpoint;
            public const string GetTransactionById = BaseEndpoint + "/{id}";
            public const string CreateTransaction = BaseEndpoint;
            public const string UpdateTransaction = BaseEndpoint + "/{id}";
            public const string DeleteTransaction = BaseEndpoint + "/{id}";
            public const string GetTransactionsByShipmentId = BaseEndpoint + "/shipment/{shipmentId}";
        }

        public static class Notification
        {
            private const string BaseEndpoint = "/" + AreaName + "notifications";

            public const string GetNotifications = BaseEndpoint;
            public const string GetNotification = $"{BaseEndpoint}/{{id}}";
            public const string CreateNotification = BaseEndpoint;
            public const string UpdateNotification = BaseEndpoint;
            public const string DeleteNotification = $"{BaseEndpoint}/{{id}}";
            public const string GetUnreadCount = $"{BaseEndpoint}/unread-count";
            public const string MarkAsRead = $"{BaseEndpoint}/{{id}}/read";
            public const string MarkAllAsRead = $"{BaseEndpoint}/read-all";
            public const string CreateCartNotification = $"{BaseEndpoint}/cart";
            public const string SendNotificationToAllUsers = $"{BaseEndpoint}/send-to-all";
            public const string GetBroadcastNotifications = $"{BaseEndpoint}/broadcasts";
        }

        public static class UserDevice
        {
            private const string BaseEndpoint = "/" + AreaName + "user-devices";
            public const string RegisterDevice = $"{BaseEndpoint}/register";
            public const string UnregisterDevice = $"{BaseEndpoint}/unregister";
            public const string GetUserDevices = BaseEndpoint;
        }

        public static class StationEndpoint 
        {
            private const string BaseEndpoint = "/" + AreaName + "/stations";
            public const string GetStations = BaseEndpoint;
            public const string GetStationById = BaseEndpoint + "/{id}";
            public const string CreateStation = BaseEndpoint;
            public const string UpdateStation = BaseEndpoint;
            public const string DeleteStation = BaseEndpoint + "/{id}";
        }
        public static class MetroLineEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/metro-lines";
            public const string GetMetroLinesDropdownList = BaseEndpoint + "/dropdown";
            public const string GetMetroLineById = BaseEndpoint + "/{id}";
            public const string CreateMetroLine = BaseEndpoint;
            public const string UpdateMetroLine = BaseEndpoint;
            public const string DeleteMetroLine = BaseEndpoint + "/{id}";
            public const string GetMetroLinesByRegion = BaseEndpoint + "/region";
        }
        public static class MetroTimeSlotEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/metro-time-slots";
            public const string GetMetroTimeSlots = BaseEndpoint;
            public const string GetMetroTimeSlotById = BaseEndpoint + "/{id}";
            public const string CreateMetroTimeSlot = BaseEndpoint;
            public const string UpdateMetroTimeSlot = BaseEndpoint;
            public const string DeleteMetroTimeSlot = BaseEndpoint + "/{id}";
        }

        public static class MetroTrainEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/metro-trains";
            public const string GetAllTrains = BaseEndpoint;
            public const string GetTrainById = BaseEndpoint + "/{id}";
            public const string CreateTrain = BaseEndpoint;
            public const string UpdateTrain = BaseEndpoint + "/{id}";
            public const string DeleteTrain = BaseEndpoint + "/{id}";
            public const string GetTrainsByLineId = BaseEndpoint + "/line/{lineId}";
        }

        public static class MediaEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/media";
            public const string UploadImage = BaseEndpoint + "/image";
            public const string DeleteImage = BaseEndpoint + "/image";
        }

        public static class ReportEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/reports";
            public const string GetReports = BaseEndpoint;
            public const string GetReportById = BaseEndpoint + "/{id}";
            public const string CreateReport = BaseEndpoint;
            public const string UpdateReport = BaseEndpoint + "/{id}";
            public const string DeleteReport = BaseEndpoint + "/{id}";
        }

        public static class SystemConfigEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/system-configs";
            public const string GetSystemConfigs = BaseEndpoint;
            public const string GetSystemConfigById = BaseEndpoint + "/{id}";
            public const string CreateSystemConfig = BaseEndpoint;
            public const string UpdateSystemConfig = BaseEndpoint + "/{id}";
            public const string DeleteSystemConfig = BaseEndpoint + "/{id}";
        }
    }
}