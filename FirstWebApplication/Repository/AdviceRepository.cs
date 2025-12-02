using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for advice/feedback data operations
    /// </summary>
    public class AdviceRepository : IAdviceRepository
    {

        private readonly ApplicationDBContext _context; 

        public AdviceRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new advice entry
        /// </summary>
        public async Task<Advice> AddAdvice(Advice advice)
        {
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();
            return advice;
        }

        /// <summary>
        /// Gets an advice entry by ID
        /// </summary>
        public async Task<Advice?> GetElementById(int id)
        {
            return await _context.Feedback
                .Where(x => x.adviceID == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes an advice entry by ID
        /// </summary>
        public async Task<Advice?> DeleteById(int id)
        {
            var elementById = await _context.Feedback.FindAsync(id);
            if (elementById != null)
            {
                _context.Feedback.Remove(elementById);
                await _context.SaveChangesAsync();
                return elementById;
            }

            return null;
        }

        /// <summary>
        /// Updates an existing advice entry
        /// </summary>
        public async Task<Advice> UpdateAdvice(Advice advice)
        {
            _context.Feedback.Update(advice);
            await _context.SaveChangesAsync();
            return advice;

        }

        /// <summary>
        /// Gets the 50 most recent advice entries
        /// </summary>
        public async Task<IEnumerable<Advice>> GetAllAdvice()
        {
            return await _context.Feedback
                .OrderByDescending(x => x.adviceID)
                .Take(50)
                .ToListAsync();
        }

        /// <summary>
        /// Maps AdviceViewModel to Advice entity
        /// </summary>
        /// <param name="viewModel">The ViewModel containing feedback data</param>
        /// <returns>Advice entity ready for database storage</returns>
        public Advice MapFromViewModel(AdviceViewModel viewModel)
        {
            return new Advice
            {
                adviceMessage = viewModel.ViewadviceMessage,
                Email = viewModel.ViewEmail
            };
        }

        /// <summary>
        /// Creates an Advice entity from email and message strings
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="message">The advice message</param>
        /// <returns>Advice entity</returns>
        public Advice CreateFromEmailAndMessage(string email, string message)
        {
            return new Advice
            {
                Email = email,
                adviceMessage = message
            };
        }
    }
}

