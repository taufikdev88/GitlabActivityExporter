namespace GitlabActivityExporter.Services;
public interface IDirectoryService
{
    string GetUserDirectoryPath();
    string GetUserConfigPath();
}
