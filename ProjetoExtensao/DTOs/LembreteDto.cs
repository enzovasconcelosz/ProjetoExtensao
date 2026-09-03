namespace ProjetoExtensao.DTOs;

public class LembreteDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime DataHoraLembrete { get; set; }
    public DateTime DataHoraRegistro { get; set; }
    public long? IdTipoLembrete { get; set; }
    public long? IdTipoNotificacao { get; set; }
    public long? IdUsuario { get; set; }
}
