using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao.Infrastructure.Repositories
{
    public class TipoLembreteRepository : ITipoLembreteRepository
    {
        private readonly AppDbContext _context;

        public TipoLembreteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TipoLembrete tipoLembrete)
        {
            await _context.TipoLembretes.AddAsync(tipoLembrete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await _context.TipoLembretes.FindAsync(id);
            if (entity == null) return;
            _context.TipoLembretes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TipoLembrete>> GetAllAsync()
        {
            return await _context.TipoLembretes.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
        }

        public async Task<TipoLembrete?> GetByIdAsync(long id)
        {
            return await _context.TipoLembretes.FindAsync(id);
        }

        public async Task UpdateAsync(TipoLembrete tipoLembrete)
        {
            // O contexto vive enquanto o app estiver aberto, entao a mesma linha
            // pode ja estar sendo rastreada de uma operacao anterior.
            var rastreado = _context.TipoLembretes.Local.FirstOrDefault(t => t.Id == tipoLembrete.Id);
            if (rastreado != null && !ReferenceEquals(rastreado, tipoLembrete))
                _context.Entry(rastreado).State = EntityState.Detached;

            _context.TipoLembretes.Update(tipoLembrete);
            await _context.SaveChangesAsync();
        }
    }
}
