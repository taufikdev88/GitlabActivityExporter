using GitlabActivityExporter.Settings;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System.Text.Json;

namespace GitlabActivityExporter.Services;
public class ConfigurationService(
    IDataProtectionProvider dataProtectionProvider,
    IDirectoryService directoryService) : IConfigurationService
{
    private readonly IDataProtector dataProtector = dataProtectionProvider.CreateProtector("AppSettings");

    public string Get(string key)
    {
        var appSetting = Get();
        var propertyInfo = appSetting.GetType().GetProperty(key);
        if (propertyInfo is null)
            return "";

        var propertyValue = propertyInfo.GetValue(appSetting);
        if (propertyValue is null)
            return "";

        return Convert.ToString(propertyValue) ?? "";
    }

    public bool Set(string key, string value)
    {
        var appSetting = Get();
        var propertyInfo = appSetting.GetType().GetProperty(key);
        if (propertyInfo is null)
            return false;

        propertyInfo.SetValue(appSetting, value);
        Write(appSetting);
        return true;
    }

    private AppSetting Get()
    {
        if (!File.Exists(directoryService.GetUserConfigPath()))
            return new();

        var settings = File.ReadAllText(directoryService.GetUserConfigPath());
        if (string.IsNullOrWhiteSpace(settings))
            return new();

        var unprotectedSettings = dataProtector.Unprotect(settings);
        return JsonSerializer.Deserialize<AppSetting>(unprotectedSettings) ?? new();
    }

    private void Write(AppSetting appSetting)
    {
        var settings = JsonSerializer.Serialize(appSetting);
        var protectedSettings = dataProtector.Protect(settings);

        File.WriteAllText(directoryService.GetUserConfigPath(), protectedSettings, Encoding.UTF8);
    }
}
