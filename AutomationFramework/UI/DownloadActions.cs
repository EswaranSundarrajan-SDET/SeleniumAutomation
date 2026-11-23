using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.UI;

public static class DownloadActions
{
    public static string WaitForFile(string directory, string fileName, TimeSpan? timeout = null)
    {
        if (Path.GetFileName(fileName) != fileName || string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Provide the expected file name in the download directory.", nameof(fileName));

        var wait = new DefaultWait<string>(Path.GetFullPath(directory))
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(15),
            PollingInterval = TimeSpan.FromMilliseconds(200),
            Message = $"The download did not finish: {fileName}"
        };
        return wait.Until(folder =>
        {
            var file = Path.Combine(folder, fileName);
            return File.Exists(file) && !File.Exists(file + ".part") &&
                !File.Exists(file + ".crdownload") ? file : null;
        })!;
    }
}
