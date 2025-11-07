using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repositories
{
    public class RegistrarRepository : IRegistrarRepository
    {
        private readonly ApplicationContext _context;

        public RegistrarRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<RapportData> AddRapport(RapportData rapport)
        {
            await _context.Rapports.AddAsync(rapport);
            await _context.SaveChangesAsync();
            return rapport;
        }

        public async Task<IEnumerable<RapportData>> GetAllRapports()
        {
            return await _context.Rapports
                .Include(r => r.Obstacle)
                .OrderByDescending(r => r.RapportID)
                .Take(50)
                .ToListAsync();
        }

        public async Task<RapportData> UpdateRapport(RapportData rapport)
        {
            _context.Rapports.Update(rapport);
            await _context.SaveChangesAsync();
            return rapport;
        }
    }
}
