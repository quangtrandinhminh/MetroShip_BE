using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MetroShip.Service.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySetting _vnpaySetting;
        private readonly VnPayLibrary _vnpayHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int WaitingTimeMinutes = 15;

        public VnPayService(IServiceProvider serviceProvider)
        {
            _vnpaySetting = VnPaySetting.Instance;
            _vnpayHelper = new VnPayLibrary();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        }

        public async Task<string> CreatePaymentUrl(string orderId, decimal totalAmount)
        {
            // get localhost of this server
            var request = _httpContextAccessor.HttpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host;
            var returnUrl = $"{scheme}://{host}" + WebApiEndpoint.ShipmentEndpoint.VnpayExecute;
            var hostName = System.Net.Dns.GetHostName();
            var clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0).ToString();
            var orderInfo = $"Thanh toán giao dịch ID: {orderId}, Tổng giá trị: {totalAmount} VND";
            var amount = (int)totalAmount * 100; // Convert to VND in cents
            // Ensure amount is a whole number (integer), not a decimal or float
            if (amount <= 0)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, 
                    "Invalid amount", StatusCodes.Status400BadRequest);
            }

            DateTime createDate = DateTime.Now;
            var bankCode = "NCB"; // Optional: specify bank code if needed
            //var tick = DateTime.Now.Ticks;
            var vnpay = new VnPayLibrary();

            // Cấu hình dữ liệu
            vnpay.AddRequestData("vnp_Version", "2.1.0"); // Version
            vnpay.AddRequestData("vnp_Command", "pay"); // Command for create token
            vnpay.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode); // Merchant code
            vnpay.AddRequestData("vnp_BankCode", bankCode);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_TxnRef", orderId); // Order code from the client
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_IpAddr", clientIPAddress);
            vnpay.AddRequestData("vnp_OrderType", "other");

            try
            {
                // Generate the payment URL
                var paymentUrl = vnpay.CreateRequestUrl(_vnpaySetting.BaseUrl, _vnpaySetting.HashSecret);
                return paymentUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating payment URL", ex);
            }
        }

        public async Task<VnPaymentResponse> PaymentExecute(VnPayCallbackModel model)
        {
            // Extract vnp_TxnRef to get OrderCode
            var vnpOrderIdStr = model.vnp_TxnRef;
            var paymentDate = model.vnp_PayDate;

            // Add all parameters from model to the vnpay helper
            // Use reflection to get all properties of the model
            var properties = typeof(VnPayCallbackModel).GetProperties();
            foreach (var property in properties)
            {
                var key = property.Name;
                var value = property.GetValue(model)?.ToString();

                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && value != null)
                {
                    _vnpayHelper.AddResponseData(key, value);
                }
            }

            // Extract necessary data from the model
            var vnpTransactionId = Convert.ToInt64(model.vnp_TransactionNo);
            var vnpSecureHash = model.vnp_SecureHash;
            var vnpResponseCode = model.vnp_ResponseCode;
            var vnpOrderInfo = model.vnp_OrderInfo;

            // Validate the signature
            bool checkSignature = _vnpayHelper.ValidateSignature(vnpSecureHash, _vnpaySetting.HashSecret);
            if (!checkSignature)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    "Invalid signature", StatusCodes.Status400BadRequest);
            }

            // Handle the response based on the VNPay response code
            /*if (vnpResponseCode != "00") // Success
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    $"Payment failed with response code: {vnpResponseCode}", StatusCodes.Status400BadRequest);
            }*/

            // Build the PaymentId by reconstructing the query string from model properties
            /*var queryParams = new List<string>();
            foreach (var property in properties)
            {
                var key = property.Name;
                var value = property.GetValue(model)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    queryParams.Add($"{key}={Uri.EscapeDataString(value)}");
                }
            }*/

            return new VnPaymentResponse()
            {
                Success = vnpResponseCode == "00", // Check if the response code indicates success
                PaymentMethod = PaymentMethodEnum.VnPay.ToString(),
                OrderDescription = vnpOrderInfo,
                OrderId = vnpOrderIdStr,
                TransactionId = vnpTransactionId.ToString(),
                Token = vnpSecureHash,
                PaymentTime = paymentDate,
                VnPayResponseCode = vnpResponseCode
            };
        }

        public int GetVnPayWaitingTimeMinutes()
        {
            return WaitingTimeMinutes;
        }
    }
}
