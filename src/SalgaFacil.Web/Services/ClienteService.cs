using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class ClienteService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    public Task<List<Cliente>> ListarAsync(bool? apenasAtivos = null) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => !apenasAtivos.HasValue || c.Ativo == apenasAtivos.Value)
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public Task<List<ClienteComResumo>> ListarComResumoAsync(string? busca = null) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => string.IsNullOrWhiteSpace(busca)
                || c.Nome.Contains(busca)
                || c.Telefone.Contains(busca)
                || (c.WhatsApp != null && c.WhatsApp.Contains(busca))
                || (c.Cpf != null && c.Cpf.Contains(busca)))
            .Select(c => new ClienteComResumo
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                WhatsApp = c.WhatsApp,
                Email = c.Email,
                Ativo = c.Ativo,
                Cidade = c.Enderecos.Where(e => e.Principal).Select(e => e.Cidade).FirstOrDefault()
                    ?? c.Enderecos.Select(e => e.Cidade).FirstOrDefault(),
                TotalPedidos = c.Pedidos.Count,
                UltimaCompra = c.Pedidos.OrderByDescending(p => p.Data).Select(p => (DateTime?)p.Data).FirstOrDefault()
            })
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public Task<Cliente?> ObterAsync(int id) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);

    public async Task SalvarAsync(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            throw new InvalidOperationException("Nome do cliente é obrigatório.");

        cliente.Nome = cliente.Nome.Trim();
        if (!string.IsNullOrWhiteSpace(cliente.Email) && !cliente.Email.Contains('@'))
            throw new InvalidOperationException("E-mail inválido.");

        var principais = cliente.Enderecos.Count(e => e.Principal);
        if (principais > 1)
            throw new InvalidOperationException("Apenas um endereço pode ser o principal.");
        if (cliente.Enderecos.Count > 0 && principais == 0)
            cliente.Enderecos.First().Principal = true;

        // Fonte única de normalização — mantém TelefoneNormalizado sempre coerente com
        // Telefone/WhatsApp, independente de onde o cliente foi criado ou editado
        // (cardápio público ou cadastro administrativo).
        cliente.TelefoneNormalizado = TelefoneNormalizador.Normalizar(cliente.Telefone) ?? TelefoneNormalizador.Normalizar(cliente.WhatsApp);

        if (cliente.Id == 0)
        {
            cliente.EmpresaId = EmpresaId;
            cliente.CriadoEm = DateTime.UtcNow;
            db.Clientes.Add(cliente);
        }
        else
        {
            var existente = await db.Clientes
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id && c.EmpresaId == EmpresaId)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            existente.Nome = cliente.Nome;
            existente.Cpf = cliente.Cpf;
            existente.Cnpj = cliente.Cnpj;
            existente.Telefone = cliente.Telefone;
            existente.WhatsApp = cliente.WhatsApp;
            existente.TelefoneNormalizado = cliente.TelefoneNormalizado;
            existente.Email = cliente.Email;
            existente.DataNascimento = cliente.DataNascimento;
            existente.Observacoes = cliente.Observacoes;
            existente.Ativo = cliente.Ativo;
            existente.AtualizadoEm = DateTime.UtcNow;

            db.EnderecosCliente.RemoveRange(existente.Enderecos);
            foreach (var end in cliente.Enderecos)
            {
                end.Id = 0;
                end.ClienteId = existente.Id;
                existente.Enderecos.Add(end);
            }

            await db.SaveChangesAsync();
            return;
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Busca um cliente existente pelo telefone normalizado (dentro da empresa) ou cria um novo.
    /// Ponto único de "achar ou criar cliente" usado pelo cadastro via cardápio público — evita
    /// duplicidade de cliente com o mesmo telefone (ver _ia/DECISOES.md 2026-07-24).
    /// Recebe <paramref name="empresaId"/> explicitamente (em vez de usar <see cref="EmpresaId"/>)
    /// porque o cardápio público é anônimo, sem <see cref="IEmpresaContext"/> autenticado.
    /// Não sobrescreve o nome de um cliente já existente — só usa o cadastro como está.
    /// </summary>
    public async Task<(Cliente Cliente, bool JaExistia)> ObterOuCriarPorTelefoneAsync(int empresaId, string nome, string? telefone, string? whatsapp, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new InvalidOperationException("Informe seu nome.");

        var telefoneBruto = !string.IsNullOrWhiteSpace(telefone) ? telefone : whatsapp;
        var normalizado = TelefoneNormalizador.Normalizar(telefoneBruto);
        if (normalizado is null)
            throw new InvalidOperationException("Informe um telefone válido.");

        var existente = await BuscarPorTelefoneNormalizadoAsync(empresaId, normalizado);
        if (existente is not null)
        {
            // Mesma regra de não sobrescrever cadastro existente já aplicada ao nome: só
            // preenche o e-mail se o cliente ainda não tinha um informado. Nunca substitui
            // um e-mail já cadastrado por um novo digitado num pedido seguinte.
            if (string.IsNullOrWhiteSpace(existente.Email) && !string.IsNullOrWhiteSpace(email))
            {
                existente.Email = email.Trim();
                existente.AtualizadoEm = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return (existente, true);
        }

        var cliente = new Cliente
        {
            EmpresaId = empresaId,
            Nome = nome.Trim(),
            Telefone = telefoneBruto!.Trim(),
            WhatsApp = whatsapp?.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            TelefoneNormalizado = normalizado,
            Observacoes = "Pedido via cardápio público",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        db.Clientes.Add(cliente);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Janela de concorrência: outra requisição pode ter criado o mesmo telefone entre a
            // consulta acima e este SaveChanges. Só é impossível de fato quando existir a
            // restrição UNIQUE (EmpresaId, TelefoneNormalizado) no banco — ver RISCOS.md
            // ("Restrição UNIQUE de telefone pendente de checagem de duplicados"). Com a
            // restrição em vigor, o INSERT falha aqui e recuperamos o registro do concorrente
            // em vez de propagar o erro para o usuário.
            db.Entry(cliente).State = EntityState.Detached;
            var criadoPelaOutraRequisicao = await db.Clientes
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.TelefoneNormalizado == normalizado);
            if (criadoPelaOutraRequisicao is not null)
                return (criadoPelaOutraRequisicao, true);
            throw;
        }

        return (cliente, false);
    }

    /// <summary>
    /// Busca um cliente pelo telefone (aceita qualquer formatação — normaliza internamente).
    /// Somente leitura em relação ao pedido; nunca cria cliente. Usado por qualquer fluxo
    /// público que precise identificar o cliente pelo telefone sem criar pedido (ex.:
    /// "meus pedidos" no cardápio). Reaproveita o mesmo fallback de auto-cura de
    /// <see cref="ObterOuCriarPorTelefoneAsync"/> — ver ali o porquê.
    /// </summary>
    public async Task<Cliente?> BuscarPorTelefoneAsync(int empresaId, string? telefone)
    {
        var normalizado = TelefoneNormalizador.Normalizar(telefone);
        return normalizado is null ? null : await BuscarPorTelefoneNormalizadoAsync(empresaId, normalizado);
    }

    /// <summary>
    /// Ponto único de "achar cliente pelo telefone" — usado tanto por
    /// <see cref="ObterOuCriarPorTelefoneAsync"/> quanto por <see cref="BuscarPorTelefoneAsync"/>.
    /// Primeiro tenta pelo índice (TelefoneNormalizado). Se não achar, cai no fallback de
    /// auto-cura: clientes criados antes desta funcionalidade (ou antes da migration ser
    /// aplicada) ainda têm TelefoneNormalizado nulo, então o filtro por índice não os
    /// encontra — sem este fallback, cada novo pedido desses clientes antigos criaria um
    /// cliente duplicado, mesmo com a checagem em vigor. EF Core não traduz
    /// TelefoneNormalizador.Normalizar para SQL, então os candidatos (só os com
    /// TelefoneNormalizado nulo — conjunto que só diminui com o tempo) são carregados e
    /// comparados em memória. Ao achar, persiste o valor normalizado no registro legado,
    /// então esse custo só existe uma vez por cliente.
    /// </summary>
    private async Task<Cliente?> BuscarPorTelefoneNormalizadoAsync(int empresaId, string normalizado)
    {
        var existente = await db.Clientes
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.TelefoneNormalizado == normalizado);
        if (existente is not null)
            return existente;

        var candidatosLegados = await db.Clientes
            .Where(c => c.EmpresaId == empresaId && c.TelefoneNormalizado == null)
            .ToListAsync();
        var existenteLegado = candidatosLegados.FirstOrDefault(c =>
            (TelefoneNormalizador.Normalizar(c.Telefone) ?? TelefoneNormalizador.Normalizar(c.WhatsApp)) == normalizado);
        if (existenteLegado is null)
            return null;

        existenteLegado.TelefoneNormalizado = normalizado;
        existenteLegado.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return existenteLegado;
    }

    public async Task AlternarAtivoAsync(int id)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");
        cliente.Ativo = !cliente.Ativo;
        cliente.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);
        if (cliente is null) return;

        var temPedido = await db.Pedidos.AnyAsync(p => p.ClienteId == id && p.EmpresaId == EmpresaId);
        if (temPedido)
            throw new InvalidOperationException("Não é possível excluir: cliente possui pedidos. Inative-o.");

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync();
    }
}

public class ClienteComResumo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Cidade { get; set; }
    public bool Ativo { get; set; }
    public int TotalPedidos { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
