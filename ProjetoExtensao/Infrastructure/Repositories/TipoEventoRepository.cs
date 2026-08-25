using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao.Infrastructure.Repositories
{
    public class TipoEventoRepository : ITipoEventoRepository
    {
        private readonly AppDbContext _context;

        public TipoEventoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TipoEvento tipoEvento)
        {
            await _context.TipoEventos.AddAsync(tipoEvento);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await _context.TipoEventos.FindAsync(id);
            if (entity == null) return;
            _context.TipoEventos.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TipoEvento>> GetAllAsync()
        {
            return await _context.TipoEventos.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
        }

        public async Task<TipoEvento?> GetByIdAsync(long id)
        {
            return await _context.TipoEventos.FindAsync(id);
        }

        public async Task UpdateAsync(TipoEvento tipoEvento)
        {
            _context.TipoEventos.Update(tipoEvento);
            await _context.SaveChangesAsync();
        }
    }
}
