using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class PreferenciaUsuario(
        bool notificar,
        TemaSistemaEnum tema
        )
    {
        // Necessario para o EF: a tabela PreferenciaUsuario nao possui coluna de tema,
        // entao o construtor primario nao pode ser usado na materializacao.
        public PreferenciaUsuario() : this(false, default)
        {
        }

        public long Id { get; set; }

        public bool Notificar { get; set; } = notificar;

        /// <summary>Se o aviso do lembrete faz o aparelho vibrar.</summary>
        public bool Vibrar { get; set; } = true;

        /// <summary>Se o aviso do lembrete tambem toca o som de notificacao.</summary>
        public bool Som { get; set; } = true;

        public TemaSistemaEnum Tema { get; set; } = tema;
    }
}