using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Validacoes;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Services
{
    public class LembreteService : ILembreteService
    {
        private readonly ILembreteRepository _repository;

        public LembreteService(ILembreteRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(Lembrete lembrete)
        {
            // Validado tambem aqui: a tela nao e o unico caminho ate o banco
            ValidacaoLembrete.Garantir(lembrete);
            return _repository.AddAsync(lembrete);
        }

        public Task DeleteAsync(long id) => _repository.DeleteAsync(id);

        public Task<IEnumerable<Lembrete>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Lembrete?> GetByIdAsync(long id) => _repository.GetByIdAsync(id);

        public Task UpdateAsync(Lembrete lembrete)
        {
            ValidacaoLembrete.Garantir(lembrete);
            return _repository.UpdateAsync(lembrete);
        }
    }
}
