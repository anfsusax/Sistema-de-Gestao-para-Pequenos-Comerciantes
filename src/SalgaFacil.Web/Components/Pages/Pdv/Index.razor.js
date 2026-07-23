// Atalhos de teclado do PDV (F2/F5/F6). São teclas de função, sem ação de digitação de texto
// associada, então interceptar globalmente aqui não quebra a digitação normal em nenhum campo
// da tela (diferente de teclas alfanuméricas, onde preventDefault bloquearia a entrada de texto).
export function registrarAtalhos(dotNetRef) {
    function handler(e) {
        if (e.key === 'F2' || e.key === 'F5' || e.key === 'F6') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OnAtalho', e.key);
        }
    }
    window.addEventListener('keydown', handler);
    return {
        dispose: () => window.removeEventListener('keydown', handler)
    };
}

export function imprimir() {
    window.print();
}
