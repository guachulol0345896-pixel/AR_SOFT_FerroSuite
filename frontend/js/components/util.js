// FerroSuite — Utilidades compartidas por los componentes.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.util = {
  // Escapa texto escrito por el usuario antes de ponerlo dentro de HTML.
  esc(valor) {
    return String(valor).replace(/[&<>"']/g, (c) =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])
    );
  },
};
