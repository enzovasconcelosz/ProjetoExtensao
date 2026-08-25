namespace ProjetoExtensao.DTOs;

public class EventoDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime DataHoraEvento { get; set; }
    public DateTime DataHoraRegistro { get; set; }
    public long? IdTipoEvento { get; set; }
    public long? IdTipoNotificacao { get; set; }
}
