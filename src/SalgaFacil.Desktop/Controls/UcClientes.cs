using SalgaFacil.Desktop.Forms;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcClientes : UserControl
{
    private DataGridView _grid = null!;
    private readonly ClienteService _service = new();

    public UcClientes()
    {
        BackColor = WinStyles.FundoGeral;
        Dock = DockStyle.Fill;
        BuildUi();
        Carregar();
    }

    private void BuildUi()
    {
        var pnlTopo = new Panel { Dock = DockStyle.Top, Height = 36 };
        var titulo = new Label { Text = "Cadastro de Clientes", Font = WinStyles.FonteTitulo, AutoSize = true, Location = new Point(0, 4) };
        var btnNovo = WinStyles.CriarBotao("Novo Cliente", true);
        btnNovo.Location = new Point(700, 4);
        btnNovo.Click += (_, _) => AbrirModal(null);
        var btnEditar = WinStyles.CriarBotao("Editar");
        btnEditar.Location = new Point(820, 4);
        btnEditar.Click += (_, _) => Editar();
        var btnExcluir = WinStyles.CriarBotao("Excluir");
        btnExcluir.Location = new Point(900, 4);
        btnExcluir.Click += (_, _) => Excluir();
        pnlTopo.Controls.AddRange([titulo, btnNovo, btnEditar, btnExcluir]);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Margin = new Padding(0, 8, 0, 0) };
        pnlGrid.BorderStyle = BorderStyle.FixedSingle;
        _grid = WinStyles.CriarGrid();
        _grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Nome", Name = "Nome" },
            new DataGridViewTextBoxColumn { HeaderText = "Telefone", Name = "Telefone" },
            new DataGridViewTextBoxColumn { HeaderText = "Pedidos", Name = "Pedidos" },
            new DataGridViewTextBoxColumn { HeaderText = "Última Compra", Name = "UltimaCompra" },
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false }
        ]);
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(pnlTopo);
    }

    private void Carregar()
    {
        _grid.Rows.Clear();
        foreach (var c in _service.Listar())
            _grid.Rows.Add(c.Nome, c.Telefone, c.TotalPedidos, c.UltimaCompra.ToString("dd/MM/yyyy"), c.Id);
    }

    private int? IdSel() => _grid.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    private void AbrirModal(Cliente? c)
    {
        using var f = new FrmCadastroCliente(c);
        if (f.ShowDialog(FindForm()) == DialogResult.OK) Carregar();
    }

    private void Editar()
    {
        var id = IdSel();
        if (!id.HasValue) { MessageBox.Show("Selecione um cliente."); return; }
        AbrirModal(_service.Obter(id.Value));
    }

    private void Excluir()
    {
        var id = IdSel();
        if (!id.HasValue) { MessageBox.Show("Selecione um cliente."); return; }
        if (MessageBox.Show("Excluir cliente?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _service.Excluir(id.Value);
            Carregar();
        }
    }
}
