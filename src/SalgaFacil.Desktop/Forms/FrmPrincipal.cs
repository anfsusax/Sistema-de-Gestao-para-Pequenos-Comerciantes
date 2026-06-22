using SalgaFacil.Desktop.Controls;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmPrincipal : Form
{
    private Panel _pnlMenu = null!;
    private Panel _pnlConteudo = null!;
    private Panel _pnlRodape = null!;
    private Label _lblRodapeDir = null!;
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
        Size = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        _pnlMenu = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = WinStyles.MenuBar, Padding = new Padding(4, 2, 4, 2) };
        var lblLogo = new Label { Text = "🥟 SalgaPro", Font = WinStyles.FonteLogo, ForeColor = Color.FromArgb(30, 58, 95), AutoSize = true, Location = new Point(8, 6) };

        var itens = new[] { "Dashboard", "Produtos", "Pedidos", "Produção", "Clientes", "Custos", "Configurações", "Sair" };
        int x = 110;
        foreach (var item in itens)
        {
            var btn = new Button
            {
                Text = item,
                FlatStyle = FlatStyle.Flat,
                Font = WinStyles.FontePadrao,
                Location = new Point(x, 3),
                Height = 24,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Tag = item
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += Menu_Click;
            _botoesMenu[item] = btn;
            _pnlMenu.Controls.Add(btn);
            x += btn.Width + 4;
        }
        _pnlMenu.Controls.Add(lblLogo);

        _pnlConteudo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = WinStyles.FundoGeral };

        _pnlRodape = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = WinStyles.Rodape, Padding = new Padding(12, 4, 12, 4) };
        var lblEsq = new Label { Text = "SalgaPro v1.0 — Gestão Comercial", ForeColor = Color.Gray, Dock = DockStyle.Left, AutoSize = true };
        _lblRodapeDir = new Label { Text = $"Usuário: {DataStore.UsuarioLogado?.Nome ?? "Maria"} | {DateTime.Now:dd/MM/yyyy}", ForeColor = Color.Gray, Dock = DockStyle.Right, AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
        _pnlRodape.Controls.Add(_lblRodapeDir);
        _pnlRodape.Controls.Add(lblEsq);

        Controls.Add(_pnlConteudo);
        Controls.Add(_pnlRodape);
        Controls.Add(_pnlMenu);
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
            kv.Value.BackColor = ativo ? WinStyles.MenuAtivo : WinStyles.MenuBar;
            kv.Value.Font = ativo ? new Font("Segoe UI", 9F, FontStyle.Bold) : WinStyles.FontePadrao;
        }

        _telaAtual?.Dispose();
        _pnlConteudo.Controls.Clear();
        uc.Dock = DockStyle.Fill;
        _pnlConteudo.Controls.Add(uc);
        _telaAtual = uc;
    }
}
