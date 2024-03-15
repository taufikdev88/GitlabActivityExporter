using Cocona;
using GitlabActivityExporter.Services;
using GitlabActivityExporter.Settings;
using System.ComponentModel;
using System.Reflection;

namespace GitlabActivityExporter.Commands;
public class Config(
    IConfigurationService config)
{
    [Command(Description = "Get all configuration properties")]
    public static void List()
    {
        Console.WriteLine("Showing all configurations");
        var props = typeof(AppSetting).GetProperties();
        foreach (var prop in props)
        {
            var description = prop.GetCustomAttribute<DescriptionAttribute>();
            Console.WriteLine($"{prop.Name}\t{description?.Description}");
        }
    }

    [Command(Description = "Get the configuration value")]
    public void Get(
        [Argument(Description = "Config name")] string name)
    {
        var selected = typeof(AppSetting).GetProperty(name);
        if (selected is null)
        {
            Console.WriteLine("Not found");
            return;
        }

        var value = config.Get(name);
        Console.WriteLine(value);
    }

    [Command(Description = "Set the configuration value")]
    public void Set(
        [Argument(Description = "Config name")] string name,
        [Argument(Description = "Config value")] string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("Invalid value");
            return;
        }

        if (config.Set(name, value))
            Console.WriteLine("Success");
        else
            Console.WriteLine("Failed");
    }
}