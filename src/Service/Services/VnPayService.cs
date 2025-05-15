using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySetting _vnpaySetting;
        private readonly VnPayLibrary _vnpayHelper;

        public VnPayService(IServiceProvider serviceProvider)
        {
            _vnpaySetting = VnPaySetting.Instance;
            _vnpayHelper = new VnPayLibrary();
        }

        public async Task<string> CreatePaymentUrl(HttpContext context, string orderId, int totalAmount)
        {
            // get localhost of this server
            var request = context.Request;
            var scheme = request.Scheme;
            var host = request.Host;
            var returnUrl = $"{scheme}://{host}" + WebApiEndpoint.Shipment.VnpayExecute;
            var hostName = System.Net.Dns.GetHostName();
            var clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0).ToString();
            var orderInfo = $"Thanh toán đơn hàng ID: {orderId}, Tổng giá trị: {totalAmount} VND";

            var tick = orderId;
            var vnpay = new VnPayLibrary();

            // Cấu hình dữ liệu
            vnpay.AddRequestData("vnp_Version", "2.1.0"); // Version
            vnpay.AddRequestData("vnp_Command", "pay"); // Command for create token
            vnpay.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode); // Merchant code
            vnpay.AddRequestData("vnp_BankCode", "");
            vnpay.AddRequestData("vnp_Locale", "vn");
            var amount = totalAmount * 100; // Convert to VND in cents

            // Ensure amount is a whole number (integer), not a decimal or float
            int amountInCents = (int)amount;  // Convert to integer

            if (amountInCents <= 0)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, "Invalid amount", StatusCodes.Status400BadRequest);
            }

            vnpay.AddRequestData("vnp_Amount", amountInCents.ToString());
            DateTime createDate = DateTime.Now;
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss")); 


            // Tính toán giá trị thanh toán
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_TxnRef", tick.ToString());
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

        public async Task<VnPaymentResponse> PaymentExecute(HttpContext context)
        {
            // Retrieve vnp_TxnRef from the query string directly
            var vnpOrderIdStr = context.Request.Query["vnp_TxnRef"].ToString();
            int vnpOrderId = 0;

            // Validate vnpOrderId
            if (string.IsNullOrEmpty(vnpOrderIdStr) || !int.TryParse(vnpOrderIdStr, out vnpOrderId))
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, 
                    "Invalid order ID", StatusCodes.Status400BadRequest);
            }

            var request = context.Request;
            var collections = request.Query;

            // Add all parameters starting with "vnp_" to the vnpay instance
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    _vnpayHelper.AddResponseData(key, value.ToString());
                }
            }

            // Extract necessary data from the response
            var vnpTransactionId = Convert.ToInt64(_vnpayHelper.GetResponseData("vnp_TransactionNo"));
            var vnpSecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnpResponseCode = _vnpayHelper.GetResponseData("vnp_ResponseCode");
            var vnpOrderInfo = _vnpayHelper.GetResponseData("vnp_OrderInfo");

            // Validate the signature
            bool checkSignature = _vnpayHelper.ValidateSignature(vnpSecureHash, _vnpaySetting.HashSecret);
            if (!checkSignature)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, 
                    "Invalid signature", StatusCodes.Status400BadRequest);
            }

            // Handle the response based on the VNPay response code
            if (vnpResponseCode != "00") // Success
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    $"Payment failed with response code: {vnpResponseCode}", StatusCodes.Status400BadRequest);
            }    

            return new VnPaymentResponse()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnpOrderInfo,
                OrderId = vnpOrderId.ToString(),
                TransactionId = vnpTransactionId.ToString(),
                Token = vnpSecureHash,
                PaymentId = request.QueryString.ToString(),
                VnPayResponseCode = vnpResponseCode
            };
        }
    }
}
