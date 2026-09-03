# Teste de usabilidade — Não Me Esquece

Roteiro para avaliar o aplicativo com usuários reais. Complementa os testes automatizados
(`dotnet test`), que verificam as regras internas; aqui o que se avalia é se a pessoa
**consegue usar** o aplicativo sem ajuda.

## Perfil dos participantes

De 5 a 8 pessoas — a partir do quinto participante os mesmos problemas começam a se
repetir. Procure incluir:

- ao menos 2 pessoas com mais de 60 anos (público-alvo principal);
- ao menos 1 pessoa sem familiaridade com aplicativos de agenda.

## Como conduzir

1. Explique que **o aplicativo está sendo testado, não a pessoa**. Não há resposta errada.
2. Peça que ela **pense em voz alta**: o que está procurando, o que espera que aconteça.
3. **Não ajude** enquanto a pessoa tenta. Só intervenha depois de 2 minutos travada — e
   registre isso como falha da tarefa.
4. Cronometre cada tarefa e anote onde a pessoa hesitou.
5. Peça autorização antes de gravar a tela ou o áudio.

Comece com um aparelho zerado (aplicativo recém-instalado, sem conta criada).

## Tarefas

| # | Tarefa | Sucesso quando… | Tempo esperado |
| --- | --- | --- | --- |
| 1 | Criar uma conta no aplicativo | Chega à tela inicial logada | até 2 min |
| 2 | Criar o tipo de lembrete "Saúde" | O tipo aparece na lista | até 1 min |
| 3 | Cadastrar um lembrete de consulta médica para amanhã de manhã | O lembrete aparece na tela inicial | até 3 min |
| 4 | Descobrir em que dia do mês há lembretes | Abre o calendário e aponta o dia destacado | até 1 min |
| 5 | Fazer o aplicativo avisar sem tocar som, só vibrando | Desliga "Tocar som" nas notificações | até 2 min |
| 6 | Conferir que o aviso realmente chega | Usa "Testar aviso" e recebe a notificação | até 1 min |
| 7 | Mudar o aplicativo para o tema escuro | O tema muda | até 1 min |
| 8 | Alterar a foto do perfil | A nova foto aparece na tela inicial | até 2 min |
| 9 | Editar o horário do lembrete criado na tarefa 3 | O novo horário aparece na lista | até 2 min |
| 10 | Excluir esse lembrete | O lembrete some da lista | até 1 min |
| 11 | Sair da conta e recuperar a senha por e-mail | Consegue entrar com a nova senha | até 4 min |

## Ficha de registro (uma por participante)

```
Participante: ____  Idade: ____  Usa smartphone há: ____  Data: __/__/____

Tarefa 1  [ ] concluiu sozinho  [ ] com ajuda  [ ] não concluiu   Tempo: ____
  Onde hesitou: ______________________________________________
  O que falou:  ______________________________________________
(repetir para as 11 tarefas)

Ao final, de 1 a 5:
  Foi fácil de usar                  1  2  3  4  5
  Entendi o que cada tela fazia      1  2  3  4  5
  Usaria este aplicativo no dia a dia 1 2  3  4  5

O que mais te incomodou? _____________________________________
O que mais gostou?      _____________________________________
```

## Verificação de acessibilidade (feita pela equipe, sem usuário)

| Item | Como verificar | Esperado |
| --- | --- | --- |
| Leitor de tela | Ativar o TalkBack (Android) ou o Narrador (Windows) e percorrer todas as telas | Todo botão é anunciado com um nome em português; nenhum é lido como "botão" sem descrição |
| Ícones decorativos | Mesma varredura | As setas "›" das listas e o "+" dentro do botão redondo não são anunciados isoladamente |
| Tema escuro | Alternar tema em todas as telas | Nenhum texto com contraste insuficiente |
| Área de toque | Medir os botões de ícone | Mínimo de 40×40 dp |
| Rotação | Girar o aparelho nas telas de cadastro | O formulário continua utilizável |
| Notificação com o app fechado | Cadastrar um lembrete para dali a 2 minutos, fechar o aplicativo e aguardar | O aviso chega mesmo com o aplicativo fechado |
| Permissão negada | Recusar a permissão de notificação e abrir a tela de Notificações | O aplicativo avisa que o sistema bloqueou, sem travar |

## Consolidação dos resultados

Depois de todos os participantes, monte a tabela abaixo e priorize o que for **crítico**
(impede concluir a tarefa) e o que se repetiu em 2 ou mais pessoas.

| Problema observado | Nº de participantes | Gravidade | Correção aplicada |
| --- | --- | --- | --- |
| | | Crítica / Média / Baixa | |

> Registre aqui as correções que entraram no aplicativo — é essa tabela que sustenta o
> item "refinar a interface com base no feedback obtido" do projeto.

### Correções já aplicadas nesta etapa

| Origem | Problema | Correção |
| --- | --- | --- |
| Verificação de acessibilidade | Botões de ícone (voltar, menu, adicionar, foto de perfil, navegação do calendário) não eram anunciados pelo leitor de tela | Adicionadas descrições e dicas de acessibilidade em português em todas as telas |
| Verificação de acessibilidade | As setas "›" e o sinal "+" eram lidos isoladamente, sem sentido | Marcados como decorativos e removidos da árvore de acessibilidade |
| Verificação de acessibilidade | As linhas do menu de configurações não anunciavam para onde levavam | Cada linha passou a ter descrição e dica ("Abre a lista de lembretes", etc.) |
