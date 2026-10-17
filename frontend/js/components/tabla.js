// FerroSuite — Componente: tabla de datos reutilizable.
// columnas: [{ titulo, valor: (fila) => "texto o html" }]
// Cada módulo (inventario, proveedores, órdenes) usa este mismo componente.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.tabla = {
  render(contenedor, columnas, filas, textoVacio) {
    const encabezado = columnas.map((c) => `<th>${c.titulo}</th>`).join("");
    const cuerpo = filas.length
      ? filas.map((f) => `<tr>${columnas.map((c) => `<td>${c.valor(f)}</td>`).join("")}</tr>`).join("")
      : `<tr><td class="tabla-vacia" colspan="${columnas.length}">${textoVacio || "Sin registros."}</td></tr>`;
    contenedor.innerHTML = `
    <div class="table-wrap">
      <table class="data-table">
        <thead><tr>${encabezado}</tr></thead>
        <tbody>${cuerpo}</tbody>
      </table>
    </div>`;
  },
};
