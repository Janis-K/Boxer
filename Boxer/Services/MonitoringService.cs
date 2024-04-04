using Boxer.Interfaces;
using Boxer.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Boxer.Services;

public class MonitoringService(ILogger<MonitoringService> logger, IFileProcessingService fileProcessingService, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    /// Executes the monitoring service asynchronously.
    /// This method continuously monitors a specified folder for newly created files and processes them.
    /// @param stoppingToken The cancellation token that can be used to stop the monitoring service.
    /// @throws System.ArgumentException If the path from appSettings is not loaded.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var boxAppSettings = scope.ServiceProvider
            .GetRequiredService<IOptionsSnapshot<BoxAppSettings>>().Value;
        if (boxAppSettings == null)
        {
            throw new ArgumentException("Path from appSettings not loaded");
        }
        logger.LogInformation("Files are being monitored");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var watcher = new FileSystemWatcher();
            watcher.Path = boxAppSettings.FolderPath;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Created += OnCreated;

            // Begin watching
            watcher.EnableRaisingEvents = true;

            // Pause for a second before continuing to monitor
            await Task.Delay(1000, stoppingToken);
        }
    }

    /// <summary>
    /// Event handler for when a file is created in a specified folder.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The event data that contains information about the created file.</param>
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        logger.LogInformation("File Created: {FullPath}", e.FullPath);
        var fileInfo = new FileInfo(e.FullPath);

        
        logger.LogInformation("File is available: {FullPath}. Processing", e.FullPath);
        
        // To execute async methods inside sync context
        Task.Run(async () =>
        {
            while (IsFileLocked(fileInfo))
            {
                logger.LogInformation("File in use. Retrying in 1 second");
                await Task.Delay(1000);
            }

            try
            {
                await fileProcessingService.ProcessFileAsync(e.FullPath);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Provided fileName is null or empty");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing file {FullPath}", e.FullPath);
            }
        });
    }

    /// <summary>
    /// Checks if a file is locked.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file is locked; otherwise, false.</returns>
    private static bool IsFileLocked(FileInfo file)
    {
        FileStream? stream = null;

        try
        {
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            stream?.Close();
        }
        
        return false;
    }
}