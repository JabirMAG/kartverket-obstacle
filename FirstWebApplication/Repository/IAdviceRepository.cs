using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for advice/feedback data operations
    /// </summary>
    public interface IAdviceRepository
    {
        /// <summary>
        /// Adds a new advice entry
        /// </summary>
        Task<Advice> AddAdvice(Advice advice);

        /// <summary>
        /// Gets an advice entry by ID
        /// </summary>
        Task<Advice> GetElementById(int id);

        /// <summary>
        /// Gets all advice entries
        /// </summary>
        Task<IEnumerable<Advice>> GetAllAdvice();

        /// <summary>
        /// Deletes an advice entry by ID
        /// </summary>
        Task<Advice> DeleteById(int id);

        /// <summary>
        /// Updates an existing advice entry
        /// </summary>
        Task<Advice> UpdateAdvice(Advice advice);
    }
}
