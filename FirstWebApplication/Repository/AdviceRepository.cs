using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace FirstWebApplication.NewFolder
{
    public class AdviceRepository : IAdviceRepository
    {

        private readonly ApplicationDBContext _context; 

        public AdviceRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Advice> AddAdvice(Advice advice)
        {
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();
            return advice;
        }

        public async Task<Advice> GetElementById(int id)
        {
            var findById = await _context.Feedback.Where(x => x.adviceID == id).FirstOrDefaultAsync();
            if (findById != null)
            {
                return findById;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<Advice>> GetAllAdvice(Advice advice)
        {
            var getAllData = await _context.Feedback.Take(50).ToListAsync();
            return getAllData;
        }


        public async Task<Advice> DeleteById (int id)
        {
            var elementById = await _context.Feedback.FindAsync(id);
            if (elementById != null)
            {
                _context.Feedback .Remove(elementById);
                await _context.SaveChangesAsync();
                return elementById;
            }

            else
            {
                return null;
            }

        }

        public async Task<Advice> UpdateAdvice(Advice advice)
        {
            _context.Feedback.Update(advice);
            await _context.SaveChangesAsync();
            return advice;

        }

        public async Task<IEnumerable<Advice>> GetAllAdvice ()
        {
            var getAllData = await _context.Feedback.Take(50).ToListAsync();
            return getAllData;
        }





        }


    }

