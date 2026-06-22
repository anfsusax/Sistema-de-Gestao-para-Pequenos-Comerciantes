# SalgadosPro

Sistema web de gestão comercial para pequenas fábricas e lojas de salgados.

## Executar

```bash
cd src/SalgaFacil.Web
dotnet run
```

Acesse `https://localhost:5xxx` (porta exibida no terminal).

**Login demo:** `maria@salgadospro.com` / `123456`

## Stack

- .NET 10 + Blazor Server
- EF Core + SQLite (`salgadospro.db` na pasta do Web)
- Clean Architecture: Domain → Infrastructure → Web

## Módulos

| Rota | Funcionalidade |
|------|----------------|
| `/` | Dashboard com KPIs, gráficos e entregas |
| `/pedidos` | Lista, filtros, novo pedido, detalhe com mudança de status |
| `/produtos` | CRUD completo com filtros frito/assado/ativo |
| `/producao` | Totais por produto e pedidos ativos |
| `/clientes` | CRUD com histórico de pedidos |
| `/custos` | Margem por produto e resumo mensal |
| `/configuracoes` | Dados da empresa e logout |

## Design

- Sidebar navy `#1a1d2e`
- Acento terracota `#d4500f`
- Fundo off-white `#f5f0eb`
- Tipografia Plus Jakarta Sans
