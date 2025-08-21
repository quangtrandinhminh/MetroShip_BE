using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Helpers
{
    public static class PushNotificationHelper
    {
        public static Dictionary<string, string> CreateCartNotificationData(int itemCount)
        {
            return new Dictionary<string, string>
        {
            { "type", "CART" },
            { "itemCount", itemCount.ToString() },
            { "timestamp", DateTimeOffset.UtcNow.AddHours(7).ToUnixTimeSeconds().ToString() }
        };
        }

        public static Dictionary<string, string> CreateChatNotificationData(int chatMessageId, int userId, string message)
        {
            return new Dictionary<string, string>
        {
            { "type", "CHAT" },
            { "chatMessageId", chatMessageId.ToString() },
            { "userId", userId.ToString() },
            { "message", message },
            { "timestamp", DateTimeOffset.UtcNow.AddHours(7).ToUnixTimeSeconds().ToString() }
        };
        }

        public static Dictionary<string, string> CreateOrderNotificationData(int orderId, string message)
        {
            return new Dictionary<string, string>
        {
            { "type", "ORDER" },
            { "orderId", orderId.ToString() },
            { "message", message },
            { "timestamp", DateTimeOffset.UtcNow.AddHours(7).ToUnixTimeSeconds().ToString() }
        };
        }
    }
}