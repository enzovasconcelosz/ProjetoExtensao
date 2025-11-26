using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class PreferenciaUsuario(
        bool notificar,
        TemaSistemaEnum tema
        )
    {
        public long Id { get; set; }

        public bool Notificar { get; set; } = notificar;

        public TemaSistemaEnum Tema { get; set; } = tema;
    }
}