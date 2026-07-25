# Frentes De Trabalho

Este é o registro que o comando `executar` usa para localizar automaticamente a tarefa da branch atual.

| Situação | Branch | Worktree | Executor | Tarefa | Contrato | Relatório |
|---|---|---|---|---|---|---|
| APROVADA | `feature/pix-manual-backend` | `SalgadosFacil-Backend` | Claude Code | `docs/TAREFAS/ATIVAS/PIX-MANUAL-001-BACKEND.md` | `docs/CONTRATOS/PIX-MANUAL-001.md` | `docs/RELATORIOS/PIX-MANUAL-001-BACKEND.md` |
| APROVADA | `feature/pix-manual-frontend` | `SalgadosFacil-Frontend` | Cursor | `docs/TAREFAS/ATIVAS/PIX-MANUAL-001-FRONTEND.md` | `docs/CONTRATOS/PIX-MANUAL-001.md` | `docs/RELATORIOS/PIX-MANUAL-001-FRONTEND.md` |

## Situações Permitidas

`PLANEJADA` → `PRONTA` → `EM EXECUÇÃO` → `ENTREGUE` → `APROVADA` → `INTEGRADA`

Somente o gestor no Principal altera este arquivo. Cada executor escreve apenas seu relatório exclusivo.
