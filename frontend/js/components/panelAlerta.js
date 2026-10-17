// FerroSuite — Componente: panel que lista los productos con riesgo de quiebre de stock.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.panelAlerta = {
  render(contenedor, productos) {
    const esc = FerroSuite.util.esc;
    if (!productos.length) {
      contenedor.innerHTML = `<div class="panel-alerta panel-ok"><strong>Sin alertas.</strong> Todos los productos están por encima de su stock mínimo.</div>`;
      return;
    }
    const items = productos
      .map((p) => `<li><strong>${esc(p.nombre)}</strong> (${esc(p.sku)}): quedan ${p.stockActual} ${esc(p.unidad.toLowerCase())}, el mínimo es ${p.stockMinimo}.</li>`)
      .join("");
    contenedor.innerHTML = `
      <div class="panel-alerta" role="status">
        <h3>${productos.length} ${productos.length === 1 ? "producto en riesgo" : "productos en riesgo"} de agotarse</h3>
        <ul>${items}</ul>
        <a class="btn btn-ghost" href="ordenes.html">Generar orden de compra</a>
      </div>`;
  },
};
