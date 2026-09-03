using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;
using ProjetoExtensao.Infrastructure.Repositories;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Separacao dos dados por conta.
///
/// Ate a correcao, os repositorios liam a tabela inteira: quem criasse uma
/// conta nova enxergava os lembretes de todas as outras. O filtro vive no
/// repositorio, que e o unico caminho ate o banco, entao e aqui que ele precisa
/// ser garantido — uma tela nova nao pode reintroduzir o problema.
/// </summary>
public class IsolamentoPorUsuarioTests : IDisposable
{
    private const long Ana = 1;
    private const long Bruno = 2;

    private readonly AppDbContext _contexto;
    private readonly UsuarioAtualFake _sessao = new();

    public IsolamentoPorUsuarioTests()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"NaoMeEsquece-{Guid.NewGuid()}")
            .Options;

        _contexto = new AppDbContext(opcoes);
    }

    public void Dispose() => _contexto.Dispose();

    private LembreteRepository RepositorioLembrete() => new(_contexto, _sessao);

    private TipoLembreteRepository RepositorioTipo() => new(_contexto, _sessao);

    private static Lembrete Lembrete(string nome) =>
        new(nome, "Descrição", DateTime.Now.AddDays(1)) { IdTipoLembrete = 1 };

    // ---------- Lembretes ----------

    [Fact]
    public async Task Usuario_ve_apenas_os_proprios_lembretes()
    {
        _sessao.Id = Ana;
        await RepositorioLembrete().AddAsync(Lembrete("Consulta da Ana"));

        _sessao.Id = Bruno;
        await RepositorioLembrete().AddAsync(Lembrete("Consulta do Bruno"));

        var doBruno = await RepositorioLembrete().GetAllAsync();

        Assert.Equal(new[] { "Consulta do Bruno" }, doBruno.Select(l => l.Nome));
    }

    [Fact]
    public async Task Conta_nova_comeca_sem_nenhum_lembrete()
    {
        _sessao.Id = Ana;
        await RepositorioLembrete().AddAsync(Lembrete("Consulta da Ana"));

        _sessao.Id = Bruno;

        Assert.Empty(await RepositorioLembrete().GetAllAsync());
    }

    [Fact]
    public async Task Lembrete_gravado_recebe_o_dono_sem_a_tela_informar()
    {
        _sessao.Id = Ana;
        var lembrete = Lembrete("Consulta da Ana");

        await RepositorioLembrete().AddAsync(lembrete);

        Assert.Equal(Ana, lembrete.IdUsuario);
    }

    [Fact]
    public async Task Busca_por_id_nao_devolve_o_lembrete_de_outra_conta()
    {
        _sessao.Id = Ana;
        var daAna = Lembrete("Consulta da Ana");
        await RepositorioLembrete().AddAsync(daAna);

        _sessao.Id = Bruno;

        Assert.Null(await RepositorioLembrete().GetByIdAsync(daAna.Id));
    }

    [Fact]
    public async Task Ninguem_exclui_o_lembrete_de_outra_conta()
    {
        _sessao.Id = Ana;
        var daAna = Lembrete("Consulta da Ana");
        await RepositorioLembrete().AddAsync(daAna);

        _sessao.Id = Bruno;
        await RepositorioLembrete().DeleteAsync(daAna.Id);

        _sessao.Id = Ana;
        Assert.Single(await RepositorioLembrete().GetAllAsync());
    }

    [Fact]
    public async Task Ninguem_edita_o_lembrete_de_outra_conta()
    {
        _sessao.Id = Ana;
        var daAna = Lembrete("Consulta da Ana");
        await RepositorioLembrete().AddAsync(daAna);

        _sessao.Id = Bruno;
        var adulterado = new Lembrete("Sequestrado pelo Bruno", "Descrição", DateTime.Now.AddDays(2))
        {
            Id = daAna.Id,
            IdTipoLembrete = 1
        };

        await RepositorioLembrete().UpdateAsync(adulterado);

        _sessao.Id = Ana;
        var restantes = await RepositorioLembrete().GetAllAsync();

        Assert.Equal(new[] { "Consulta da Ana" }, restantes.Select(l => l.Nome));
    }

    [Fact]
    public async Task Usuario_edita_o_proprio_lembrete()
    {
        _sessao.Id = Ana;
        var repositorio = RepositorioLembrete();
        var daAna = Lembrete("Consulta");
        await repositorio.AddAsync(daAna);

        var alterado = new Lembrete("Consulta remarcada", "Descrição", DateTime.Now.AddDays(3))
        {
            Id = daAna.Id,
            IdTipoLembrete = 1
        };

        await repositorio.UpdateAsync(alterado);

        var restantes = await repositorio.GetAllAsync();

        Assert.Equal(new[] { "Consulta remarcada" }, restantes.Select(l => l.Nome));
    }

    [Fact]
    public async Task Sessao_encerrada_nao_enxerga_lembrete_nenhum()
    {
        _sessao.Id = Ana;
        await RepositorioLembrete().AddAsync(Lembrete("Consulta da Ana"));

        // Depois de sair da conta, a lista fica vazia — nao "todos os lembretes"
        _sessao.Id = null;

        Assert.Empty(await RepositorioLembrete().GetAllAsync());
    }

    // ---------- Tipos de lembrete ----------

    [Fact]
    public async Task Usuario_ve_apenas_os_proprios_tipos()
    {
        _sessao.Id = Ana;
        await RepositorioTipo().AddAsync(new TipoLembrete("Saúde"));

        _sessao.Id = Bruno;
        await RepositorioTipo().AddAsync(new TipoLembrete("Trabalho"));

        var doBruno = await RepositorioTipo().GetAllAsync();

        Assert.Equal(new[] { "Trabalho" }, doBruno.Select(t => t.Nome));
    }

    [Fact]
    public async Task Tipo_gravado_recebe_o_dono_sem_a_tela_informar()
    {
        _sessao.Id = Ana;
        var tipo = new TipoLembrete("Saúde");

        await RepositorioTipo().AddAsync(tipo);

        Assert.Equal(Ana, tipo.IdUsuario);
    }

    [Fact]
    public async Task Ninguem_exclui_o_tipo_de_outra_conta()
    {
        _sessao.Id = Ana;
        var daAna = new TipoLembrete("Saúde");
        await RepositorioTipo().AddAsync(daAna);

        _sessao.Id = Bruno;
        await RepositorioTipo().DeleteAsync(daAna.Id);

        _sessao.Id = Ana;
        Assert.Single(await RepositorioTipo().GetAllAsync());
    }

    [Fact]
    public async Task Sessao_encerrada_nao_enxerga_tipo_nenhum()
    {
        _sessao.Id = Ana;
        await RepositorioTipo().AddAsync(new TipoLembrete("Saúde"));

        _sessao.Id = null;

        Assert.Empty(await RepositorioTipo().GetAllAsync());
    }
}

/// <summary>
/// Sessao controlada pelo teste: troca de usuario sem passar pela tela de login.
/// </summary>
public class UsuarioAtualFake : IUsuarioAtual
{
    public long? Id { get; set; }
}
