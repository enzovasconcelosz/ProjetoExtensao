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

        public TemaSistemaEnum Tema { get; set; } = tema;
    }
}