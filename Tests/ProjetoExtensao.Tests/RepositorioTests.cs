using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;
using ProjetoExtensao.Infrastructure.Repositories;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Comportamento dos repositorios dentro de uma unica conta.
///
/// O <see cref="IsolamentoPorUsuarioTests"/> cobre a separacao entre contas;
/// aqui o que se verifica e que, para o proprio dono, gravar, ler, editar e
/// excluir continuam funcionando depois do filtro por usuario.
/// </summary>
public class RepositorioTests : IDisposable
{
    private const long Ana = 1;

    private readonly AppDbContext _contexto;
    private readonly UsuarioAtualFake _sessao = new() { Id = Ana };

    public RepositorioTests()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"NaoMeEsquece-{Guid.NewGuid()}")
            .Options;

        _contexto = new AppDbContext(opcoes);
    }

    public void Dispose() => _contexto.Dispose();

    private LembreteRepository RepositorioLembrete() => new(_contexto, _sessao);

    private TipoLembreteRepository RepositorioTipo() => new(_contexto, _sessao);

    private static Lembrete Lembrete(string nome, DateTime? quando = null) =>
        new(nome, "Descrição", quando ?? DateTime.Now.AddDays(1)) { IdTipoLembrete = 1 };

    // ---------- Lembretes ----------

    [Fact]
    public async Task Busca_por_id_devolve_o_proprio_lembrete()
    {
        var repositorio = RepositorioLembrete();
        var lembrete = Lembrete("Consulta");
        await repositorio.AddAsync(lembrete);

        var encontrado = await repositorio.GetByIdAsync(lembrete.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("Consulta", encontrado!.Nome);
    }

    [Fact]
    public async Task Busca_por_id_inexistente_devolve_nulo()
    {
        Assert.Null(await RepositorioLembrete().GetByIdAsync(999));
    }

    [Fact]
    public async Task Lista_vem_ordenada_pela_data_do_lembrete()
    {
        var repositorio = RepositorioLembrete();
        var baseData = DateTime.Now.AddDays(1);

        await repositorio.AddAsync(Lembrete("Depois", baseData.AddDays(2)));
        await repositorio.AddAsync(Lembrete("Antes", baseData));
        await repositorio.AddAsync(Lembrete("No meio", baseData.AddDays(1)));

        var lista = await repositorio.GetAllAsync();

        Assert.Equal(new[] { "Antes", "No meio", "Depois" }, lista.Select(l => l.Nome));
    }

    [Fact]
    public async Task Excluir_remove_o_lembrete_da_lista()
    {
        var repositorio = RepositorioLembrete();
        var lembrete = Lembrete("Consulta");
        await repositorio.AddAsync(lembrete);

        await repositorio.DeleteAsync(lembrete.Id);

        Assert.Empty(await repositorio.GetAllAsync());
    }

    [Fact]
    public async Task Excluir_um_id_inexistente_nao_quebra()
    {
        var repositorio = RepositorioLembrete();
        await repositorio.AddAsync(Lembrete("Consulta"));

        await repositorio.DeleteAsync(999);

        Assert.Single(await repositorio.GetAllAsync());
    }

    [Fact]
    public async Task Editar_altera_a_data_do_proprio_lembrete()
    {
        var repositorio = RepositorioLembrete();
        var lembrete = Lembrete("Consulta");
        await repositorio.AddAsync(lembrete);

        var novaData = DateTime.Now.AddDays(5);
        var alterado = new Lembrete("Consulta", "Descrição", novaData)
        {
            Id = lembrete.Id,
            IdTipoLembrete = 1
        };

        await repositorio.UpdateAsync(alterado);

        var atual = await repositorio.GetByIdAsync(lembrete.Id);

        Assert.Equal(novaData, atual!.DataHoraLembrete);
    }

    // ---------- Tipos de lembrete ----------

    [Fact]
    public async Task Tipos_vem_ordenados_pelo_nome()
    {
        var repositorio = RepositorioTipo();

        await repositorio.AddAsync(new TipoLembrete("Trabalho"));
        await repositorio.AddAsync(new TipoLembrete("Casa"));
        await repositorio.AddAsync(new TipoLembrete("Saúde"));

        var lista = await repositorio.GetAllAsync();

        Assert.Equal(new[] { "Casa", "Saúde", "Trabalho" }, lista.Select(t => t.Nome));
    }

    [Fact]
    public async Task Busca_por_id_devolve_o_proprio_tipo()
    {
        var repositorio = RepositorioTipo();
        var tipo = new TipoLembrete("Saúde");
        await repositorio.AddAsync(tipo);

        var encontrado = await repositorio.GetByIdAsync(tipo.Id);

        Assert.Equal("Saúde", encontrado!.Nome);
    }

    [Fact]
    public async Task Editar_renomeia_o_proprio_tipo()
    {
        var repositorio = RepositorioTipo();
        var tipo = new TipoLembrete("Saude");
        await repositorio.AddAsync(tipo);

        var alterado = new TipoLembrete("Saúde") { Id = tipo.Id };
        await repositorio.UpdateAsync(alterado);

        var lista = await repositorio.GetAllAsync();

        Assert.Equal(new[] { "Saúde" }, lista.Select(t => t.Nome));
    }

    [Fact]
    public async Task Excluir_remove_o_tipo_da_lista()
    {
        var repositorio = RepositorioTipo();
        var tipo = new TipoLembrete("Saúde");
        await repositorio.AddAsync(tipo);

        await repositorio.DeleteAsync(tipo.Id);

        Assert.Empty(await repositorio.GetAllAsync());
    }
}
