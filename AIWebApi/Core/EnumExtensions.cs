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

    public static T CreateByDescription<T>(this string description) where T : Enum
    {
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute && attribute.Description == description)
            {
                return (T)Enum.Parse(typeof(T), field.Name);
            }
        }
        throw new ArgumentException($"No enum with description {description} found in {typeof(T)}");
    }
}
