using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class Lembrete(
        string nome,
        string descricao,
        DateTime dataHoraLembrete
        )
    {
        public long Id { get; set; }
        public string Nome { get; set; } = nome;

        public string Descricao { get; set; } = descricao;

        public DateTime DataHoraLembrete { get; set; } = dataHoraLembrete;

        public DateTime DataHoraRegistro { get; set; } = DateTime.Now;

        public long? IdTipoNotificacao { get; set; }
        public TipoNotificacao? TipoNotificacao { get; set; }

        public long? IdTipoLembrete { get; set; }
        public TipoLembrete? TipoLembrete { get; set; }
    }
}