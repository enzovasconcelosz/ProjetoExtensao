namespace ProjetoExtensao.Entities
{
    public class Aparencia(
        string descricao
        )
    {
        public long Id { get; set; }

        public string Descricao { get; set; } = descricao;
    }
}
