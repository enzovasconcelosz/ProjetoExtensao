using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Validacoes;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Services
{
    public class TipoLembreteService : ITipoLembreteService
    {
        private readonly ITipoLembreteRepository _repository;

        public TipoLembreteService(ITipoLembreteRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(TipoLembrete tipoLembrete)
        {
            // Validado tambem aqui: a tela nao e o unico caminho ate o banco
            ValidacaoTipoLembrete.Garantir(tipoLembrete);
            return _repository.AddAsync(tipoLembrete);
        }

        public Task DeleteAsync(long id)
        {
            return _repository.DeleteAsync(id);
        }

        public Task<IEnumerable<TipoLembrete>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<TipoLembrete?> GetByIdAsync(long id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task UpdateAsync(TipoLembrete tipoLembrete)
        {
            ValidacaoTipoLembrete.Garantir(tipoLembrete);
            return _repository.UpdateAsync(tipoLembrete);
        }
    }
}
