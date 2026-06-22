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
        Size = new Size(420, 340);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        var lblLogo = new Label { Text = "🥟 SalgaPro", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 58, 95), AutoSize = true, Location = new Point(130, 24) };
        var lblSub = new Label { Text = "Gestão Comercial", ForeColor = Color.Gray, AutoSize = true, Location = new Point(155, 52) };

        var pnl = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Location = new Point(40, 80), Size = new Size(330, 200) };

        var lblEmail = new Label { Text = "Email", Location = new Point(16, 16), AutoSize = true };
        _txtEmail = new TextBox { Location = new Point(16, 34), Width = 296, Text = "admin@salgapro.com" };

        var lblSenha = new Label { Text = "Senha", Location = new Point(16, 68), AutoSize = true };
        _txtSenha = new TextBox { Location = new Point(16, 86), Width = 296, PasswordChar = '●', Text = "123456" };

        var btnEntrar = WinStyles.CriarBotao("Entrar", true);
        btnEntrar.Location = new Point(16, 130);
        btnEntrar.Width = 140;
        btnEntrar.Click += BtnEntrar_Click;

        var btnRecuperar = WinStyles.CriarBotao("Recuperar senha");
        btnRecuperar.Location = new Point(172, 130);
        btnRecuperar.Width = 140;
        btnRecuperar.Click += (_, _) => MessageBox.Show("Entre em contato com o administrador.", "Recuperar senha", MessageBoxButtons.OK, MessageBoxIcon.Information);

        pnl.Controls.AddRange([lblEmail, _txtEmail, lblSenha, _txtSenha, btnEntrar, btnRecuperar]);
        Controls.AddRange([lblLogo, lblSub, pnl]);
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
