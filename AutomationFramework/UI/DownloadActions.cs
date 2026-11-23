using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.UI;

/// <summary>Waits for files in the scenario's download directory.</summary>
public static class DownloadActions
{
    /// <summary>Returns the downloaded file path after the named file appears and temporary download files disappear.</summary>
    public static string WaitForFile(string directory, string fileName, TimeSpan? timeout = null) =>
        WaitForFile(directory, fileName, timeout, minimumBytes: 0);

    /// <summary>Waits for a completed download with at least the expected number of bytes.</summary>
    public static string WaitForFile(string directory, string fileName, TimeSpan? timeout, long minimumBytes)
    {
        if (Path.GetFileName(fileName) != fileName || string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Provide the expected file name in the download directory.", nameof(fileName));
        ArgumentOutOfRangeException.ThrowIfNegative(minimumBytes);

        var wait = new DefaultWait<string>(Path.GetFullPath(directory))
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(15),
            PollingInterval = TimeSpan.FromMilliseconds(200),
            Message = $"The download did not finish: {fileName}"
        };
        return wait.Until(folder =>
        {
            var file = new FileInfo(Path.Combine(folder, fileName));
            return file.Exists && file.Length >= minimumBytes && !File.Exists(file.FullName + ".part") &&
                !File.Exists(file.FullName + ".crdownload") ? file.FullName : null;
        })!;
    }
}
