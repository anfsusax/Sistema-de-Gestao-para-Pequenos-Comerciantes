# Decisões Técnicas E De Governança

## 2026-07-24 — Preservar A Arquitetura Atual

**Contexto:** a solução real possui Domain, Infrastructure, Web e Desktop.

**Decisão:** não criar Application, API REST, Controllers, Repository, Unit of Work, CQRS, MediatR ou microserviços sem decisão formal futura.

**Impacto:** o Web continua usando Services internos e acesso ao DbContext de acordo com o padrão existente.

## 2026-07-24 — Três Ambientes De Trabalho

**Contexto:** a missão atual divide a execução entre gestão técnica, Cloud Code e Cursor.

**Decisão:** usar Principal (`main`), Backend (`feature/backend`) e Frontend (`feature/frontend`) em pastas irmãs.

**Impacto:** o mapa modular de `WORKTREE.md` continua orientando escopo e riscos, mas a separação física vigente é por responsabilidade técnica.

## 2026-07-24 — Principal É O Ponto De Integração

**Contexto:** arquivos compartilhados e migrations apresentam alto risco de conflito.

**Decisão:** documentação, revisão, smoke tests, resolução de conflitos e migrations autorizadas são coordenados no ambiente Principal.

**Impacto:** nenhuma entrega é integrada automaticamente. A ordem padrão é Backend, Frontend, ajustes, testes e documentação.

## 2026-07-24 — Migration Exclusiva E Autorizada

**Contexto:** o snapshot do EF Core é sequencial e conflita quando duas branches geram migrations.

**Decisão:** somente uma branch pode produzir migration por vez, após autorização explícita, sincronização com `main` e revisão do snapshot.

**Impacto:** a limpeza e validação de clientes duplicados não autorizam por si só a criação do índice UNIQUE.

## 2026-07-24 — Validar Antes De Expandir O MVP

**Contexto:** o fluxo público está implementado, mas não possui baseline runtime registrado.

**Decisão:** a próxima entrega é a validação ponta a ponta do fluxo principal.

**Impacto:** página de detalhes, carrinho flutuante, pacotes e outras melhorias ficam depois do baseline, salvo correção necessária para concluir o fluxo.
