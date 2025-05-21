using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MetroShip.Utility.Config
{
    public class SystemSettingModel
    {
        private static SystemSettingModel _instance;

        public static IConfiguration Configs { get; set; }
        public string ApplicationName { get; set; } /*= Assembly.GetEntryAssembly()?.GetName().Name;*/

        public string? Domain { get; set; }
        public string SecretKey { get; set; }
        public string SecretCode { get; set; }

        public static SystemSettingModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemSettingModel();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }

    public class MailSettingModel
    {
        public static MailSettingModel Instance { get; set; }
        public SmtpSetting Smtp { get; set; }
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
    }

    public class SmtpSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UsingCredential { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class VnPaySetting
    {
        public static VnPaySetting Instance { get; set; }
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string BaseUrl { get; set; }
        public string Version { get; set; }
        public string CurrCode { get; set; }
        public string Locale { get; set; }
    }

    public class VietQRSetting
    {
        public static VietQRSetting Instance { get; set; }
        public string ClientID { get; set; }
        public string APIKey { get; set; }
    }

    public class GoogleSetting
    {
        public static GoogleSetting Instance { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
    }

    public class PayOSSetting
    {
        public static PayOSSetting Instance { get; set; }
        public string ClientID { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
    }

    public class CloudinarySetting
    {
        public static CloudinarySetting Instance { get; set; }
        public string CloudinaryUrl { get; set; }
    }

    public class TwilioSetting
    {
        public static TwilioSetting Instance { get; set; }
        public string AccountSID { get; set; }
        public string AuthToken { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class AdminConfig
    {
        public static AdminConfig Instance { get; set; }
        public string AdminUsername { get; set; }
        public string AdminHashPassword { get; set; }
    }

    public class SystemConfigSetting
    {
        public static SystemConfigSetting Instance { get; set; }
        public string CONFIRMATION_HOUR { get; set; }
        public string PAYMENT_REQUEST_HOUR { get; set; }
        public string MAX_SCHEDULE_SHIPMENT_DAY { get; set; }
        public string ALLOW_CANCEL_BEFORE_HOUR { get; set; }
        public string REFUND_PERCENT { get; set; }
        public string SURCHARGE_AFTER_DELIVERED_HOUR { get; set; }
        public string SURCHARGE_PER_DAY_VND { get; set; }
        public string FREE_STORAGE_DAY { get; set; }
    }
}