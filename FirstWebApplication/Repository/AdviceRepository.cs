using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repositories
{
    // Repository for tilråding/tilbakemelding dataoperasjoner
    public class AdviceRepository : IAdviceRepository
    {

        private readonly ApplicationDBContext _context; 

        public AdviceRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // Legger til en ny tilrådingsoppføring
        public async Task<Advice> AddAdvice(Advice advice)
        {
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();
            return advice;
        }

        // Henter en tilrådingsoppføring etter ID
        public async Task<Advice?> GetElementById(int id)
        {
            return await _context.Feedback
                .Where(x => x.adviceID == id)
                .FirstOrDefaultAsync();
        }

        // Sletter en tilrådingsoppføring etter ID
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

        // Oppdaterer en eksisterende tilrådingsoppføring
        public async Task<Advice> UpdateAdvice(Advice advice)
        {
            _context.Feedback.Update(advice);
            await _context.SaveChangesAsync();
            return advice;

        }

        // Henter de 50 nyeste tilrådingsoppføringene
        public async Task<IEnumerable<Advice>> GetAllAdvice()
        {
            return await _context.Feedback
                .OrderByDescending(x => x.adviceID)
                .Take(50)
                .ToListAsync();
        }

        // Mapper AdviceViewModel til Advice entity
        public Advice MapFromViewModel(AdviceViewModel viewModel)
        {
            return new Advice
            {
                adviceMessage = viewModel.ViewadviceMessage,
                Email = viewModel.ViewEmail
            };
        }

        // Oppretter en Advice entity fra e-post og meldingsstrenger
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
