using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Interfaces
{
    public interface ITipoLembreteService
    {
        Task<IEnumerable<TipoLembrete>> GetAllAsync();
        Task<TipoLembrete?> GetByIdAsync(long id);
        Task AddAsync(TipoLembrete tipoLembrete);
        Task UpdateAsync(TipoLembrete tipoLembrete);
        Task DeleteAsync(long id);
    }
}
