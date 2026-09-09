using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;
using ProjetoExtensao.Infrastructure.Repositories;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Os demais testes usam o provedor InMemory, que ignora o mapeamento relacional.
/// Aqui o banco e um SQLite de verdade, criado pelo mesmo EnsureCreated que roda
/// no aparelho: e o que garante que o aplicativo instalado consegue criar as
/// tabelas e gravar os dados no primeiro uso.
/// </summary>
public class EsquemaSqliteTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _contexto;
    private readonly UsuarioAtualFake _sessao = new() { Id = 1 };

    public EsquemaSqliteTests()
    {
        // ":memory:" some quando a conexao fecha, entao ela fica aberta pelo teste
        _conexao = new SqliteConnection("Data Source=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexao)
            .Options;

        _contexto = new AppDbContext(opcoes);
        _contexto.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexao.Dispose();
    }

    [Fact]
    public void EnsureCreated_criaAsTabelasDoAplicativo()
    {
        var tabelas = new List<string>();

        using var comando = _conexao.CreateCommand();
        comando.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";

        using var leitor = comando.ExecuteReader();
        while (leitor.Read())
            tabelas.Add(leitor.GetString(0));

        Assert.Contains("Usuario", tabelas);
        Assert.Contains("Lembrete", tabelas);
        Assert.Contains("TipoLembrete", tabelas);
        Assert.Contains("PreferenciaUsuario", tabelas);
    }

    [Fact]
    public async Task Lembrete_gravadoNoSqlite_eLidoDeVolta()
    {
        var quando = new DateTime(2026, 10, 5, 14, 30, 0);
        var repositorio = new LembreteRepository(_contexto, _sessao);

        // Diferente do InMemory, o SQLite cobra as chaves estrangeiras: o dono e
        // o tipo do lembrete precisam existir antes.
        await CriarDonoETipoAsync();

        await repositorio.AddAsync(
            new Lembrete("Consulta", "Levar exames", quando)
            {
                IdTipoLembrete = _contexto.TipoLembretes.Single().Id
            });

        var salvos = await repositorio.GetAllAsync();
        var lembrete = Assert.Single(salvos);

        Assert.Equal("Consulta", lembrete.Nome);
        Assert.Equal("Levar exames", lembrete.Descricao);
        // A data precisa voltar identica: o SQLite nao tem tipo de data proprio
        Assert.Equal(quando, lembrete.DataHoraLembrete);
    }

    /// <summary>
    /// Linhas minimas de que um lembrete depende para ser gravado. O usuario e
    /// salvo antes do tipo porque o Id dele so existe depois da insercao.
    /// </summary>
    private async Task CriarDonoETipoAsync()
    {
        var dono = new Usuario("ana@exemplo.com", "hash", "Ana");

        _contexto.Usuarios.Add(dono);
        await _contexto.SaveChangesAsync();

        _sessao.Id = dono.Id;

        _contexto.TipoLembretes.Add(new TipoLembrete("Saúde") { IdUsuario = dono.Id });
        await _contexto.SaveChangesAsync();
    }

    [Fact]
    public async Task Usuario_gravadoNoSqlite_recebeIdGerado()
    {
        var usuario = new Usuario("ana@exemplo.com", "hash", "Ana");

        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync();

        Assert.True(usuario.Id > 0);
    }
}
