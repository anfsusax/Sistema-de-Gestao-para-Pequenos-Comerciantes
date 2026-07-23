# SalgaFacil.Desktop

Aplicação **Windows Forms** (SalgaPro) — sidebar navy (`#1a1d2e`) + acento terracota
(`#d4500f`), alinhada ao design system do Web (SalgadosPro, `src/SalgaFacil.Web/wwwroot/app.css`).

Parte da solution `SalgaFacil`, junto com Domain, Infrastructure e Web.

## Executar

```bash
dotnet run
```

Login: `admin@salgapro.com` / `123456`

## Pastas

- `Forms/` — Login, Principal (sidebar), modais de cadastro
- `Controls/` — UserControls das telas (Dashboard, Produtos, Clientes, Pedidos, Producao, Custos, Configuracoes)
- `Models/`, `Services/` — dados em memória (evolução futura: Infrastructure)
- `Helpers/WinStyles.cs` — paleta e estilos compartilhados (navy/terracota, cards de métrica, grids, botões)

## Limitações Conhecidas Do Redesign

Cantos arredondados e sombra suave (box-shadow) do Web não são reproduzidos — exigiriam
desenho customizado via GDI+ (`GraphicsPath`/owner-draw). A fonte "Plus Jakarta Sans" do
Web também não está embutida; usa-se "Segoe UI" como substituto mais próximo já instalado
no Windows. Ver `_ia/DECISOES.md` (entrada de 2026-07-01, "Redesign Visual Do Desktop").
