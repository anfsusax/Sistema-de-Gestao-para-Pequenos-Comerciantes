using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmCadastroCliente : Form
{
    private readonly Cliente _cliente;
    private readonly ClienteService _service = new();
    private TextBox _txtNome = null!;
    private TextBox _txtTelefone = null!;
    private TextBox _txtEndereco = null!;
    private TextBox _txtObs = null!;

    public FrmCadastroCliente(Cliente? existente)
    {
        _cliente = existente != null ? new Cliente
        {
            Id = existente.Id, Nome = existente.Nome, Telefone = existente.Telefone,
            Endereco = existente.Endereco, Observacao = existente.Observacao,
            TotalPedidos = existente.TotalPedidos, UltimaCompra = existente.UltimaCompra
        } : new Cliente();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Cadastro de Cliente";
        Size = new Size(480, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 245, 245);
        Font = WinStyles.FontePadrao;

        int y = 16;
        Controls.Add(Lbl("Nome", 16, y)); y += 18;
        _txtNome = new TextBox { Location = new Point(16, y), Width = 430, Text = _cliente.Nome }; Controls.Add(_txtNome); y += 32;
        Controls.Add(Lbl("Telefone", 16, y)); y += 18;
        _txtTelefone = new TextBox { Location = new Point(16, y), Width = 430, Text = _cliente.Telefone }; Controls.Add(_txtTelefone); y += 32;
        Controls.Add(Lbl("Endereço", 16, y)); y += 18;
        _txtEndereco = new TextBox { Location = new Point(16, y), Width = 430, Text = _cliente.Endereco }; Controls.Add(_txtEndereco); y += 32;
        Controls.Add(Lbl("Observação", 16, y)); y += 18;
        _txtObs = new TextBox { Location = new Point(16, y), Width = 430, Height = 60, Multiline = true, Text = _cliente.Observacao }; Controls.Add(_txtObs); y += 72;

        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
        btnSalvar.Location = new Point(260, y);
        btnSalvar.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtNome.Text)) { MessageBox.Show("Nome é obrigatório."); return; }
            if (string.IsNullOrWhiteSpace(_txtTelefone.Text)) { MessageBox.Show("Telefone é obrigatório."); return; }
            _cliente.Nome = _txtNome.Text.Trim();
            _cliente.Telefone = _txtTelefone.Text.Trim();
            _cliente.Endereco = _txtEndereco.Text;
            _cliente.Observacao = _txtObs.Text;
            _service.Salvar(_cliente);
            DialogResult = DialogResult.OK;
            Close();
        };
        var btnCancelar = WinStyles.CriarBotao("Cancelar");
        btnCancelar.Location = new Point(350, y);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([btnSalvar, btnCancelar]);
    }

    private static Label Lbl(string t, int x, int y) => new() { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8F) };
}
