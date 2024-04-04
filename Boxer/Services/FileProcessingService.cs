using System.Text;
using System.Text.RegularExpressions;
using Boxer.Interfaces;
using Boxer.Models;

namespace Boxer.Services;

public partial class FileProcessingService(ILogger<FileProcessingService> logger, IServiceScopeFactory serviceScopeFactory) : IFileProcessingService
{
    private const string BoxIdentifier = "HDR";
    private const string LineIdentifier = "LINE";

    private const string BoxLineRegularExpression = @"HDR\s+(?<supplierIdentifier>\S+)\s+(?<id>\S+)";
    private const string ContentLineRegularExpression = @"LINE\s+(?<poNumber>\S+)\s+(?<isbn>\S+)\s+(?<quantity>\S+)";
    
    /// <inheritdoc />
    public async Task ProcessFileAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
        }
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var boxRepository = scope.ServiceProvider.GetRequiredService<IBoxRepository>();
            
            var currentBox = new Box();
            var currentContent = new List<Box.Content>();

            using var sr = new StreamReader(fileName);
            while (await sr.ReadLineAsync() is { } line)
            {
                try
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.StartsWith(BoxIdentifier))
                    {
                        currentBox = await ProcessBox(line, currentBox, currentContent, boxRepository);
                        currentContent = [];
                    }

                    if (!line.StartsWith(LineIdentifier)) continue;
                    {
                        ProcessContent(line, currentContent);
                    }
                }
                catch (ArgumentException e)
                {
                    logger.LogError(e, "Exception during Box or Content processing. Skipping current box");
                    currentBox = new Box();
                    currentContent = [];
                }
            }
            
            // Adding the final box
            if (!string.IsNullOrEmpty(currentBox?.Identifier))
            {
                await boxRepository.AddBoxAsync(new Box(currentContent, currentBox.SupplierIdentifier, currentBox.Identifier));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Uncaught exception when processing file {FilePath}", fileName);
            throw;
        }
    }

    /// <summary>
    ///     Processes a line of text as a box item and returns the updated box.
    /// </summary>
    /// <param name="line">The line of text to process as a box item.</param>
    /// <param name="currentBox">The current box being processed.</param>
    /// <param name="currentContent">The current content list of the box.</param>
    /// <param name="boxRepository">The box repository for adding boxes.</param>
    /// <returns>The updated box after processing the line.</returns>
    /// <exception cref="ArgumentException">Thrown when there is an error processing the box item.</exception>
    private async Task<Box?> ProcessBox(string line, Box currentBox, List<Box.Content> currentContent,
        IBoxRepository boxRepository)
    {
        var headerPattern = BoxLineRegex();
        var match = headerPattern.Match(line);
        if (!match.Success) return null;
        if (match.Groups.Count != 3)
        {
            var stringBuilder = new StringBuilder();
    
            for (var i = 0; i < match.Groups.Count; i++)
            {
                stringBuilder.AppendLine($"{match.Groups[i].Value} ");
            }
            logger.LogTrace("Box line items of box that couldn't be processed: {Items}",stringBuilder.ToString());
            throw new ArgumentException("Error processing box item");
        }
            
        var supplierId = match.Groups["supplierIdentifier"].Value;
        var id = match.Groups["id"].Value;
        if (string.IsNullOrEmpty(currentBox.Identifier))
            return new Box
            {
                SupplierIdentifier = supplierId,
                Identifier = id
            };
        await boxRepository.AddBoxAsync(new Box(currentContent, currentBox.SupplierIdentifier,
            currentBox.Identifier));
        return new Box
        {
            SupplierIdentifier = supplierId,
            Identifier = id
        };

    }

    /// <summary>
    ///     Processes a content line from a file and adds it to the current content list.
    /// </summary>
    /// <param name="line">The content line to process.</param>
    /// <param name="currentContent">The current content list to add the processed content to.</param>
    private void ProcessContent(string line, ICollection<Box.Content> currentContent)
    {
        var linePattern = ContentLineRegex();
        var match = linePattern.Match(line);
        if (match.Success)
        {
            if (int.TryParse(match.Groups["quantity"].Value, out var quantity))
            {
                var content = new Box.Content
                {
                    PoNumber = match.Groups["poNumber"].Value,
                    Isbn = match.Groups["isbn"].Value,
                    Quantity = quantity
                };
                currentContent.Add(content);
            }
            else 
            {
                logger.LogError("Could not parse quantity: {Quantity}", match.Groups["quantity"].Value);
            }
        } else {
            logger.LogError("Content line is incorrectly formatted: {Line}", line);
        }
    }
    
    /// <summary>
    ///     Create Regex Source Generator for the box line regular expressions
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(BoxLineRegularExpression)]
    private static partial Regex BoxLineRegex();
    
    /// <summary>
    ///     Create Regex Source Generator for the content line regular expressions
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(ContentLineRegularExpression)]
    private static partial Regex ContentLineRegex();
}