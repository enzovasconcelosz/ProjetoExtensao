namespace ProjetoExtensao.DTOs;

public class PreferenciaUsuarioDto
{
    public long Id { get; set; }
    public bool Notificar { get; set; }
    public bool Vibrar { get; set; }
    public bool Som { get; set; }
    public string? Tema { get; set; }
}
