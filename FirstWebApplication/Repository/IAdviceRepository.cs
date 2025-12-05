using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

namespace FirstWebApplication.Repositories
{
    // Repository-grensesnitt for tilråding/tilbakemelding dataoperasjoner
    public interface IAdviceRepository
    {
        // Legger til en ny tilrådingsoppføring
        Task<Advice> AddAdvice(Advice advice);
        
        // Henter en tilrådingsoppføring etter ID
        Task<Advice?> GetElementById(int id);
        
        // Henter alle tilrådingsoppføringer
        Task<IEnumerable<Advice>> GetAllAdvice();
        
        // Sletter en tilrådingsoppføring etter ID
        Task<Advice?> DeleteById(int id);
        
        // Oppdaterer en eksisterende tilrådingsoppføring
        Task<Advice> UpdateAdvice(Advice advice);
        
        // Mapper AdviceViewModel til Advice entity
        Advice MapFromViewModel(AdviceViewModel viewModel);
        
        // Oppretter en Advice entity fra e-post og meldingsstrenger
        Advice CreateFromEmailAndMessage(string email, string message);
    }
}
