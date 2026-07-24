# Executar A Tarefa Da Worktree

Este protocolo permite que qualquer IA execute uma tarefa com o comando único `executar`, mesmo quando seu sandbox não possui acesso ao Git completo ou ao SDK do projeto.

## 1. Identificar A Frente

1. Tente executar `git branch --show-current`.
2. Se o Git estiver indisponível porque a pasta `.git` aponta para fora do sandbox, não pare: identifique a frente pelo nome da pasta atual na coluna `Worktree` de `docs/FRENTES.md`.
3. Localize a linha correspondente e confirme que a situação é `PRONTA` ou `EM EXECUÇÃO`.
4. Leia a tarefa, o contrato e qualquer seção `RODADA DE AJUSTES DO GESTOR` indicada para a frente.
5. Leia `docs/WORKTREE.md` e `docs/ESTADO.md` apenas no que for relevante.

Pare somente se nenhuma branch nem worktree puder ser identificada de forma inequívoca.

## 2. Verificar Isolamento

- Trabalhe somente nesta worktree e nos arquivos permitidos pela tarefa.
- Se a tarefa contiver `Regra Excepcional De Retomada`, preserve as alterações locais listadas e continue sobre elas; não exija worktree limpa.
- Fora dessa exceção, confirme que a worktree está limpa quando o Git estiver disponível.
- Não altere `docs/FRENTES.md`; esse registro pertence ao gestor no Principal.
- Não faça merge, rebase, push ou migration, salvo autorização explícita na tarefa.

## 3. Trabalhar De Forma Autônoma

- Execute toda a implementação e todos os ajustes da tarefa sem solicitar confirmações intermediárias.
- O contrato congelado é a fonte da verdade entre frentes.
- Não espere outra frente terminar.
- Quando uma dependência real ainda não existir, use somente o adapter, fake, fixture ou sandbox previsto no contrato e na tarefa.
- Não invente campos, endpoints, regras ou arquitetura.

## 4. Ferramentas Indisponíveis

A indisponibilidade de Git, SDK, navegador ou rede no sandbox não bloqueia a edição dos arquivos e não deve gerar pergunta ao usuário.

- Não tente instalar SDK, obter root ou contornar allowlist de rede.
- Se `dotnet` não existir, faça as verificações estáticas possíveis, registre `BUILD PENDENTE DO GESTOR` e continue até concluir código e relatório.
- Se o Git não acessar os metadados da worktree, revise os arquivos permitidos diretamente, registre `DIFF E COMMIT PENDENTES DO GESTOR` e continue.
- Não afirme que executou build, teste, diff ou commit quando a ferramenta não estava disponível.
- O gestor no Principal executará automaticamente as validações locais e o commit pendentes na próxima revisão.

## 5. Validar

Quando as ferramentas estiverem disponíveis:

- execute build, testes e cenários obrigatórios;
- revise `git diff` e confirme o escopo;
- confirme que nenhum segredo, dado privado ou artefato local entrou no diff.

Quando não estiverem disponíveis, revise assinaturas, tipos, regras e arquivos manualmente e liste com precisão o que ficou para o gestor.

## 6. Entregar

1. Preencha o relatório exclusivo indicado em `docs/FRENTES.md`.
2. Registre resumo, arquivos alterados, validações reais, limitações e pendências do gestor.
3. Se o Git estiver disponível, crie o commit pequeno e descritivo solicitado.
4. Se o Git estiver indisponível, não peça acesso e não peça que o usuário escolha: encerre como `EXECUTOR CONCLUÍDO — AGUARDANDO VALIDAÇÃO DO GESTOR`.
5. Não faça push.
6. Responda com: tarefa, resultado da implementação, validações executadas e pendências automáticas do gestor.

A IA executora conclui quando código e relatório estão prontos. A entrega só é aprovada depois que o gestor validar build, diff, commit e integração.