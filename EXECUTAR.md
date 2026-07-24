# Executar A Tarefa Da Worktree

Este protocolo permite que qualquer IA execute uma tarefa com o comando único `executar`.

## 1. Identificar A Frente

1. Execute `git branch --show-current`.
2. Localize exatamente essa branch em `docs/FRENTES.md`.
3. Confirme que a situação é `PRONTA` ou `EM EXECUÇÃO`.
4. Leia o arquivo de tarefa e o contrato indicados na mesma linha.
5. Leia `docs/WORKTREE.md` e `docs/ESTADO.md` apenas no que for relevante.

Se a branch não estiver cadastrada, pare e informe: `Nenhuma tarefa cadastrada para esta branch.`

## 2. Verificar Isolamento

- Confirme que a worktree está limpa antes de editar.
- Trabalhe somente nesta worktree e nesta branch.
- Não edite arquivos pertencentes a outra frente.
- Não altere `docs/FRENTES.md`; esse registro pertence ao gestor no Principal.
- Não faça merge, rebase, push ou migration, salvo autorização explícita na tarefa.

## 3. Trabalhar De Forma Autônoma

- Execute toda a tarefa sem solicitar confirmações intermediárias.
- O contrato congelado é a fonte da verdade entre frentes.
- Não espere outra frente terminar.
- Quando uma dependência real ainda não existir, use somente o adapter, fake, fixture ou sandbox previsto no contrato e na tarefa.
- Um fake deve implementar o mesmo contrato da implementação real e nunca pode esconder divergência de tipos, estados ou mensagens.
- Não invente campos, endpoints, regras ou arquitetura.

## 4. Validar

- Execute build, testes e cenários obrigatórios descritos na tarefa.
- Revise `git diff` e confirme que todos os arquivos estão no escopo permitido.
- Confirme que nenhum segredo, dado privado ou artefato local entrou no diff.

## 5. Entregar

1. Preencha o relatório exclusivo indicado em `docs/FRENTES.md`.
2. Registre resumo, arquivos alterados, testes, build, limitações, riscos e pendências.
3. Crie um commit pequeno e descritivo incluindo código e relatório.
4. Não faça push, salvo instrução explícita.
5. Responda com: tarefa, resultado, build/testes, commit e eventuais decisões necessárias.

A tarefa só está entregue quando código, validação, relatório e commit estiverem concluídos.