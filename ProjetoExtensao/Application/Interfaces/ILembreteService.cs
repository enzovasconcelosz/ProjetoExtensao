using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Interfaces
{
    public interface ILembreteService
    {
        Task<IEnumerable<Lembrete>> GetAllAsync();
        Task<Lembrete?> GetByIdAsync(long id);
        Task AddAsync(Lembrete lembrete);
        Task UpdateAsync(Lembrete lembrete);
        Task DeleteAsync(long id);
    }
}
