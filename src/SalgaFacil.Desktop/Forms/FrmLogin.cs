using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmLogin : Form
{
    private TextBox _txtEmail = null!;
    private TextBox _txtSenha = null!;
    private readonly AuthService _auth = new();

    public FrmLogin()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "SalgaPro — Login";
        Size = new Size(420, 420);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        // Cabeçalho com logo + subtítulo centralizados via Dock+TextAlign (sem calcular X manualmente
        // como antes: Point(130,24)/Point(155,52) só funcionavam para um tamanho de fonte específico).
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90 };
        var lblSub = new Label { Text = "Gestão Comercial", ForeColor = WinStyles.TextoMuted, Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleCenter };
        var lblLogo = new Label { Text = "🥟 SalgaPro", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = WinStyles.Navy, Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
        pnlHeader.Controls.Add(lblSub);
        pnlHeader.Controls.Add(lblLogo);

        var pnlCorpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40, 0, 40, 24) };

        // BUG CORRIGIDO: "pnl" e "campos" estavam com Dock=Fill + AutoSize=true ao mesmo tempo —
        // combinação contraditória em WinForms (Dock=Fill trava o tamanho ao espaço do pai,
        // então AutoSize nunca tinha efeito e o conteúdo que não coubesse era cortado sem aviso;
        // foi isso que cortou a caixa de Senha e escondeu os botões no print). Trocado para
        // Dock=Top: fixa a Largura (igual ao pai) mas deixa a Altura livre para o AutoSize calcular
        // a partir do conteúdo real.
        var pnl = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(16) };

        var campos = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

        _txtEmail = new TextBox { Width = 280, Text = "admin@salgapro.com" };
        WinStyles.AdicionarCampo(campos, "Email", _txtEmail);

        _txtSenha = new TextBox { Width = 280, PasswordChar = '●', Text = "123456" };
        WinStyles.AdicionarCampo(campos, "Senha", _txtSenha);

        var pnlBotoes = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 14, 0, 0) };
        var btnEntrar = WinStyles.CriarBotao("Entrar", true);
        btnEntrar.Click += BtnEntrar_Click;
        var btnRecuperar = WinStyles.CriarBotao("Recuperar senha");
        btnRecuperar.Margin = new Padding(8, 0, 0, 0);
        btnRecuperar.Click += (_, _) => MessageBox.Show("Entre em contato com o administrador.", "Recuperar senha", MessageBoxButtons.OK, MessageBoxIcon.Information);
        pnlBotoes.Controls.AddRange([btnEntrar, btnRecuperar]);
        campos.Controls.Add(pnlBotoes);

        pnl.Controls.Add(campos);
        pnlCorpo.Controls.Add(pnl);

        Controls.Add(pnlCorpo);
        Controls.Add(pnlHeader);
        AcceptButton = btnEntrar;
    }

    private void BtnEntrar_Click(object? sender, EventArgs e)
    {
        if (_auth.Login(_txtEmail.Text.Trim(), _txtSenha.Text))
        {
            Hide();
            using var principal = new FrmPrincipal();
            principal.ShowDialog();
            Close();
        }
        else
            MessageBox.Show("Email ou senha incorretos.", "Erro de login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
