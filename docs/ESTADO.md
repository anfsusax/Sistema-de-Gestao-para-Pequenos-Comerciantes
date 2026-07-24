# Estado Atual — SalgadosFácil

Atualizado em: 2026-07-24

## Resumo

O projeto está na fase de conclusão e validação do MVP. A solução compila, o fluxo público de compra está implementado no código, mas ainda falta validar o comportamento completo em runtime com PostgreSQL e navegador.

## Git E Ambientes

- Branch principal: `main`
- HEAD verificado: `9f1a61b`
- `main` sincronizada com `origin/main` no momento do diagnóstico
- Arquivos rastreados: sem alterações locais antes desta organização
- Pendência inicial encontrada: pasta `docs/` ainda não rastreada
- Worktrees no início do diagnóstico: somente o Principal
- Modelo operacional vigente: Principal, Backend e Frontend

## Estrutura Confirmada

| Projeto | Papel | Dependência |
|---|---|---|
| `SalgaFacil.Domain` | Entidades, enums e regras centrais | Nenhuma |
| `SalgaFacil.Infrastructure` | EF Core, PostgreSQL, DbContext e migrations | Domain |
| `SalgaFacil.Web` | Blazor Server e Services com acesso direto ao DbContext | Infrastructure |
| `SalgaFacil.Desktop` | WinForms isolado | Nenhuma ProjectReference |

Não existem camada Application, API REST separada, Controllers, Repository ou Unit of Work.

## Validação Técnica

- Comando: `dotnet build SalgaFacil.slnx --no-restore`
- Resultado em 2026-07-24: sucesso, 0 erros e 11 avisos
- Avisos de código: duas possíveis desreferências nulas em `Components/Pages/Pdv/Index.razor`
- Avisos de dependências: vulnerabilidades conhecidas de alta gravidade reportadas para `SQLitePCLRaw.lib.e_sqlite3` e `System.Security.Cryptography.Xml`
- Testes automatizados: nenhum projeto de testes encontrado
- Runtime com PostgreSQL e smoke tests no navegador: ainda não validado nesta entrega

## Funcionalidades Presentes No Código

- Cadastros de categorias, unidades, produtos e clientes
- Loja pública por slug
- Carrinho persistente e seletor de quantidade
- Checkout com forma de pagamento
- Criação e consulta de pedidos por telefone
- Administração de pedidos
- PDV/caixa, produção, custos e dashboard
- Upload de imagem de produto
- Tratamento de clientes duplicados por telefone normalizado

## Problemas E Riscos Conhecidos

1. Fluxo principal do MVP ainda sem validação ponta a ponta em runtime.
2. Limpeza de clientes duplicados deve preceder qualquer índice UNIQUE.
3. Persistência de autenticação ainda é limitada ao circuito atual.
4. Não há testes automatizados.
5. Há avisos de vulnerabilidade em dependências restauradas.
6. `Program.cs`, DI, Auth, DbContext, migrations, layouts e CSS global são áreas compartilhadas de alto risco de conflito.
7. O repositório mistura terminadores de linha e ainda não possui uma decisão formal sobre `.gitattributes`.

## Divergências Registradas

- `_ia/ESTADO.md` informa HEAD `68e065b`, alterações sem commit e remoto em `2714c5d`; o Git real está em `9f1a61b`, sincronizado com `origin/main`, sem alterações rastreadas no início deste diagnóstico.
- O `WORKTREE.md` original recomenda worktrees por módulo. A missão atual determinou ambientes Backend e Frontend; a adaptação foi registrada na seção 13 do próprio documento, preservando o mapa modular.
- O nome aparece como SalgaFacil, SalgadosFácil, SalgadosPro e SalgaPro em pontos históricos. Nenhum projeto foi renomeado.

## Próximo Passo Recomendado

Executar no ambiente Principal uma validação ponta a ponta do fluxo:

`loja pública → produto → quantidade → carrinho → cliente → checkout → pedido → confirmação → meus pedidos → alteração administrativa de status`.

Nenhuma nova funcionalidade deve ser iniciada antes de registrar o resultado desse baseline.
