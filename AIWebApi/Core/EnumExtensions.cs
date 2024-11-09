using System.ComponentModel;
using System.Reflection;

namespace AIWebApi.Core;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute = field != null ? Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute : null;
        return attribute == null ? value.ToString() : attribute.Description;
    }
}
