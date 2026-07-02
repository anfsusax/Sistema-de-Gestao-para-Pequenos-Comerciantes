using SalgaFacil.Desktop.Controls;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

// Layout redesenhado em 2026-07-01 para aproximar o visual do SalgadosPro Web
// (src/SalgaFacil.Web/Components/Layout/MainLayout.razor + wwwroot/app.css):
// sidebar navy fixa à esquerda com item ativo em terracota sólido, em vez do
// menu horizontal no topo usado até então.
public class FrmPrincipal : Form
{
    private const int LarguraSidebar = 220;

    private Panel _pnlSidebar = null!;
    private Panel _pnlConteudoHost = null!;
    private Panel _pnlConteudo = null!;
    private readonly Dictionary<string, Button> _botoesMenu = new();
    private UserControl? _telaAtual;

    public FrmPrincipal()
    {
        DataStore.Inicializar();
        InitializeComponent();
        Navegar("Dashboard", new UcDashboard());
    }

    private void InitializeComponent()
    {
        Text = "SalgaPro — Gestão Comercial";
        Size = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 620);
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        MontarSidebar();
        MontarAreaPrincipal();

        Controls.Add(_pnlConteudoHost);
        Controls.Add(_pnlSidebar);
    }

    private void MontarSidebar()
    {
        _pnlSidebar = new Panel { Dock = DockStyle.Left, Width = LarguraSidebar, BackColor = WinStyles.Navy, Padding = new Padding(16, 20, 16, 16) };

        // Rodapé da sidebar (Configurações + Sair) — Dock=Bottom não conflita com os grupos
        // Dock=Top abaixo, então a ordem de Add aqui não importa.
        var pnlFooter = new Panel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 14, 0, 0) };
        var lblLinha = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(45, 48, 68) };
        var btnConfig = CriarBotaoNav("Configurações");
        btnConfig.Dock = DockStyle.Top;
        btnConfig.Click += Menu_Click;
        _botoesMenu["Configurações"] = btnConfig;
        var btnSair = CriarBotaoNav("Sair");
        btnSair.Dock = DockStyle.Top;
        btnSair.ForeColor = Color.FromArgb(240, 200, 190);
        btnSair.Click += Menu_Click;
        // Adiciona na ordem inversa da leitura: em WinForms, entre múltiplos filhos Dock=Top do
        // mesmo pai, o ÚLTIMO adicionado fica mais perto da borda (visualmente primeiro).
        pnlFooter.Controls.Add(btnSair);
        pnlFooter.Controls.Add(btnConfig);
        pnlFooter.Controls.Add(lblLinha);

        // Menu de navegação: FlowLayoutPanel (TopDown) com largura fixa em cada botão evita a
        // pegadinha de ordenação do Dock=Top (não precisa inverter a ordem de adição aqui).
        var pnlMenu = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        var itensMenu = new[] { "Dashboard", "Produtos", "Pedidos", "Produção", "Clientes", "Custos" };
        foreach (var item in itensMenu)
        {
            var btn = CriarBotaoNav(item);
            btn.Click += Menu_Click;
            _botoesMenu[item] = btn;
            pnlMenu.Controls.Add(btn);
        }

        // Bloco de marca (ícone quadrado terracota + nome + subtítulo), igual ao ".sidebar-brand" do Web.
        var pnlBrand = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 20) };
        var pnlIcone = new Panel { Size = new Size(36, 36), BackColor = WinStyles.Terracota };
        pnlIcone.Controls.Add(new Label { Text = "🥟", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 13F) });
        var pnlBrandTexto = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(10, 2, 0, 0) };
        pnlBrandTexto.Controls.Add(new Label { Text = "SalgaPro", ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0) });
        pnlBrandTexto.Controls.Add(new Label { Text = "Gestão Comercial", ForeColor = Color.FromArgb(160, 165, 180), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Margin = new Padding(0) });
        pnlBrand.Controls.Add(pnlIcone);
        pnlBrand.Controls.Add(pnlBrandTexto);

        // Ordem de Add do grupo Dock=Top (pnlMenu, pnlBrand): último adicionado fica no topo —
        // por isso pnlBrand (que deve ficar acima) é adicionado por último.
        _pnlSidebar.Controls.Add(pnlFooter);
        _pnlSidebar.Controls.Add(pnlMenu);
        _pnlSidebar.Controls.Add(pnlBrand);
    }

    private Button CriarBotaoNav(string texto)
    {
        var btn = new Button
        {
            Text = texto,
            Tag = texto,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(190, 195, 210),
            BackColor = WinStyles.Navy,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false,
            Width = LarguraSidebar - 32,
            Height = 34,
            Padding = new Padding(12, 0, 0, 0),
            Margin = new Padding(0, 0, 0, 4),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 48, 68);
        return btn;
    }

    private void MontarAreaPrincipal()
    {
        _pnlConteudoHost = new Panel { Dock = DockStyle.Fill };

        // Barra superior simplificada (equivalente ao topbar do Web: só informação do usuário
        // alinhada à direita, sem título/breadcrumb).
        var pnlTopo = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = WinStyles.FundoGeral, Padding = new Padding(24, 0, 24, 0) };
        var pnlUsuario = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        var nomeUsuario = DataStore.UsuarioLogado?.Nome ?? "Maria";
        var lblUsuarioTopo = new Label { Text = $"Olá, {nomeUsuario}", ForeColor = WinStyles.TextoMuted, Font = WinStyles.FontePadrao, AutoSize = true, Margin = new Padding(0, 16, 12, 0) };
        var pnlAvatar = new Panel { Size = new Size(34, 34), BackColor = WinStyles.Terracota, Margin = new Padding(0, 9, 0, 0) };
        pnlAvatar.Controls.Add(new Label { Text = nomeUsuario.Length > 0 ? nomeUsuario[..1].ToUpperInvariant() : "?", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) });
        pnlUsuario.Controls.Add(lblUsuarioTopo);
        pnlUsuario.Controls.Add(pnlAvatar);
        pnlTopo.Controls.Add(pnlUsuario);

        _pnlConteudo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 8, 24, 24), BackColor = WinStyles.FundoGeral };

        _pnlConteudoHost.Controls.Add(_pnlConteudo);
        _pnlConteudoHost.Controls.Add(pnlTopo);
    }

    private void Menu_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string item) return;
        if (item == "Sair") { Close(); return; }

        UserControl? uc = item switch
        {
            "Dashboard" => new UcDashboard(),
            "Produtos" => new UcProdutos(),
            "Pedidos" => new UcPedidos(),
            "Produção" => new UcProducao(),
            "Clientes" => new UcClientes(),
            "Custos" => new UcCustos(),
            "Configurações" => new UcConfiguracoes(),
            _ => null
        };
        if (uc != null) Navegar(item, uc);
    }

    private void Navegar(string item, UserControl uc)
    {
        foreach (var kv in _botoesMenu)
        {
            var ativo = kv.Key == item;
            kv.Value.BackColor = ativo ? WinStyles.Terracota : WinStyles.Navy;
            kv.Value.ForeColor = ativo ? Color.White : Color.FromArgb(190, 195, 210);
            kv.Value.Font = ativo ? new Font("Segoe UI", 9F, FontStyle.Bold) : new Font("Segoe UI", 9F);
        }

        _telaAtual?.Dispose();
        _pnlConteudo.Controls.Clear();
        uc.Dock = DockStyle.Fill;
        _pnlConteudo.Controls.Add(uc);
        _telaAtual = uc;
    }
}
