using System.Web;

namespace MetroShip.Utility.Helpers
{
    public static class ObjExtensions
    {
        public static string ToJsonString(this object obj)
        {
            return ObjHelper.ToJsonString(obj);
        }

        public static T Clone<T>(this T obj)
        {
            return ObjHelper.Clone(obj);
        }

        public static T ConvertTo<T>(this object obj)
        {
            return ObjHelper.ConvertTo<T>(obj);
        }

        public static T WithoutRefLoop<T>(this T obj)
        {
            return ObjHelper.WithoutRefLoop(obj);
        }

        public static string GetQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }
    }
}
