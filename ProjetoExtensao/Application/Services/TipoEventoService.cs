using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Services
{
    public class TipoEventoService : ITipoEventoService
    {
        private readonly ITipoEventoRepository _repository;

        public TipoEventoService(ITipoEventoRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(TipoEvento tipoEvento)
        {
            return _repository.AddAsync(tipoEvento);
        }

        public Task DeleteAsync(long id)
        {
            return _repository.DeleteAsync(id);
        }

        public Task<IEnumerable<TipoEvento>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<TipoEvento?> GetByIdAsync(long id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task UpdateAsync(TipoEvento tipoEvento)
        {
            return _repository.UpdateAsync(tipoEvento);
        }
    }
}
