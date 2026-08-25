namespace ProjetoExtensao.Entities
{
    public class TipoNotificacao(
        string descricao
        )
    {
        public long Id { get; set; }

        public string Descricao { get; set; } = descricao;
    }
}
