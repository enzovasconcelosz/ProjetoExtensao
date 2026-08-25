using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Interfaces
{
    public interface ITipoEventoRepository
    {
        Task<IEnumerable<TipoEvento>> GetAllAsync();
        Task<TipoEvento?> GetByIdAsync(long id);
        Task AddAsync(TipoEvento tipoEvento);
        Task UpdateAsync(TipoEvento tipoEvento);
        Task DeleteAsync(long id);
    }
}
