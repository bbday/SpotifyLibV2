using System;
using System.Linq;

namespace Spotify.Lib.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringAttribute : Attribute
    {
        public StringAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
#nullable enable
        public static bool GetValue(Type enumType, Enum enumValue, out string? result)
        {
            if (enumType
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttributes(typeof(StringAttribute), true)
                .FirstOrDefault() is StringAttribute stringAttr)
            {
                result = stringAttr.Value;
                return true;
            }

            result = null;
            return false;
        }
#nullable disable
    }
}