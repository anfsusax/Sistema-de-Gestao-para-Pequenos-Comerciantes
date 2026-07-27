// PIX-MANUAL-002: abre/baixa o comprovante de pagamento Pix no navegador do usuário
// administrativo. Os bytes chegam do servidor via IJSRuntime (base64) — não há rota HTTP
// pública para o arquivo (ele fica fora de wwwroot, ver ComprovanteArmazenamentoService), por
// isso o "download" é montado inteiramente no cliente a partir de um Blob em memória.
window.salgaFacilAbrirComprovante = (base64, contentType, nomeArquivo) => {
    const byteChars = atob(base64);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = nomeArquivo || "comprovante";
    a.target = "_blank";
    a.rel = "noopener";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    setTimeout(() => URL.revokeObjectURL(url), 60000);
};
