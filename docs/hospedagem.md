# Publicação e distribuição

O Não Me Esquece é um aplicativo .NET MAUI: ele roda no aparelho do usuário, não em um
navegador. Os dados ficam em um banco **SQLite dentro do próprio aparelho**, então
publicar o aplicativo se resume a **distribuir o instalador** — não há servidor de banco
para manter, configurar ou pagar.

```
        Celular do usuário
┌────────────────────────────────┐
│  Não Me Esquece (APK)          │
│  ┌──────────────────────────┐  │
│  │ naomeesquece.db3 (SQLite)│  │
│  └──────────────────────────┘  │
└────────────────────────────────┘
                ▲
                │ download do APK
         GitHub Releases
```

---

## Parte 1 — O banco de dados

### Onde os dados ficam

O arquivo `naomeesquece.db3` é criado em `FileSystem.AppDataDirectory`, a área privada do
aplicativo no aparelho. Nenhum outro aplicativo consegue lê-lo, e ele é removido junto com
o aplicativo quando o usuário o desinstala.

As tabelas são criadas sozinhas no primeiro uso: `Conexao.GarantirBancoCriado()` roda na
inicialização (`MauiProgram`) e chama o `EnsureCreated` do EF Core. **Não há nada para
configurar** — o usuário baixa, instala e o aplicativo já grava.

### O que isso significa na prática

| | |
| --- | --- |
| Funciona sem internet | Sim — os lembretes não dependem de rede |
| Precisa de servidor, senha ou firewall | Não |
| Custo de hospedagem | Nenhum |
| Dados visíveis a outros aplicativos | Não |
| Dados compartilhados entre aparelhos | **Não** — cada aparelho tem o seu banco |

### Limitação conhecida

Como o banco vive no aparelho, **trocar de celular significa começar do zero**: os
lembretes não acompanham a conta. O login e a separação por usuário continuam valendo
dentro do mesmo aparelho (duas pessoas podem usar o mesmo celular sem ver os lembretes uma
da outra), mas não há sincronização.

A evolução natural do projeto, se a sincronização passar a ser necessária, é colocar uma
**API REST** entre o aplicativo e um banco em nuvem. O desenho em camadas já favorece
isso: as telas falam com `Application/Services`, que dependem das interfaces de
repositório — bastaria uma implementação de `ILembreteRepository` que chame a API, sem
mexer em nenhuma tela.

> Conectar o aplicativo **direto** a um banco em nuvem (Azure SQL, por exemplo) seria mais
> simples, mas exigiria embutir a senha do banco dentro do APK e expor o servidor à
> internet — qualquer pessoa poderia extrair a senha do arquivo instalado e acessar os
> dados de todos os usuários. Por isso esse caminho foi descartado.

### Por que não usar `Preferences`

O MAUI oferece `Preferences`, um armazenamento de chave/valor parecido com o `localStorage`
do navegador. O projeto o usa para configurações simples — tema, sessão, preferências de
notificação. Ele **não** serve para os lembretes: não há como consultar por período nem
relacionar lembrete, tipo e usuário em um dicionário de strings. Por isso os dados
relacionais ficam no SQLite.

---

## Parte 2 — Gerar e publicar o aplicativo

### Android (APK)

```bash
dotnet publish ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-android -c Release
```

O `.apk` fica em `ProjetoExtensao/bin/Release/net10.0-android/publish/`.

Para gerar um APK assinado (necessário para a Play Store e recomendado para distribuição
direta), crie uma chave uma única vez:

```bash
keytool -genkeypair -v -keystore naomeesquece.keystore -alias naomeesquece -keyalg RSA -keysize 2048 -validity 10000
```

E publique usando essa chave:

```bash
dotnet publish ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=naomeesquece.keystore -p:AndroidSigningKeyAlias=naomeesquece -p:AndroidSigningKeyPass=SUA_SENHA -p:AndroidSigningStorePass=SUA_SENHA
```

> O arquivo `.keystore` e as senhas **não** vão para o repositório. Se a chave for perdida,
> não é mais possível publicar atualizações do mesmo aplicativo.

### Windows

```bash
dotnet publish ProjetoExtensao/ProjetoExtensao.csproj -f net10.0-windows10.0.19041.0 -c Release
```

### Publicar no GitHub Releases

1. No repositório, abra **Releases → Draft a new release**.
2. Crie a tag `v1.0.0` e o título "Não Me Esquece 1.0.0".
3. Anexe o `.apk` (e o executável Windows, se quiser).
4. Na descrição, informe: o que mudou, a versão mínima do Android e o aviso de que o
   Android pedirá confirmação para instalar um aplicativo fora da Play Store.
5. Publique. O link fica disponível em
   `https://github.com/enzovasconcelosz/ProjetoExtensao/releases`.

### Como o usuário instala

1. Abre o link do Releases no celular e baixa o `.apk`.
2. O Android pergunta se permite instalar de fontes desconhecidas — o usuário autoriza
   apenas para o navegador usado no download.
3. Abre o arquivo baixado e confirma a instalação.

---

## Verificação final

- [ ] Aplicativo instalado em um aparelho **diferente** do de desenvolvimento
- [ ] Cadastro de conta funcionando já na primeira abertura (o banco se cria sozinho)
- [ ] Lembrete criado continua lá depois de fechar e reabrir o aplicativo
- [ ] Duas contas no mesmo aparelho não enxergam os lembretes uma da outra
- [ ] Recuperação de senha enviando e-mail
- [ ] Aviso do lembrete chegando no horário, com o aplicativo fechado
- [ ] Link do Releases abrindo para quem não tem acesso ao repositório
