# Índice De Contratos

Contratos compartilhados são congelados no Principal antes da abertura das frentes paralelas.

| Contrato | Versão | Situação | Documento | Código compartilhado |
|---|---|---|---|---|
| PIX-MANUAL-001 | 1 | CONGELADO | `docs/CONTRATOS/PIX-MANUAL-001.md` | `src/SalgaFacil.Web/Contracts/Pagamentos/PagamentoPixContracts.cs` |

## Regras

- Backend e Frontend consomem exatamente o mesmo contrato.
- Mudança durante a execução exige nova versão e coordenação do gestor.
- Executor não altera contrato congelado.
- Test doubles implementam a mesma interface e são identificados como simulados.
- Contrato não cria API REST, Controller ou nova camada arquitetural.