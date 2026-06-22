# SalgaFacil — Projetos

Sistema de gestão comercial para pequenas fábricas e lojas de salgados.

## Estrutura

```
src/
├── SalgaFacil.Domain/          Entidades e enums compartilhados
├── SalgaFacil.Infrastructure/  EF Core, banco de dados
├── SalgaFacil.Web/             Interface Blazor (SalgadosPro)
└── SalgaFacil.Desktop/         Interface Windows Forms (SalgaPro)
```

## Executar — Web (Blazor)

```bash
cd src/SalgaFacil.Web
dotnet run
```

Login: `maria@salgadospro.com` / `123456`

## Executar — Desktop (Windows Forms)

```bash
cd src/SalgaFacil.Desktop
dotnet run
```

Login: `admin@salgapro.com` / `123456`

## Visual Studio

Abra `SalgaFacil.slnx` na raiz e defina o projeto de inicialização (Web ou Desktop).
