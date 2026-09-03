namespace ProjetoExtensao.Application.Interfaces
{
    /// <summary>
    /// Identifica o usuario logado para os repositorios, que precisam filtrar
    /// os dados por conta sem depender de como a sessao e guardada.
    /// </summary>
    public interface IUsuarioAtual
    {
        /// <summary>Id do usuario logado, ou nulo quando ninguem entrou.</summary>
        long? Id { get; }
    }
}
