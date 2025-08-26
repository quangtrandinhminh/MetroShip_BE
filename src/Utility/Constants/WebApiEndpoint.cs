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
            public const string AssignRoleToStaff = BaseEndpoint + "/admin/assign-role";
            public const string GetAssignmentRoles = BaseEndpoint + "/assignment-roles";
            public const string CreateStaff = BaseEndpoint + "/staff-account";
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
            public const string GetParcelByTrackingCode = BaseEndpoint + "/{parcelCode}";
            public const string CreateShipmentItem = BaseEndpoint;
            public const string UpdateShipmentItem = BaseEndpoint;
            public const string DeleteShipmentItem = BaseEndpoint + "/{id}";
            public const string ConfirmParcel = BaseEndpoint + "/staff/confirmation";
            public const string RejectParcel = BaseEndpoint + "/staff/rejection/{parcelId}";
            public const string GetChargeableWeight = BaseEndpoint + "/chargeable-weight";
            public const string LoadParcelOnTrain = BaseEndpoint + "/staff/loading/{parcelCode}/{trainCode}";
            public const string UnloadParcelFromTrain = BaseEndpoint + "/staff/unloading/{parcelCode}/{trainCode}";
            public const string UpdateParcelStatusToAwaitingDelivery = BaseEndpoint + "/staff/awaiting-delivery/{parcelCode}";
            public const string ReportLostParcel = BaseEndpoint + "/staff/lost/{parcelCode}/{trackingForShipmentStatus}";
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
            public const string PickUpShipment = BaseEndpoint + "/staff/pickup-confirmation";
            public const string RejectShipment = BaseEndpoint + "/staff/reject-confirmation";
            public const string GetLocation = BaseEndpoint + "/{trackingCode}/location";
            public const string UnloadingAtStation = BaseEndpoint + "/staff/update-unloading"; 
            public const string StorageInWarehouse = BaseEndpoint + "/staff/update-storage";
            public const string AssignTrainToShipment = BaseEndpoint + "/staff/assign-train";
            public const string GetShipmentById = BaseEndpoint + "/{id}";
            public const string CancelShipment = BaseEndpoint + "/cancel";
            public const string FeedbackShipment = BaseEndpoint + "/feedback";
            public const string CompleteShipment = BaseEndpoint + "/complete";
            public const string ReturnForShipment = BaseEndpoint + "/return/{shipmentId}";
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
            public const string GetTransactionType = BaseEndpoint + "/types";
        }

        public static class Notification
        {
            private const string BaseEndpoint = "/" + AreaName + "/notifications";

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
            private const string BaseEndpoint = "/" + AreaName + "/user-devices";
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
        public static class MetroRouteEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/metro-lines";
            public const string GetMetroLines = BaseEndpoint;
            public const string GetMetroLinesDropdownList = BaseEndpoint + "/dropdown";
            public const string GetMetroLineById = BaseEndpoint + "/{id}";
            public const string CreateMetroLine = BaseEndpoint;
            public const string UpdateMetroLine = BaseEndpoint;
            public const string DeleteMetroLine = BaseEndpoint + "/{id}";
            public const string GetMetroLinesByRegion = BaseEndpoint + "/region";
            public const string ActivateMetroLine = BaseEndpoint + "/activation/{id}";
            public const string GetMetroLineWithStationsById = BaseEndpoint + "/{id}/with-stations";
            public const string GetAllActiveMetroLines = BaseEndpoint + "/active-lines";
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
            public const string GetTrainsByLineSlotAndDate = BaseEndpoint 
                + "/line/{LineId}/slot/{TimeSlotId}/date/{Date}";
            public const string GetTrainById = BaseEndpoint + "/{id}";
            public const string CreateTrain = BaseEndpoint;
            public const string UpdateTrain = BaseEndpoint + "/{id}";
            public const string DeleteTrain = BaseEndpoint + "/{id}";
            public const string GetTrainsByLineId = BaseEndpoint + "/line/{lineId}";
            public const string SendLocation = BaseEndpoint + "/staff/send-location";
            public const string AddShipmentItinerariesForTrain = BaseEndpoint + "/itineraries";
        }

        public static class MediaEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/media";
            public const string UploadImage = BaseEndpoint + "/image";
            public const string UploadMultipleImages = BaseEndpoint + "/images";
            public const string UploadVideo = BaseEndpoint + "/video";
            public const string UploadMultipleVideos = BaseEndpoint + "/videos";
            public const string UploadRawFile = BaseEndpoint + "/raw-file";
            public const string UploadMultipleRawFiles = BaseEndpoint + "/raw-files";
            public const string DeleteResource = BaseEndpoint + "/resource";
            public const string DeleteMultipleResources = BaseEndpoint + "/resources";
            public const string GetBusinessMediaType = BaseEndpoint + "/business-media-type";
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

        public static class RegionEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/regions";
            public const string GetRegions = BaseEndpoint;
            public const string GetRegionById = BaseEndpoint + "/{id}";
            public const string CreateRegion = BaseEndpoint;
            public const string UpdateRegion = BaseEndpoint + "/{id}";
            public const string DeleteRegion = BaseEndpoint + "/{id}";
        }

        public static class PricingEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/pricing";
            public const string GetAllPricing = BaseEndpoint;
            public const string GetPricingById = BaseEndpoint + "/{id}";
            public const string GetPricingTable = BaseEndpoint + "/table";
            public const string CreatePricing = BaseEndpoint;
            public const string UpdatePricing = BaseEndpoint + "/{id}";
            public const string DeletePricing = BaseEndpoint + "/{id}";
            public const string CalculatePrice = BaseEndpoint + "/calculation";
        }

        public static class InsurancePolicyEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/insurance-policies";
            public const string GetAllPolicies = BaseEndpoint;
            public const string GetPolicyById = BaseEndpoint + "/{id}";
            public const string CreatePolicy = BaseEndpoint;
            public const string UpdatePolicy = BaseEndpoint + "/{id}";
            public const string DeletePolicy = BaseEndpoint + "/{id}";
        }

        public static class SupportTicketEndpoint
        {
            private const string BaseEndpoint = "/" + AreaName + "/support-tickets";
            public const string GetAllTickets = BaseEndpoint;
            public const string GetTicketById = BaseEndpoint + "/{ticketId}";
            public const string CreateTicket = BaseEndpoint;
            public const string UpdateTicket = BaseEndpoint + "/{id}";
            public const string DeleteTicket = BaseEndpoint + "/{id}";
            public const string GetTicketsByUserId = BaseEndpoint + "/user/{userId}";
            public const string GetTicketStatusEnum = BaseEndpoint + "/status";
            public const string GetSupportTypeEnum = BaseEndpoint + "/type";
            public const string ResolveTicket = BaseEndpoint + "/resolve";
            public const string CloseTicket = BaseEndpoint + "/close/{ticketId}";
        }
    }
}