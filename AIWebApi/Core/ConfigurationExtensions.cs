namespace AIWebApi.Core;

public static class ConfigurationExtensions
{
    public static T GetStrictValue<T>(this IConfigurationSection section, string key) => CheckAndReturnValue(section.GetValue<T>(key));

    public static T GetStrictValue<T>(this IConfiguration configuration, string key) => CheckAndReturnValue(configuration.GetValue<T>(key));

    private static T CheckAndReturnValue<T>(T? value) => value == null || value.Equals(default(T)) ? throw new ConfigNotExistsException() : value;
}
