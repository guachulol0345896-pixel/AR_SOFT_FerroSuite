// FerroSuite — Utilidades compartidas por los componentes.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.util = {
  // Escapa texto antes de ponerlo dentro de HTML.
  esc(valor) {
    return String(valor ?? "").replace(/[&<>"']/g, (c) =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])
    );
  },

  // 1234.5 -> "1.234,5"
  numero(valor) {
    return Number(valor).toLocaleString("es-CO");
  },

  // 1234567 -> "$ 1.234.567"
  pesos(valor) {
    if (valor === null || valor === undefined) return "—";
    return "$ " + Math.round(valor).toLocaleString("es-CO");
  },

  // (46, "Unidad") -> "46 unidades"
  cantidad(valor, unidad) {
    let u = unidad.toLowerCase();
    if (valor !== 1) u = u === "galón" ? "galones" : u === "unidad" ? "unidades" : u + "s";
    return FerroSuite.util.numero(valor) + " " + u;
  },

  fecha(iso) {
    return new Date(iso).toLocaleString("es-CO", { dateStyle: "short", timeStyle: "short" });
  },
};
