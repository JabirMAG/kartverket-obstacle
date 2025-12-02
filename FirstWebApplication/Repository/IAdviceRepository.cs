using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

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
        Task<Advice?> GetElementById(int id);
        
        /// <summary>
        /// Gets all advice entries
        /// </summary>
        Task<IEnumerable<Advice>> GetAllAdvice();
        
        /// <summary>
        /// Deletes an advice entry by ID
        /// </summary>
        Task<Advice?> DeleteById(int id);
        
        /// <summary>
        /// Updates an existing advice entry
        /// </summary>
        Task<Advice> UpdateAdvice(Advice advice);
        
        /// <summary>
        /// Maps AdviceViewModel to Advice entity
        /// </summary>
        /// <param name="viewModel">The ViewModel containing feedback data</param>
        /// <returns>Advice entity ready for database storage</returns>
        Advice MapFromViewModel(AdviceViewModel viewModel);
        
        /// <summary>
        /// Creates an Advice entity from email and message strings
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="message">The advice message</param>
        /// <returns>Advice entity</returns>
        Advice CreateFromEmailAndMessage(string email, string message);
    }
}
