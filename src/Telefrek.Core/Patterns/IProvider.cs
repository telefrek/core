using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Patterns
{
    /// <summary>
    /// Provider interface
    /// </summary>
    /// <typeparam name="T">The type of object managed by the provider.</typeparam>
    public interface IProvider<T>
    {
        /// <summary>
        /// Get the current instance asynchronously
        /// </summary>
        /// <returns>The instance</returns>
        Task<T> GetAsync();

        /// <summary>
        /// Resets the provider asynchronously
        /// </summary>
        Task ResetAsync();
    }
}