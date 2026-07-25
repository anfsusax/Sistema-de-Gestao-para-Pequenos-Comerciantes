using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public sealed class ClienteAuthService(SalgaFacilDbContext db)
{
    public ClienteSessao? ClienteAtual { get; private set; }

    public bool EstaAutenticado(int empresaId) => ClienteAtual?.EmpresaId == empresaId;

    public async Task<bool> LoginAsync(int empresaId, string email, string senha)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(emailNormalizado) || string.IsNullOrWhiteSpace(senha))
            return false;

        var senhaHash = DbSeeder.HashSenha(senha);
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.EmpresaId == empresaId && c.Ativo && c.Email != null &&
                c.Email.ToLower() == emailNormalizado && c.SenhaHash == senhaHash);

        if (cliente is null)
            return false;

        ClienteAtual = CriarSessao(cliente);
        return true;
    }

    public async Task<ClienteSessao> CadastrarAsync(
        int empresaId, string nome, string email, string telefone, string senha)
    {
        nome = nome.Trim();
        var emailNormalizado = email.Trim().ToLowerInvariant();
        var telefoneNormalizado = TelefoneNormalizador.Normalizar(telefone);

        if (nome.Length < 2)
            throw new InvalidOperationException("Informe seu nome.");
        if (!emailNormalizado.Contains('@'))
            throw new InvalidOperationException("Informe um e-mail válido.");
        if (telefoneNormalizado is null)
            throw new InvalidOperationException("Informe um telefone válido.");
        if (senha.Length < 6)
            throw new InvalidOperationException("A senha deve ter pelo menos 6 caracteres.");

        var porEmail = await db.Clientes.FirstOrDefaultAsync(c =>
            c.EmpresaId == empresaId && c.Email != null && c.Email.ToLower() == emailNormalizado);

        Cliente cliente;
        if (porEmail is not null)
        {
            if (!string.IsNullOrWhiteSpace(porEmail.SenhaHash))
                throw new InvalidOperationException("Este e-mail já está cadastrado. Entre com sua senha.");
            if (porEmail.TelefoneNormalizado != telefoneNormalizado)
                throw new InvalidOperationException("Este e-mail já pertence a outro cadastro.");

            cliente = porEmail;
            cliente.Nome = nome;
            cliente.Telefone = telefone.Trim();
            cliente.TelefoneNormalizado = telefoneNormalizado;
            cliente.Ativo = true;
            cliente.AtualizadoEm = DateTime.UtcNow;
        }
        else
        {
            cliente = new Cliente
            {
                EmpresaId = empresaId,
                Nome = nome,
                Email = emailNormalizado,
                Telefone = telefone.Trim(),
                TelefoneNormalizado = telefoneNormalizado,
                Ativo = true
            };
            db.Clientes.Add(cliente);
        }

        cliente.Email = emailNormalizado;
        cliente.SenhaHash = DbSeeder.HashSenha(senha);
        await db.SaveChangesAsync();

        ClienteAtual = CriarSessao(cliente);
        return ClienteAtual;
    }

    public void Sair() => ClienteAtual = null;

    private static ClienteSessao CriarSessao(Cliente cliente) =>
        new(cliente.Id, cliente.EmpresaId, cliente.Nome, cliente.Email ?? "",
            cliente.Telefone ?? cliente.WhatsApp ?? "");
}

public sealed record ClienteSessao(
    int Id, int EmpresaId, string Nome, string Email, string Telefone);