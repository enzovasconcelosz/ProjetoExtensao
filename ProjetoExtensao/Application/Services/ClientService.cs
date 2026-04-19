using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Domain.Entities;

namespace ProjetoExtensao.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;

        public ClientService(IClientRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(Client client)
        {
            return _repository.AddAsync(client);
        }

        public Task DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        public Task<IEnumerable<Client>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<Client?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task UpdateAsync(Client client)
        {
            return _repository.UpdateAsync(client);
        }
    }
}