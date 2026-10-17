// FerroSuite — Componente: etiqueta de estado (por ejemplo "Stock bajo").
// Tonos: alerta (naranja), neutro (gris), ok (verde).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.etiqueta = {
  html(texto, tono) {
    return `<span class="etiqueta etiqueta-${tono || "neutro"}">${FerroSuite.util.esc(texto)}</span>`;
  },
};
