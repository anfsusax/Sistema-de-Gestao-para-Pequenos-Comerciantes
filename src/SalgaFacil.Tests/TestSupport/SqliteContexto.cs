using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Tests.TestSupport;

/// <summary>
/// Banco Sqlite em memória, isolado por instância de teste (cada teste abre sua própria conexão
/// ":memory:" — não compartilha estado com outros testes, ao contrário de um banco de arquivo
/// único). Não é o Postgres real de produção, mas exercita o DbContext e o mapeamento do
/// SalgaFacilDbContext de verdade (FKs, índices, EnsureCreated a partir do modelo do EF), o que é
/// suficiente para os testes de serviço deste projeto (nenhuma feature aqui depende de sintaxe
/// específica do Postgres).
/// </summary>
public sealed class SqliteContexto : IDisposable
{
    private readonly SqliteConnection _connection;
    public SalgaFacilDbContext Db { get; }

    public SqliteContexto()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SalgaFacilDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new SalgaFacilDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
