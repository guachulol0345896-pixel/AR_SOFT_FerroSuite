// FerroSuite — Servicio de Órdenes de compra (capa de Aplicación del prototipo).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.ordenesService = {
  // RF08: crea una orden en estado "Pendiente de aprobación".
  crearOrden(d) {
    const errores = [];
    const cantidad = Number(d.cantidad);
    if (!d.sku) errores.push("Elige un producto.");
    if (!d.nit) errores.push("Elige un proveedor.");
    if (d.cantidad === "" || isNaN(cantidad) || cantidad <= 0) errores.push("La cantidad debe ser mayor que 0.");
    if (errores.length) return errores;
    const numero = String(modeloOrdenes.length + 1).padStart(3, "0");
    modeloOrdenes.push({ codigo: "OC-" + numero, sku: d.sku, nit: d.nit, cantidad, estado: "Pendiente de aprobación" });
    return [];
  },

  // RF09: aprueba una orden pendiente.
  aprobar(codigo) {
    const orden = modeloOrdenes.find((o) => o.codigo === codigo);
    if (orden) orden.estado = "Aprobada";
  },
};
