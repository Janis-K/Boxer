using Boxer.Models;

namespace Boxer.Interfaces;

public interface IBoxRepository
{
    /// <summary>
    ///     Adds a box to the box repository.
    /// </summary>
    /// <param name="box">The box to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddBoxAsync(Box box);
}