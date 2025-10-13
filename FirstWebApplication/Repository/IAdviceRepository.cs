using FirstWebApplication.Models;

namespace FirstWebApplication.NewFolder
{
    public interface IAdviceRepository
    {
        Task<Advice> AddAdvice(Advice advice);

        Task<Advice> GetElementById(int id);

        Task<IEnumerable<Advice>> GetAllAdvice(Advice advice);

        Task<Advice> DeleteById(int id);

        Task<Advice> UpdateAdvice(Advice advice);
    }
}
