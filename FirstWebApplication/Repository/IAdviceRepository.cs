using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    public interface IAdviceRepository
    {
        Task<Advice> AddAdvice(Advice advice);

        Task<Advice> GetElementById(int id);

        Task<IEnumerable<Advice>> GetAllAdvice();

        Task<Advice> DeleteById(int id);

        Task<Advice> UpdateAdvice(Advice advice);
    }
}
