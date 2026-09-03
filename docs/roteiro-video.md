# Roteiro da apresentação em vídeo (até 10 minutos)

Vídeo demonstrando os testes, as melhorias de interface, a hospedagem e a página do
GitHub Pages.

## Antes de gravar

- [ ] Banco no servidor no ar e com dados de exemplo (2 tipos de lembrete e 3 lembretes)
- [ ] APK já publicado no GitHub Releases
- [ ] Página do GitHub Pages publicada e abrindo
- [ ] Terminal aberto na pasta do projeto, fonte ampliada (ninguém enxerga fonte 12 em vídeo)
- [ ] Aparelho Android espelhado na tela (`scrcpy`) ou emulador aberto
- [ ] Notificações do computador silenciadas
- [ ] Microfone testado — áudio ruim estraga um vídeo bom

**Gravação:** OBS Studio (gratuito) ou a Gravação de Jogo do Windows (`Win + Alt + R`).
Grave em 1080p e faça uma passagem de ensaio cronometrada antes da definitiva.

---

## Estrutura (9 minutos, com folga para o limite de 10)

### 1. Abertura — 0:00 a 0:40

Mostrando a tela inicial do aplicativo.

> "Olá, eu sou o Enzo Vasconcelos de Morais, do curso de Engenharia de Software da
> UNIFIL. Este é o Não Me Esquece, um aplicativo de lembretes feito para quem
> precisa de apoio para lembrar de consultas, medicamentos e contas. Nesta etapa do projeto
> de extensão eu vou mostrar quatro coisas: os testes, as melhorias de interface, a
> hospedagem e a página de documentação do projeto."

### 2. O aplicativo funcionando — 0:40 a 2:30

Demonstre o fluxo completo, sem narrar cada clique:

1. Login
2. Criar um tipo de lembrete ("Saúde")
3. Cadastrar um lembrete de consulta para amanhã
4. Mostrar o lembrete aparecendo na tela inicial
5. Abrir o calendário e mostrar o dia destacado
6. Cadastrar um lembrete para dali a um minuto, fechar o aplicativo e mostrar a
   notificação chegando

> "Repare que o dia com lembrete aparece destacado tanto na semana da tela inicial quanto
> no calendário — foi um pedido que apareceu nos testes com usuários."

E, sobre a notificação:

> "O aviso é agendado no sistema operacional, não no aplicativo. Por isso ele chega mesmo
> com o aplicativo fechado e sem internet — que é exatamente a situação de quem precisa
> ser lembrado de tomar um remédio."

Mostre também a tela de Notificações e desligue o som, deixando só a vibração.

> "No Android, som e vibração vêm do canal da notificação, e um canal não muda depois de
> criado. Então o aplicativo registra um canal para cada combinação, e trocar a escolha
> reagenda os lembretes que já estavam marcados."

### 3. Testes automatizados — 2:30 a 4:00

No terminal, execute e deixe o resultado na tela:

```bash
dotnet test Tests/ProjetoExtensao.Tests/ProjetoExtensao.Tests.csproj
```

> "São 52 testes automatizados cobrindo as regras que não podem falhar: as validações do
> lembrete, o serviço que grava no banco, o hash da senha e o código de recuperação."

Abra um teste e mostre um exemplo concreto — sugestão: o teste que prova que um lembrete
inválido **não** chega ao repositório.

> "Este aqui é o que mais importa: mesmo que uma tela futura esqueça de validar, o serviço
> barra o dado inválido antes do banco."

Depois, mostre uma validação acontecendo na tela: tente salvar um lembrete com data no
passado e mostre a mensagem aparecendo.

> "É a mesma regra rodando: a tela e o serviço usam a mesma validação."

### 4. Testes de usabilidade e melhorias — 4:00 a 6:00

Mostre o roteiro (`docs/teste-de-usabilidade.md`) e a tabela de correções.

> "Os testes com usuários foram feitos com [N] pessoas, incluindo [descrever o perfil].
> Cada uma recebeu nove tarefas e eu registrei onde travaram."

Cite dois ou três problemas encontrados e mostre a correção **na tela**, lado a lado se
possível.

Demonstre as melhorias de acessibilidade ao vivo:

1. Configurações → Aparência → alterne para o tema escuro e percorra duas telas
2. Ative o TalkBack e toque no botão de voltar, no menu e no botão de adicionar

> "Todos os botões de ícone agora têm nome em português para o leitor de tela. Antes, o
> TalkBack anunciava apenas 'botão', sem dizer o que ele fazia."

### 5. Hospedagem — 6:00 a 7:30

1. Mostre o banco no portal da Azure (ou no servidor escolhido)
2. Mostre a cadeia de conexão apontando para o servidor — **oculte a senha**
3. Crie um lembrete no aparelho e mostre o registro aparecendo na consulta ao banco
4. Abra a página de Releases no GitHub e mostre o APK publicado
5. Se possível, instale em um segundo aparelho e entre com a mesma conta

> "O banco está hospedado no Azure SQL, então os dados não ficam presos a um aparelho: o
> mesmo usuário entra em qualquer celular e vê os mesmos lembretes. O aplicativo é
> distribuído pelo GitHub Releases."

### 6. Página do GitHub Pages — 7:30 a 8:40

Abra `https://enzovasconcelosz.github.io/ProjetoExtensao/` e percorra:

- a descrição do projeto
- o diagrama de arquitetura
- o diagrama entidade-relacionamento
- o fluxo de navegação das telas
- os links para o repositório, o download e a documentação

> "A página reúne os diagramas e as informações iniciais do projeto, e serve como ponto de
> entrada para quem quiser conhecer ou instalar o aplicativo."

### 7. Encerramento — 8:40 a 9:00

> "Esse foi o Não Me Esquece: testado, hospedado, documentado e disponível para download.
> Obrigado."

---

## Dicas

- **Não leia o roteiro.** Use-o como lista de tópicos; a fala natural rende melhor.
- **Corte os tempos mortos.** Compilação e instalação podem ser aceleradas ou cortadas.
- **Mostre, depois explique.** Ação primeiro, narração em cima dela.
- **Nunca mostre senhas** — nem a do banco, nem a do e-mail, nem a da keystore.
- Se estourar os 10 minutos, encurte a seção 2: ela é a que o avaliador já conhece.
