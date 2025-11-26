namespace ProjetoExtensao.Entities
{
    public class ContatoEletronico(
        string descricao
        )
    {
        public long Id { get; set; }

        public string Descricao { get; set; } = descricao;
    }
}