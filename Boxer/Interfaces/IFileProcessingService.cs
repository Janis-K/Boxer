namespace Boxer.Interfaces;

public interface IFileProcessingService
{
    /// <summary>
    /// Processes a file asynchronously.
    /// </summary>
    /// <param name="fileName">The name of the file to be processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessFileAsync(string fileName);
}