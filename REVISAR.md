# Revisar Entregas Paralelas

Executar somente no worktree Principal quando o usuário disser `revisar entregas`.

## Procedimento

1. Leia `docs/FRENTES.md`.
2. Para cada frente, inspecione a branch e a worktree sem alterar seu conteúdo.
3. Leia o relatório exclusivo diretamente na branch.
4. Compare a branch com sua base e revise:
   - contrato;
   - escopo e propriedade de arquivos;
   - build e testes;
   - segurança e dados sensíveis;
   - migrations e arquivos compartilhados;
   - riscos de integração.
5. Classifique cada entrega como `APROVADA`, `AJUSTES` ou `REPROVADA`.
6. Defina a ordem de integração e o trabalho de ligação entre implementações.
7. Não faça merge automaticamente. Apresente o diagnóstico e solicite autorização para integrar as aprovadas.

## Resultado

Atualize `docs/RELATORIOS/INTEGRACAO.md` somente após a revisão e apresente um resumo claro ao usuário.