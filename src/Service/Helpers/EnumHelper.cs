using System.ComponentModel;
using MetroShip.Service.ApiModels;

namespace MetroShip.Service.Helpers;

public class EnumHelper
{
    public static IDictionary<int, string> GetEnumDictionary<T>()
    {
        var enumType = typeof(T);
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        var dictionary = new Dictionary<int, string>();
        foreach (var value in Enum.GetValues(enumType))
        {
            var name = Enum.GetName(enumType, value);
            if (name != null)
            {
                var field = enumType.GetField(name);
                if (field != null)
                {
                    var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                    dictionary.Add((int)value, attr?.Description ?? name);
                }
            }
        }

        return dictionary;
    }

    public static IList<EnumResponse> GetEnumList<T>()
    {
        var enumType = typeof(T);
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        var list = new List<EnumResponse>();
        foreach (var value in Enum.GetValues(enumType))
        {
            var name = Enum.GetName(enumType, value);
            if (name != null)
            {
                var field = enumType.GetField(name);
                if (field != null)
                {
                    var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                    list.Add(new EnumResponse
                    {
                        Id = (int)value,
                        Value = name,
                        Description = attr?.Description
                    });
                }
            }
        }

        return list;
    }
}