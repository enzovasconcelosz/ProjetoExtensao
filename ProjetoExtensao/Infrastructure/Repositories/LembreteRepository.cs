using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao.Infrastructure.Repositories
{
    public class LembreteRepository : ILembreteRepository
    {
        private readonly AppDbContext _context;
        private readonly IUsuarioAtual _usuarioAtual;

        public LembreteRepository(AppDbContext context, IUsuarioAtual usuarioAtual)
        {
            _context = context;
            _usuarioAtual = usuarioAtual;
        }

        /// <summary>
        /// Somente os lembretes do usuario logado.
        ///
        /// O filtro fica aqui, e nao nas telas, porque este e o unico caminho
        /// ate o banco: qualquer tela nova ja nasce enxergando apenas os dados
        /// da conta que esta usando o aplicativo.
        ///
        /// Sem usuario logado o resultado e vazio — e o que vale para uma sessao
        /// encerrada, e nao "todos os lembretes".
        /// </summary>
        private IQueryable<Lembrete> DoUsuario()
        {
            var idUsuario = _usuarioAtual.Id;

            return idUsuario == null
                ? _context.Lembretes.Where(l => false)
                : _context.Lembretes.Where(l => l.IdUsuario == idUsuario);
        }

        public async Task AddAsync(Lembrete lembrete)
        {
            // Carimba o dono na gravacao: a tela nao precisa saber disso
            lembrete.IdUsuario ??= _usuarioAtual.Id;

            await _context.Lembretes.AddAsync(lembrete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            // Busca pelo filtro do usuario: ninguem exclui o lembrete de outra conta
            var entity = await DoUsuario().FirstOrDefaultAsync(l => l.Id == id);
            if (entity == null) return;
            _context.Lembretes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Lembrete>> GetAllAsync()
        {
            return await DoUsuario()
                .Include(l => l.TipoLembrete)
                .AsNoTracking()
                .OrderBy(l => l.DataHoraLembrete)
                .ToListAsync();
        }

        public async Task<Lembrete?> GetByIdAsync(long id)
        {
            return await DoUsuario().FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task UpdateAsync(Lembrete lembrete)
        {
            // O lembrete precisa ser mesmo desta conta; caso contrario a edicao
            // e ignorada, em vez de sobrescrever o dado de outro usuario.
            var idUsuario = _usuarioAtual.Id;

            var pertence = await _context.Lembretes
                .AsNoTracking()
                .AnyAsync(l => l.Id == lembrete.Id && l.IdUsuario == idUsuario);

            if (!pertence)
                return;

            lembrete.IdUsuario = idUsuario;

            // As navegacoes vem preenchidas quando a entidade foi lida com Include.
            // Anexa-las de novo geraria conflito de rastreamento; as chaves
            // estrangeiras sozinhas ja bastam para gravar.
            lembrete.TipoLembrete = null;
            lembrete.TipoNotificacao = null;
            lembrete.Usuario = null;

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
