namespace GitlabActivityExporter.Services;
public interface IConfigurationService
{
    string Get(string name);
    bool Set(string name, string value);
}
