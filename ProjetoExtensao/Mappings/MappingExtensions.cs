using System;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Mappings;

public static class MappingExtensions
{
    // Usuario
    public static UsuarioDto ToDto(this Usuario u)
    {
        if (u == null) return null!;
        return new UsuarioDto
        {
            Id = u.Id,
            Login = u.Login,
            Senha = u.Senha,
            NomeUsuario = u.NomeUsuario,
            IdImagem = u.IdImagem,
            IdContatoEletronico = u.IdContatoEletronico,
            IdAparencia = u.IdAparencia,
            IdPreferenciaUsuario = u.IdPreferenciaUsuario,
            DataHoraRegistro = u.DataHoraRegistro
        };
    }

    public static Usuario ToEntity(this UsuarioDto dto)
    {
        if (dto == null) return null!;
        var u = new Usuario(dto.Login ?? string.Empty, dto.Senha ?? string.Empty, dto.NomeUsuario ?? string.Empty);
        u.Id = dto.Id;
        u.IdImagem = dto.IdImagem;
        u.IdContatoEletronico = dto.IdContatoEletronico;
        u.IdAparencia = dto.IdAparencia;
        u.IdPreferenciaUsuario = dto.IdPreferenciaUsuario;
        u.DataHoraRegistro = dto.DataHoraRegistro;
        return u;
    }

    // ImagemUsuario
    public static ImagemDto ToDto(this ImagemUsuario img)
    {
        if (img == null) return null!;
        return new ImagemDto { Id = img.Id, ArquivoImagem = img.ArquivoImagem, DataHoraRegistro = null };
    }

    public static ImagemUsuario ToEntity(this ImagemDto dto)
    {
        if (dto == null) return null!;
        var img = new ImagemUsuario(dto.ArquivoImagem ?? Array.Empty<byte>());
        img.Id = dto.Id;
        return img;
    }

    // ContatoEletronico
    public static ContatoEletronicoDto ToDto(this ContatoEletronico c)
    {
        if (c == null) return null!;
        return new ContatoEletronicoDto { Id = c.Id, Descricao = c.Descricao };
    }

    public static ContatoEletronico ToEntity(this ContatoEletronicoDto dto)
    {
        if (dto == null) return null!;
        var c = new ContatoEletronico(dto.Descricao ?? string.Empty);
        c.Id = dto.Id;
        return c;
    }

    // PreferenciaUsuario
    public static PreferenciaUsuarioDto ToDto(this PreferenciaUsuario p)
    {
        if (p == null) return null!;
        return new PreferenciaUsuarioDto { Id = p.Id, Notificar = p.Notificar, Vibrar = p.Vibrar, Som = p.Som, Tema = p.Tema.ToString() };
    }

    public static PreferenciaUsuario ToEntity(this PreferenciaUsuarioDto dto)
    {
        if (dto == null) return null!;
        TemaSistemaEnum tema = TemaSistemaEnum.Padrao;
        if (!string.IsNullOrWhiteSpace(dto.Tema)) Enum.TryParse<TemaSistemaEnum>(dto.Tema!, out tema);
        var p = new PreferenciaUsuario(dto.Notificar, tema);
        p.Id = dto.Id;
        p.Vibrar = dto.Vibrar;
        p.Som = dto.Som;
        return p;
    }

    // Aparencia
    public static AparenciaDto ToDto(this Aparencia a)
    {
        if (a == null) return null!;
        return new AparenciaDto { Id = a.Id, Descricao = a.Descricao };
    }

    public static Aparencia ToEntity(this AparenciaDto dto)
    {
        if (dto == null) return null!;
        var a = new Aparencia(dto.Descricao ?? string.Empty);
        a.Id = dto.Id;
        return a;
    }

    // TipoLembrete
    public static TipoLembreteDto ToDto(this TipoLembrete t)
    {
        if (t == null) return null!;
        return new TipoLembreteDto { Id = t.Id, Nome = t.Nome, DataHoraRegistro = t.DataHoraRegistro, IdImagemUsuario = t.IdImagemUsuario, IdUsuario = t.IdUsuario };
    }

    public static TipoLembrete ToEntity(this TipoLembreteDto dto)
    {
        if (dto == null) return null!;
        var t = new TipoLembrete(dto.Nome ?? string.Empty);
        t.Id = dto.Id;
        t.DataHoraRegistro = dto.DataHoraRegistro;
        t.IdImagemUsuario = dto.IdImagemUsuario;
        t.IdUsuario = dto.IdUsuario;
        return t;
    }

    // TipoNotificacao
    public static TipoNotificacaoDto ToDto(this TipoNotificacao t)
    {
        if (t == null) return null!;
        return new TipoNotificacaoDto { Id = t.Id, Descricao = t.Descricao };
    }

    public static TipoNotificacao ToEntity(this TipoNotificacaoDto dto)
    {
        if (dto == null) return null!;
        var t = new TipoNotificacao(dto.Descricao ?? string.Empty);
        t.Id = dto.Id;
        return t;
    }

    // Lembrete
    public static LembreteDto ToDto(this Lembrete e)
    {
        if (e == null) return null!;
        return new LembreteDto
        {
            Id = e.Id,
            Nome = e.Nome,
            Descricao = e.Descricao,
            DataHoraLembrete = e.DataHoraLembrete,
            DataHoraRegistro = e.DataHoraRegistro,
            IdTipoLembrete = e.IdTipoLembrete,
            IdTipoNotificacao = e.IdTipoNotificacao,
            IdUsuario = e.IdUsuario
        };
    }

    public static Lembrete ToEntity(this LembreteDto dto)
    {
        if (dto == null) return null!;
        var e = new Lembrete(dto.Nome ?? string.Empty, dto.Descricao ?? string.Empty, dto.DataHoraLembrete);
        e.Id = dto.Id;
        e.IdTipoLembrete = dto.IdTipoLembrete;
        e.IdTipoNotificacao = dto.IdTipoNotificacao;
        e.IdUsuario = dto.IdUsuario;
        e.DataHoraRegistro = dto.DataHoraRegistro;
        return e;
    }
}
