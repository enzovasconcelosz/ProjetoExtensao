# Não Me Esquece

Aplicativo de lembretes desenvolvido como Projeto de Extensão do curso de Engenharia de
Software (UNIFIL). O objetivo é ajudar pessoas que precisam de apoio para lembrar de
compromissos recorrentes — consultas, medicamentos, contas — com uma interface simples e
tema claro/escuro.

- **Documentação e diagramas:** <https://enzovasconcelosz.github.io/ProjetoExtensao/>
- **Download do aplicativo:** [Releases](https://github.com/enzovasconcelosz/ProjetoExtensao/releases)

---

## Sumário

1. [O que o sistema faz](#o-que-o-sistema-faz)
2. [Tecnologias](#tecnologias)
3. [Arquitetura](#arquitetura)
4. [Modelo de dados](#modelo-de-dados)
5. [Como configurar](#como-configurar)
6. [Como executar](#como-executar)
7. [Testes](#testes)
8. [Publicação e distribuição](#publicação-e-distribuição)
9. [Notificações](#notificações)
10. [Acessibilidade](#acessibilidade)
11. [Estrutura de pastas](#estrutura-de-pastas)

---

## O que o sistema faz

| Funcionalidade | Descrição |
| --- | --- |
| Cadastro e login | Conta com login, senha e e-mail de contato. A senha é gravada com PBKDF2/SHA-256. |
| Recuperação de senha | Envio de código de 6 dígitos por e-mail, válido por 10 minutos e de uso único. |
| Lembretes | Cadastro com nome, tipo, data/hora, descrição e imagem opcional. |
| Tipos de lembrete | Categorias criadas pelo próprio usuário (Saúde, Casa, Trabalho…). |
| Notificações | Aviso no aparelho na hora do lembrete, mesmo com o aplicativo fechado. |
| Calendário | Visão mensal com destaque nos dias que possuem lembrete. |
| Tela inicial | Próximos lembretes e a semana atual com os dias marcados. |
| Aparência | Tema claro, escuro ou o do sistema. |
| Acessibilidade | Botões descritos para leitor de tela em todas as telas. |

## Tecnologias

- **.NET 10** e **.NET MAUI** (Android, iOS, MacCatalyst e Windows)
- **Entity Framework Core** com **SQLite** (banco local, no próprio aparelho)
- **xUnit** para os testes automatizados
- **Plugin.LocalNotification** para os avisos agendados no aparelho
- **SMTP** para o envio dos códigos de recuperação de senha

## Arquitetura

O projeto segue uma separação em camadas, com a regra de negócio isolada da interface e
do banco de dados:

```
Páginas XAML  ──►  Application/Services  ──►  Application/Interfaces
(telas MAUI)       (regra de negócio)         (contratos)
                          │                        ▲
                          ▼                        │
                  Application/Validacoes    Infrastructure/Repositories
                  (regras de preenchimento)  (EF Core → SQLite)
```

Dois pontos importantes do desenho:

- **A validação vive em um lugar só.** `ValidacaoLembrete` e `ValidacaoTipoLembrete` são
  usadas pela tela (para orientar o usuário, campo a campo) e pelo serviço (para impedir
  que qualquer outro caminho grave um dado inválido).
- **O serviço não conhece o banco.** Ele depende de `ILembreteRepository`, o que permite
  testá-lo com um repositório em memória, sem banco nenhum instalado.

## Modelo de dados

Os dados ficam em um arquivo SQLite (`naomeesquece.db3`) dentro da área privada do
aplicativo no aparelho. As tabelas são criadas no primeiro uso, pelo próprio EF Core, e
seguem o nome no singular:

- **Usuario** — login, senha (hash), nome; referencia `Imagem`, `ContatoEletronico`,
  `Aparencia` e `PreferenciaUsuario`.
- **Lembrete** — nome, descrição, data/hora do lembrete e do registro; referencia
  `TipoLembrete` e `TipoNotificacao`.
- **TipoLembrete** — nome da categoria; pertence a um `Usuario` e pode ter imagem.
- **ContatoEletronico** — e-mail do usuário.
- **PreferenciaUsuario** — se o usuário quer ser notificado (`FlNotificar`) e como o
  aviso chega: vibrando (`FlVibrar`), tocando (`FlSom`) ou os dois.
- **Aparencia** — tema escolhido.
- **TipoNotificacao** — forma de notificação do lembrete.

O diagrama entidade-relacionamento está na
[página do projeto](https://enzovasconcelosz.github.io/ProjetoExtensao/).

## Como configurar

### Pré-requisitos

| Item | Versão |
| --- | --- |
| .NET SDK | 10.0 ou superior |
| Workload MAUI | `dotnet workload install maui` |
| Visual Studio 2022/2026 | opcional, com a carga "Desenvolvimento para dispositivos móveis com .NET" |

### 1. Clonar o repositório

```bash
git clone https://github.com/enzovasconcelosz/ProjetoExtensao.git
```

### 2. Configurar o e-mail

O banco não precisa de configuração: o arquivo SQLite é criado automaticamente na primeira
execução, dentro da pasta do aplicativo.

As configurações ficam em `ProjetoExtensao/appsettings.json`. Para desenvolvimento local,
use `appsettings.Development.json` — ele **não** é versionado, então as suas senhas não vão
para o repositório.

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "User": "seu-endereco@gmail.com",
    "Password": "sua-senha-de-aplicativo",
    "From": "seu-endereco@gmail.com",
    "EnableSsl": "true"
  }
}
```

> **Nunca** coloque a senha real em `appsettings.json`. No Gmail, use uma
> [senha de aplicativo](https://support.google.com/accounts/answer/185833) em vez da senha
> da conta.

Sem as credenciais de SMTP o aplicativo funciona normalmente; apenas a recuperação de
senha por e-mail fica indisponível.

## Como executar

### Windows

```bash
dotnet build ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-windows10.0.19041.0
```

```bash
dotnet run --project ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-windows10.0.19041.0
```

### Android (emulador ou aparelho conectado)

```bash
dotnet build ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-android -t:Run
```

Pelo Visual Studio, basta selecionar o destino (Windows Machine ou o emulador Android) e
pressionar F5.

## Testes

Os testes automatizados cobrem as regras que não dependem de tela nem de banco: validações,
serviços, hash de senha e o código de recuperação.

```bash
dotnet test Tests/ProjetoExtensao.Tests/ProjetoExtensao.Tests.csproj
```

| Arquivo | O que verifica |
| --- | --- |
| `ValidacaoLembreteTests` | Nome mínimo, tipo obrigatório, data futura, descrição obrigatória. |
| `ValidacaoTipoLembreteTests` | Nome mínimo da categoria. |
| `LembreteServiceTests` | Nada inválido chega ao repositório, mesmo por fora da tela. |
| `SenhaHashTests` | Senha nunca é gravada em texto puro; senha errada é recusada. |
| `ConfirmationCodeServiceTests` | Código de uso único, limite de tentativas, expiração, isolamento por e-mail. |
| `EscolhaNotificacaoTests` | Cada combinação de vibrar/tocar tem canal próprio e estável; nada é agendado com o aviso desligado ou hora no passado. |

O roteiro dos testes com usuários reais está em
[`docs/teste-de-usabilidade.md`](docs/teste-de-usabilidade.md).

## Publicação e distribuição

O passo a passo completo — geração e distribuição do APK — está em
[`docs/hospedagem.md`](docs/hospedagem.md).

## Notificações

Quando chega a hora do lembrete, o aplicativo emite um aviso no aparelho. O agendamento
é local: quem guarda o alarme é o sistema operacional, então o aviso chega **com o
aplicativo fechado e sem internet**.

### Escolhendo como o aviso chega

Em Configurações → Notificações:

| Opção | Efeito |
| --- | --- |
| Avisar na hora do lembrete | Liga e desliga os avisos. |
| Vibrar | O aparelho vibra quando o aviso chega. |
| Tocar som | Além de vibrar, toca o som de notificação. |
| Testar aviso | Dispara um aviso de exemplo em 5 segundos. |

A escolha fica no aparelho (é ele quem dispara o aviso) e é copiada para a tabela
`PreferenciaUsuario`, acompanhando a conta em outro aparelho.

### Por que existem quatro canais de notificação

No Android 8 em diante são os **canais** — e não a notificação — que definem som e
vibração, e um canal não muda depois de criado. Por isso o aplicativo registra quatro
canais na inicialização, um para cada combinação de vibrar/tocar, e a escolha do usuário
decide qual deles recebe o aviso.

É também por isso que **alterar a preferência reagenda os lembretes já cadastrados**: um
aviso marcado no canal antigo continuaria se comportando do jeito antigo.

### Quando os avisos são criados e removidos

| Ação | O que acontece com o aviso |
| --- | --- |
| Salvar um lembrete | O aviso é agendado para a data e hora escolhidas. |
| Editar um lembrete | O aviso anterior é substituído. |
| Excluir um lembrete | O aviso é cancelado. |
| Alterar as preferências | Todos os lembretes futuros são reagendados. |
| Entrar na conta | Os avisos da conta passam a existir também neste aparelho. |
| Sair da conta | Os avisos agendados são cancelados. |

O agendamento vive no aparelho, não no banco — por isso o login precisa recriá-lo: um
celular novo, ou uma reinstalação, entra sem nenhum aviso marcado.

### Permissões do Android

Declaradas em `Platforms/Android/AndroidManifest.xml`:

| Permissão | Para quê |
| --- | --- |
| `POST_NOTIFICATIONS` | Exibir notificações. Obrigatória a partir do Android 13; o aplicativo pede ao usuário na primeira vez. |
| `VIBRATE` | Fazer o aparelho vibrar. |
| `SCHEDULE_EXACT_ALARM` / `USE_EXACT_ALARM` | Avisar na hora marcada, e não "por volta" dela. |
| `RECEIVE_BOOT_COMPLETED` | Remarcar os alarmes depois que o aparelho é reiniciado. |

> Se os avisos não chegarem, verifique se as notificações do aplicativo estão liberadas
> nas configurações do aparelho e se ele não está com economia de bateria agressiva para
> o aplicativo. O botão **Testar aviso** confirma em 5 segundos se está tudo certo.

## Acessibilidade

- **Tema:** claro, escuro ou o do sistema.
- **Leitor de tela:** todos os botões sem texto (voltar, menu, adicionar, foto de perfil,
  navegação do calendário) possuem descrição e dica em português; os ícones decorativos
  são omitidos da árvore de acessibilidade para não poluir a leitura.
- **Área de toque:** os controles de ícone têm no mínimo 40×40 dp.

## Estrutura de pastas

```
ProjetoExtensao/
├── ProjetoExtensao/              Aplicativo .NET MAUI
│   ├── *.xaml / *.xaml.cs        Telas
│   ├── Application/              Serviços, interfaces e validações
│   ├── Domain/ Entities/         Entidades do domínio
│   ├── DTOs/ Mappings/           Objetos de transporte e conversões
│   ├── Infrastructure/           DbContext e repositórios (EF Core)
│   ├── Services/                 Hash de senha, e-mail, tema, imagens
│   └── Resources/                Estilos, cores, fontes e imagens
├── Tests/ProjetoExtensao.Tests/  Testes automatizados (xUnit)
└── docs/                         Página do GitHub Pages e documentação
```

## Créditos

Projeto acadêmico desenvolvido por **Enzo Vasconcelos de Morais** para a disciplina de
Projeto de Extensão da UNIFIL.
