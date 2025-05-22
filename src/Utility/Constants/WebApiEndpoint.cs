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
            private const string BaseEndpoint = "/" + AreaName + "/user";
            public const string GetUsers = BaseEndpoint;
            public const string GetUser = BaseEndpoint + "/{id}";
            public const string CreateUser = BaseEndpoint;
            public const string UpdateUser = BaseEndpoint;
            public const string DeleteUser = BaseEndpoint + "/{id}";
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
        }

        public static class Parcel
        {
            private const string BaseEndpoint = "/" + AreaName + "/parcels";
            public const string GetParcels = BaseEndpoint;
            public const string GetParcelById = BaseEndpoint + "/{id}";
            public const string CreateShipmentItem = BaseEndpoint;
            public const string UpdateShipmentItem = BaseEndpoint;
            public const string DeleteShipmentItem = BaseEndpoint + "/{id}";
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
    }
}