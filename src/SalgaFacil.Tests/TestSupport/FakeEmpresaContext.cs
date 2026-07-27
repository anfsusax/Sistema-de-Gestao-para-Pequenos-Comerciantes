using SalgaFacil.Web.Services;

namespace SalgaFacil.Tests.TestSupport;

/// <summary>Stub de IEmpresaContext — simula "usuário administrativo autenticado da empresa X" sem precisar de AuthService/circuito Blazor.</summary>
public sealed class FakeEmpresaContext(int? empresaId) : IEmpresaContext
{
    public int? EmpresaId { get; } = empresaId;

    public int RequireEmpresaId() =>
        EmpresaId ?? throw new InvalidOperationException("Faça login para continuar.");
}
