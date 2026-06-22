using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcConfiguracoes : UserControl
{
    private TextBox _txtNome = null!;
    private TextBox _txtCnpj = null!;
    private TextBox _txtTelefone = null!;

    public UcConfiguracoes()
    {
        BackColor = WinStyles.FundoGeral;
        Dock = DockStyle.Fill;
        BuildUi();
    }

    private void BuildUi()
    {
        var titulo = new Label { Text = "Configurações", Font = WinStyles.FonteTitulo, Dock = DockStyle.Top, Height = 32 };

        var grp = new GroupBox { Text = "Dados da Empresa", Dock = DockStyle.Top, Height = 200, Width = 400, Padding = new Padding(12), BackColor = WinStyles.PainelBranco };
        grp.FlatStyle = FlatStyle.Standard;

        _txtNome = new TextBox { Text = DataStore.Empresa.Nome, Width = 320, Location = new Point(12, 32) };
        _txtCnpj = new TextBox { Text = DataStore.Empresa.Cnpj, Width = 320, Location = new Point(12, 72) };
        _txtTelefone = new TextBox { Text = DataStore.Empresa.Telefone, Width = 320, Location = new Point(12, 112) };

        grp.Controls.AddRange([
            new Label { Text = "Nome da Empresa", Location = new Point(12, 16), AutoSize = true },
            _txtNome,
            new Label { Text = "CNPJ", Location = new Point(12, 56), AutoSize = true },
            _txtCnpj,
            new Label { Text = "Telefone", Location = new Point(12, 96), AutoSize = true },
            _txtTelefone
        ]);

        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
        btnSalvar.Location = new Point(12, 150);
        btnSalvar.Click += (_, _) =>
        {
            DataStore.Empresa.Nome = _txtNome.Text;
            DataStore.Empresa.Cnpj = _txtCnpj.Text;
            DataStore.Empresa.Telefone = _txtTelefone.Text;
            MessageBox.Show("Configurações salvas com sucesso.", "SalgaPro");
        };
        grp.Controls.Add(btnSalvar);

        Controls.Add(grp);
        Controls.Add(titulo);
    }
}
