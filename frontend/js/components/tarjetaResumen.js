// FerroSuite — Componente: tarjeta con un dato clave (número grande + descripción).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.tarjetaResumen = {
  html({ titulo, valor, detalle, tono }) {
    const esc = FerroSuite.util.esc;
    return `
    <article class="tarjeta-resumen ${tono ? "tono-" + tono : ""}">
      <span class="tarjeta-titulo">${esc(titulo)}</span>
      <strong class="tarjeta-valor">${esc(valor)}</strong>
      <span class="tarjeta-detalle">${esc(detalle || "")}</span>
    </article>`;
  },

  // Dibuja varias tarjetas dentro de un contenedor.
  render(contenedor, tarjetas) {
    contenedor.innerHTML = tarjetas.map((t) => this.html(t)).join("");
  },
};
