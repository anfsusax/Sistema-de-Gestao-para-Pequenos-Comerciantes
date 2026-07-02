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
        Size = new Size(480, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        // BUG CORRIGIDO: "corpo" estava com Dock=Fill + AutoSize=true ao mesmo tempo — combinação
        // contraditória em WinForms que cortava o conteúdo que não coubesse (mesma causa do bug
        // encontrado em FrmLogin.cs: botão "Salvar"/"Cancelar" ficavam fora da área visível).
        // Dock=Top fixa a Largura (igual à janela) e deixa a Altura livre pro AutoSize.
        var corpo = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(16) };

        _txtNome = new TextBox { Width = 430, Text = _cliente.Nome };
        WinStyles.AdicionarCampo(corpo, "Nome", _txtNome);

        _txtTelefone = new TextBox { Width = 430, Text = _cliente.Telefone };
        WinStyles.AdicionarCampo(corpo, "Telefone", _txtTelefone);

        _txtEndereco = new TextBox { Width = 430, Text = _cliente.Endereco };
        WinStyles.AdicionarCampo(corpo, "Endereço", _txtEndereco);

        _txtObs = new TextBox { Width = 430, Height = 60, Multiline = true, Text = _cliente.Observacao };
        WinStyles.AdicionarCampo(corpo, "Observação", _txtObs);

        var pnlBotoes = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 16, 0, 0) };
        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
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
        btnCancelar.Margin = new Padding(8, 0, 0, 0);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBotoes.Controls.AddRange([btnSalvar, btnCancelar]);
        corpo.Controls.Add(pnlBotoes);

        Controls.Add(corpo);
    }
}
