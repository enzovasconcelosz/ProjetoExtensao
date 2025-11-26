using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class Evento(
        string descricao,
        DateTime dataHoraEvento
        )
    {
        public long Id { get; set; }

        public string Descricao { get; set; } = descricao;

        public DateTime DataHoraEvento { get; set; } = dataHoraEvento;

        public DateTime DataHoraRegistro { get; set; } = DateTime.Now;

        public TipoNotificacaoEnum? TipoNotificacao { get; set; }

        public long? IdTipoEvento { get; set; }
        public TipoEvento? TipoEvento { get; set; }
    }
}