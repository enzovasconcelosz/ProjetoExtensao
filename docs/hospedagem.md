# Publicação e hospedagem

O Não Me Esquece é um aplicativo .NET MAUI: ele roda no aparelho do usuário, não em um
navegador. "Hospedar a aplicação" aqui significa duas coisas:

1. **Colocar o banco de dados em um servidor**, para que qualquer aparelho com o
   aplicativo instalado acesse os mesmos dados;
2. **Distribuir o instalador**, para que os usuários consigam baixar e instalar.

```
Celular do usuário                    Servidor
┌──────────────────┐                 ┌────────────────────┐
│  Não Me Esquece  │ ──── TCP 1433 ──►│  SQL Server / Azure │
│  (APK instalado) │                 │  banco NaoMeEsquece │
└──────────────────┘                 └────────────────────┘
        ▲
        │ download do APK
   GitHub Releases
```

---

## Parte 1 — Banco de dados no servidor

### Opção A: Azure SQL Database (recomendada)

O nível **Basic** custa poucos reais por mês e a Azure oferece crédito para contas de
estudante ([Azure for Students](https://azure.microsoft.com/pt-br/free/students/), sem
cartão de crédito).

1. No [portal da Azure](https://portal.azure.com), crie um **SQL Database**:
   - Nome do banco: `NaoMeEsquece`
   - Servidor: crie um novo, anote o nome (`seuservidor.database.windows.net`), o login e a senha do administrador
   - Camada de computação: **Basic** ou **Serverless (General Purpose)**
2. Em **Rede**, marque "Permitir que serviços e recursos do Azure acessem este servidor" e
   adicione o seu IP à regra de firewall.
3. Conecte-se pelo SQL Server Management Studio ou pelo editor de consultas do portal e
   execute os scripts de `ProjetoExtensao/Scripts` em ordem numérica para criar as tabelas.
4. No aplicativo, ajuste a cadeia de conexão em `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:seuservidor.database.windows.net,1433;Database=NaoMeEsquece;User ID=seulogin;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

> A senha do banco **não** pode ser publicada no GitHub. Mantenha-a apenas em
> `appsettings.Development.json`, que já está no `.gitignore`.

### Opção B: SQL Server em uma VPS

Se preferir um servidor próprio (Contabo, Hostinger, DigitalOcean…):

1. Instale o SQL Server Express no servidor.
2. Habilite a autenticação **SQL Server e Windows** (autenticação mista).
3. Habilite o protocolo TCP/IP no SQL Server Configuration Manager e libere a porta 1433
   no firewall.
4. Crie um login exclusivo do aplicativo, com permissão apenas no banco `NaoMeEsquece`.
5. Use a cadeia de conexão:
   `Server=IP_DO_SERVIDOR,1433;Database=NaoMeEsquece;User ID=appnaomeesquece;Password=SUA_SENHA;TrustServerCertificate=True;`

### Checklist de segurança

- [ ] Login do aplicativo **não** é o administrador (`sa`) e só enxerga o banco `NaoMeEsquece`
- [ ] Conexão com criptografia (`Encrypt=True` na Azure)
- [ ] Firewall liberado apenas para as origens necessárias
- [ ] Senha fora do controle de versão
- [ ] Backup automático ativo (na Azure já vem ligado)

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
- [ ] Cadastro de conta funcionando contra o banco no servidor
- [ ] Lembrete criado em um aparelho aparece ao entrar com a mesma conta em outro
- [ ] Recuperação de senha enviando e-mail
- [ ] Aviso do lembrete chegando no horário, com o aplicativo fechado
- [ ] Link do Releases abrindo para quem não tem acesso ao repositório
