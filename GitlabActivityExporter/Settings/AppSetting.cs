using System.ComponentModel;

namespace GitlabActivityExporter.Settings;
public class AppSetting
{
    [Description("Gitlab Host")]
    public string? Host { get; set; }
    [Description("Gitlab Private Token")]
    public string? Token { get; set; }
}
