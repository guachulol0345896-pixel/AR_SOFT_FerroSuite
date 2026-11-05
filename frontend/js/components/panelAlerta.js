// FerroSuite — Componente: panel que lista los productos con riesgo de quiebre de stock.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.panelAlerta = {
  render(contenedor, productos) {
    const { esc, numero, cantidad } = FerroSuite.util;
    if (!productos.length) {
      contenedor.innerHTML = `<div class="panel-alerta panel-ok"><strong>Sin alertas.</strong> Todos los productos están por encima de su stock mínimo.</div>`;
      return;
    }
    const items = productos
      .map((p) =>
        p.stockActual <= 0
          ? `<li><strong>${esc(p.nombre)}</strong> (${esc(p.sku)}): <strong>agotado</strong>, el mínimo es ${numero(p.stockMinimo)}.</li>`
          : `<li><strong>${esc(p.nombre)}</strong> (${esc(p.sku)}): quedan ${esc(cantidad(p.stockActual, p.unidad))}, el mínimo es ${numero(p.stockMinimo)}.</li>`
      )
      .join("");
    contenedor.innerHTML = `
      <div class="panel-alerta" role="status">
        <h3>${productos.length} ${productos.length === 1 ? "producto en riesgo" : "productos en riesgo"} de agotarse</h3>
        <ul>${items}</ul>
        <a class="btn btn-ghost" href="#asistente">Pedir sugerencia al asistente</a>
        <a class="btn btn-ghost" href="ordenes.html">Generar orden de compra</a>
      </div>`;
  },
};
