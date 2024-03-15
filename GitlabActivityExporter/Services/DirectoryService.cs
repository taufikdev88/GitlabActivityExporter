namespace GitlabActivityExporter.Services;
public class DirectoryService : IDirectoryService
{
    public string GetUserConfigPath()
    {
        return Path.Combine(GetUserDirectoryPath(), ".gitlab-activity-exporter");
    }

    public string GetUserDirectoryPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
