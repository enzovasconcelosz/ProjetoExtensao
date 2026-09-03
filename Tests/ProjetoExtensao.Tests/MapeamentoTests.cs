using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Mappings;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Conversao entre entidade e DTO.
///
/// As telas trabalham com DTO e o banco com entidade; um campo esquecido na
/// conversao se perde silenciosamente. E o caso do dono do lembrete e das
/// preferencias de notificacao, campos novos que atravessam esse caminho toda
/// vez que a tela de edicao grava.
/// </summary>
public class MapeamentoTests
{
    // ---------- Lembrete ----------

    [Fact]
    public void Lembrete_leva_o_dono_para_o_dto()
    {
        var lembrete = new Lembrete("Consulta", "Cardiologista", new DateTime(2026, 3, 1, 9, 0, 0))
        {
            Id = 5,
            IdTipoLembrete = 2,
            IdUsuario = 9
        };

        Assert.Equal(9, lembrete.ToDto().IdUsuario);
    }

    [Fact]
    public void Lembrete_traz_o_dono_de_volta_do_dto()
    {
        var dto = new LembreteDto
        {
            Id = 5,
            Nome = "Consulta",
            Descricao = "Cardiologista",
            DataHoraLembrete = new DateTime(2026, 3, 1, 9, 0, 0),
            IdTipoLembrete = 2,
            IdUsuario = 9
        };

        Assert.Equal(9, dto.ToEntity().IdUsuario);
    }

    [Fact]
    public void Editar_um_lembrete_nao_perde_o_dono_no_caminho()
    {
        // Percurso real da tela de edicao: entidade -> DTO -> entidade
        var original = new Lembrete("Consulta", "Cardiologista", new DateTime(2026, 3, 1, 9, 0, 0))
        {
            Id = 5,
            IdTipoLembrete = 2,
            IdTipoNotificacao = 1,
            IdUsuario = 9
        };

        var voltou = original.ToDto().ToEntity();

        Assert.Equal(original.IdUsuario, voltou.IdUsuario);
        Assert.Equal(original.Id, voltou.Id);
        Assert.Equal(original.Nome, voltou.Nome);
        Assert.Equal(original.Descricao, voltou.Descricao);
        Assert.Equal(original.DataHoraLembrete, voltou.DataHoraLembrete);
        Assert.Equal(original.IdTipoLembrete, voltou.IdTipoLembrete);
        Assert.Equal(original.IdTipoNotificacao, voltou.IdTipoNotificacao);
    }

    [Fact]
    public void Lembrete_sem_dono_continua_sem_dono()
    {
        var lembrete = new Lembrete("Consulta", "Cardiologista", DateTime.Now);

        Assert.Null(lembrete.ToDto().IdUsuario);
    }

    // ---------- Tipo de lembrete ----------

    [Fact]
    public void Editar_um_tipo_nao_perde_o_dono_no_caminho()
    {
        var original = new TipoLembrete("Saúde") { Id = 3, IdUsuario = 9 };

        var voltou = original.ToDto().ToEntity();

        Assert.Equal(9, voltou.IdUsuario);
        Assert.Equal("Saúde", voltou.Nome);
        Assert.Equal(3, voltou.Id);
    }

    // ---------- Preferencia de notificacao ----------

    [Fact]
    public void Preferencia_leva_vibrar_e_som_para_o_dto()
    {
        var preferencia = new PreferenciaUsuario { Id = 1, Notificar = true, Vibrar = true, Som = false };

        var dto = preferencia.ToDto();

        Assert.True(dto.Notificar);
        Assert.True(dto.Vibrar);
        Assert.False(dto.Som);
    }

    [Fact]
    public void Preferencia_traz_vibrar_e_som_de_volta_do_dto()
    {
        var dto = new PreferenciaUsuarioDto { Id = 1, Notificar = true, Vibrar = false, Som = true };

        var preferencia = dto.ToEntity();

        Assert.True(preferencia.Notificar);
        Assert.False(preferencia.Vibrar);
        Assert.True(preferencia.Som);
    }

    [Fact]
    public void Preferencia_sobrevive_a_ida_e_volta()
    {
        var original = new PreferenciaUsuario { Id = 4, Notificar = true, Vibrar = false, Som = false };

        var voltou = original.ToDto().ToEntity();

        Assert.Equal(original.Notificar, voltou.Notificar);
        Assert.Equal(original.Vibrar, voltou.Vibrar);
        Assert.Equal(original.Som, voltou.Som);
        Assert.Equal(original.Id, voltou.Id);
    }

    [Fact]
    public void Preferencia_nova_ja_nasce_vibrando_e_tocando()
    {
        // Precisa corresponder ao DEFAULT (1) das colunas FlVibrar e FlSom:
        // um usuario que nunca abriu a tela de notificacoes recebe o aviso completo
        var preferencia = new PreferenciaUsuario();

        Assert.True(preferencia.Vibrar);
        Assert.True(preferencia.Som);
    }
}
