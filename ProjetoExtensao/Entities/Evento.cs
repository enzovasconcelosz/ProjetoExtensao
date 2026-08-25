using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class Evento(
        string nome,
        string descricao,
        DateTime dataHoraEvento
        )
    {
        public long Id { get; set; }
        public string Nome { get; set; } = nome;

        public string Descricao { get; set; } = descricao;

        public DateTime DataHoraEvento { get; set; } = dataHoraEvento;

        public DateTime DataHoraRegistro { get; set; } = DateTime.Now;

        public long? IdTipoNotificacao { get; set; }
        public TipoNotificacao? TipoNotificacao { get; set; }

        public long? IdTipoEvento { get; set; }
        public TipoEvento? TipoEvento { get; set; }
    }
}