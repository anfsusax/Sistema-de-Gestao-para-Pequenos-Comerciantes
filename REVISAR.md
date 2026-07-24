# Revisar Entregas Paralelas

Executar somente no worktree Principal quando o usuário disser `revisar entregas`.

## Responsabilidades

- Executores entregam somente código e relatório no working tree.
- Executores nunca fazem stage, commit, merge, rebase ou push.
- O gestor é o único responsável por build final, diff, classificação, commit e integração.

## Procedimento

1. Leia `docs/FRENTES.md`.
2. Para cada frente, inspecione a branch, o working tree e o relatório exclusivo sem descartar alterações.
3. Compare cada entrega com sua base e revise:
   - contrato;
   - escopo e propriedade de arquivos;
   - build e testes;
   - segurança e dados sensíveis;
   - migrations e arquivos compartilhados;
   - riscos de integração.
4. Execute localmente as validações que o sandbox do executor não conseguiu executar.
5. Classifique cada entrega como `APROVADA`, `AJUSTES` ou `REPROVADA`.
6. Para entrega `AJUSTES` ou `REPROVADA`, não crie commit do código: registre os apontamentos e devolva a frente ao executor.
7. Para entrega `APROVADA`, crie na branch da frente um commit pequeno contendo somente código e relatório aprovados e registre o hash.
8. Defina a ordem de integração e o trabalho de ligação entre implementações.
9. Não faça merge automaticamente. Apresente o diagnóstico e solicite autorização para integrar as aprovadas.

Se uma entrega antiga já possuir commit criado antes desta política, preserve o histórico e revise normalmente; não reescreva commits apenas para adequação processual.

## Resultado

Atualize `docs/RELATORIOS/INTEGRACAO.md` após a revisão e apresente um resumo claro ao usuário.