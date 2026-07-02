using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmCadastroProduto : Form
{
    private readonly Produto _produto;
    private readonly ProdutoService _service = new();
    private TextBox _txtNome = null!;
    private ComboBox _cmbCategoria = null!;
    private RadioButton _rbFrito = null!;
    private RadioButton _rbAssado = null!;
    private TextBox _txtPreco = null!;
    private TextBox _txtCusto = null!;
    private TextBox _txtDescricao = null!;
    private CheckBox _chkAtivo = null!;

    public FrmCadastroProduto(Produto? existente)
    {
        _produto = existente != null ? new Produto
        {
            Id = existente.Id, Nome = existente.Nome, Categoria = existente.Categoria,
            Tipo = existente.Tipo, PrecoVenda = existente.PrecoVenda, CustoEstimado = existente.CustoEstimado,
            Descricao = existente.Descricao, Status = existente.Status
        } : new Produto();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Cadastro de Produto";
        Size = new Size(500, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        // BUG CORRIGIDO: "corpo" estava com Dock=Fill + AutoSize=true ao mesmo tempo — combinação
        // contraditória em WinForms que cortava o conteúdo que não coubesse (botões "Salvar"/
        // "Cancelar" ficavam fora da área visível). Dock=Top fixa a Largura e deixa a Altura
        // livre pro AutoSize calcular a partir do conteúdo real.
        var corpo = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(16) };

        _txtNome = new TextBox { Width = 450, Text = _produto.Nome };
        WinStyles.AdicionarCampo(corpo, "Nome", _txtNome);

        _cmbCategoria = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbCategoria.Items.AddRange(["Salgado", "Doce", "Bebida"]);
        _cmbCategoria.SelectedItem = _produto.Categoria;
        if (_cmbCategoria.SelectedIndex < 0) _cmbCategoria.SelectedIndex = 0;

        var pnlTipo = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0) };
        _rbFrito = new RadioButton { Text = "Frito", AutoSize = true, Checked = _produto.Tipo == TipoProduto.Frito };
        _rbAssado = new RadioButton { Text = "Assado", AutoSize = true, Margin = new Padding(12, 0, 0, 0), Checked = _produto.Tipo == TipoProduto.Assado };
        pnlTipo.Controls.AddRange([_rbFrito, _rbAssado]);

        corpo.Controls.Add(WinStyles.CriarLinhaDupla("Categoria", _cmbCategoria, "Tipo", pnlTipo));

        _txtPreco = new TextBox { Width = 200, Text = _produto.PrecoVenda > 0 ? _produto.PrecoVenda.ToString("F2") : "" };
        _txtCusto = new TextBox { Width = 200, Text = _produto.CustoEstimado > 0 ? _produto.CustoEstimado.ToString("F2") : "" };
        corpo.Controls.Add(WinStyles.CriarLinhaDupla("Preço Venda", _txtPreco, "Custo Estimado", _txtCusto));

        _txtDescricao = new TextBox { Width = 450, Height = 60, Multiline = true, Text = _produto.Descricao };
        WinStyles.AdicionarCampo(corpo, "Descrição", _txtDescricao);

        _chkAtivo = new CheckBox { Text = "Status Ativo", Checked = _produto.Status == StatusProduto.Ativo, AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        corpo.Controls.Add(_chkAtivo);

        var pnlBotoes = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 16, 0, 0) };
        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
        btnSalvar.Click += BtnSalvar_Click;
        var btnCancelar = WinStyles.CriarBotao("Cancelar");
        btnCancelar.Margin = new Padding(8, 0, 0, 0);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBotoes.Controls.AddRange([btnSalvar, btnCancelar]);
        corpo.Controls.Add(pnlBotoes);

        Controls.Add(corpo);
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNome.Text)) { MessageBox.Show("Nome é obrigatório."); return; }
        if (_cmbCategoria.SelectedItem == null) { MessageBox.Show("Categoria é obrigatória."); return; }
        if (!decimal.TryParse(_txtPreco.Text, out var preco)) { MessageBox.Show("Preço de venda inválido."); return; }

        _produto.Nome = _txtNome.Text.Trim();
        _produto.Categoria = _cmbCategoria.SelectedItem!.ToString()!;
        _produto.Tipo = _rbFrito.Checked ? TipoProduto.Frito : TipoProduto.Assado;
        _produto.PrecoVenda = preco;
        decimal.TryParse(_txtCusto.Text, out var custo);
        _produto.CustoEstimado = custo;
        _produto.Descricao = _txtDescricao.Text;
        _produto.Status = _chkAtivo.Checked ? StatusProduto.Ativo : StatusProduto.Inativo;

        _service.Salvar(_produto);
        DialogResult = DialogResult.OK;
        Close();
    }
}
