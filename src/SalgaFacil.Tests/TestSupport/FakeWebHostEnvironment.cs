using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace SalgaFacil.Tests.TestSupport;

/// <summary>
/// Stub mínimo de IWebHostEnvironment só para satisfazer ComprovanteArmazenamentoService, que só
/// lê ContentRootPath. Os FileProviders nunca são usados por esse serviço — deixados null! de
/// propósito (não são acessados, e IWebHostEnvironment não expõe nada que force inicializá-los).
/// </summary>
public sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "SalgaFacil.Tests";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = "";
    public string EnvironmentName { get; set; } = "Development";
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string WebRootPath { get; set; } = "";
}
