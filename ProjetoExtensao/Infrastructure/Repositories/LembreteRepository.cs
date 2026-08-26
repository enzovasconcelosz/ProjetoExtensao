using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao.Infrastructure.Repositories
{
    public class LembreteRepository : ILembreteRepository
    {
        private readonly AppDbContext _context;

        public LembreteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Lembrete lembrete)
        {
            await _context.Lembretes.AddAsync(lembrete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await _context.Lembretes.FindAsync(id);
            if (entity == null) return;
            _context.Lembretes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Lembrete>> GetAllAsync()
        {
            return await _context.Lembretes
                .Include(l => l.TipoLembrete)
                .AsNoTracking()
                .OrderBy(l => l.DataHoraLembrete)
                .ToListAsync();
        }

        public async Task<Lembrete?> GetByIdAsync(long id)
        {
            return await _context.Lembretes.FindAsync(id);
        }

        public async Task UpdateAsync(Lembrete lembrete)
        {
            // As navegacoes vem preenchidas quando a entidade foi lida com Include.
            // Anexa-las de novo geraria conflito de rastreamento; as chaves
            // estrangeiras sozinhas ja bastam para gravar.
            lembrete.TipoLembrete = null;
            lembrete.TipoNotificacao = null;

            // O contexto vive enquanto o app estiver aberto, entao a mesma linha
            // pode ja estar sendo rastreada de uma operacao anterior.
            var rastreado = _context.Lembretes.Local.FirstOrDefault(l => l.Id == lembrete.Id);
            if (rastreado != null && !ReferenceEquals(rastreado, lembrete))
                _context.Entry(rastreado).State = EntityState.Detached;

            _context.Lembretes.Update(lembrete);
            await _context.SaveChangesAsync();
        }
    }
}
