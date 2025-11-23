using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for report/rapport data operations
    /// </summary>
    public class RegistrarRepository : IRegistrarRepository
    {
        private readonly ApplicationDBContext _context;

        public RegistrarRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new report
        /// </summary>
        public async Task<RapportData> AddRapport(RapportData rapport)
        {
            await _context.Rapports.AddAsync(rapport);
            await _context.SaveChangesAsync();
            return rapport;
        }

        /// <summary>
        /// Gets the 50 most recent reports with their associated obstacles
        /// </summary>
        public async Task<IEnumerable<RapportData>> GetAllRapports()
        {
            return await _context.Rapports
                .Include(r => r.Obstacle)
                    .ThenInclude(o => o.OwnerUser)
                .OrderByDescending(r => r.RapportID)
                .Take(50)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing report
        /// </summary>
        public async Task<RapportData> UpdateRapport(RapportData rapport)
        {
            _context.Rapports.Update(rapport);
            await _context.SaveChangesAsync();
            return rapport;
        }
    }
}
