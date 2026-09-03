using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao.Infrastructure.Repositories
{
    public class TipoLembreteRepository : ITipoLembreteRepository
    {
        private readonly AppDbContext _context;
        private readonly IUsuarioAtual _usuarioAtual;

        public TipoLembreteRepository(AppDbContext context, IUsuarioAtual usuarioAtual)
        {
            _context = context;
            _usuarioAtual = usuarioAtual;
        }

        /// <summary>
        /// Somente os tipos criados pelo usuario logado. Sem usuario logado o
        /// resultado e vazio — veja <see cref="LembreteRepository"/>.
        /// </summary>
        private IQueryable<TipoLembrete> DoUsuario()
        {
            var idUsuario = _usuarioAtual.Id;

            return idUsuario == null
                ? _context.TipoLembretes.Where(t => false)
                : _context.TipoLembretes.Where(t => t.IdUsuario == idUsuario);
        }

        public async Task AddAsync(TipoLembrete tipoLembrete)
        {
            // Carimba o dono na gravacao: a tela nao precisa saber disso
            tipoLembrete.IdUsuario ??= _usuarioAtual.Id;

            await _context.TipoLembretes.AddAsync(tipoLembrete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await DoUsuario().FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return;
            _context.TipoLembretes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TipoLembrete>> GetAllAsync()
        {
            return await DoUsuario().AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
        }

        public async Task<TipoLembrete?> GetByIdAsync(long id)
        {
            return await DoUsuario().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(TipoLembrete tipoLembrete)
        {
            // Como no lembrete: a edicao so vale para um tipo desta conta
            var idUsuario = _usuarioAtual.Id;

            var pertence = await _context.TipoLembretes
                .AsNoTracking()
                .AnyAsync(t => t.Id == tipoLembrete.Id && t.IdUsuario == idUsuario);

            if (!pertence)
                return;

            tipoLembrete.IdUsuario = idUsuario;
            tipoLembrete.Usuario = null;

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
