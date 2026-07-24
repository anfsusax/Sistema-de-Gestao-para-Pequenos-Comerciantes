# Executar A Tarefa Da Worktree

Este protocolo permite que qualquer IA execute uma tarefa com o comando único `executar`, inclusive em sandboxes sem acesso ao Git completo ou ao SDK do projeto.

## 1. Identificar A Frente

1. Tente executar `git branch --show-current`.
2. Se o Git estiver indisponível porque `.git` aponta para fora do sandbox, identifique a frente pelo nome da pasta atual na coluna `Worktree` de `docs/FRENTES.md`.
3. Localize a linha correspondente e confirme que a situação é `PRONTA` ou `EM EXECUÇÃO`.
4. Leia a tarefa, o contrato e qualquer seção `RODADA DE AJUSTES DO GESTOR`.
5. Leia `docs/WORKTREE.md` e `docs/ESTADO.md` apenas no que for relevante.

Pare somente se nenhuma frente puder ser identificada de forma inequívoca.

## 2. Verificar Isolamento

- Trabalhe somente nesta worktree e nos arquivos permitidos pela tarefa.
- Se a tarefa contiver `Regra Excepcional De Retomada`, preserve as alterações locais listadas e continue sobre elas.
- Não descarte nem sobrescreva trabalho anterior.
- Não altere `docs/FRENTES.md`; esse registro pertence ao gestor no Principal.
- Não execute `git add`, `git commit`, merge, rebase, push ou migration.

## 3. Trabalhar De Forma Autônoma

- Execute toda a implementação e todos os ajustes sem solicitar confirmações intermediárias.
- O contrato congelado é a fonte da verdade entre frentes.
- Não espere outra frente terminar.
- Use somente adapters, fakes, fixtures ou sandboxes previstos no contrato e na tarefa.
- Não invente campos, endpoints, regras ou arquitetura.

## 4. Ferramentas Indisponíveis

A ausência de Git, SDK, navegador ou rede não bloqueia a edição e não deve gerar pergunta ao usuário.

- Não tente instalar SDK, obter root ou contornar allowlist.
- Se `dotnet` não existir, faça verificações estáticas, registre `BUILD PENDENTE DO GESTOR` e conclua código e relatório.
- Se o Git não acessar a worktree, revise diretamente os arquivos permitidos e registre `DIFF PENDENTE DO GESTOR`.
- Não afirme que executou build, teste ou diff quando a ferramenta não estava disponível.
- O gestor executará as validações locais posteriormente.

## 5. Validar

Quando as ferramentas estiverem disponíveis:

- execute build, testes e cenários obrigatórios;
- use o Git somente para leitura de status e diff;
- confirme que nenhum segredo, dado privado ou artefato local entrou nas alterações.

Quando não estiverem disponíveis, revise assinaturas, tipos, regras e arquivos manualmente e liste precisamente o que ficou para o gestor.

## 6. Entregar Ao Gestor

1. Preencha o relatório exclusivo indicado em `docs/FRENTES.md`.
2. Registre resumo, arquivos alterados, validações reais, limitações e pendências.
3. Não prepare stage e não crie commit, mesmo que o Git esteja disponível.
4. Não faça push.
5. Encerre com `EXECUTOR CONCLUÍDO — AGUARDANDO VALIDAÇÃO DO GESTOR`.

O executor entrega código e relatório no working tree. Ao receber `revisar entregas`, o gestor executa build, testes, diff, classificação e, quando a entrega estiver aprovada, cria o commit e conduz a integração.