// FerroSuite — Servicio de Órdenes de compra del frontend.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.ordenesService = {
  listar() {
    return FerroSuite.api.pedir("ordenes.listar");
  },

  validar(d) {
    const errores = [];
    if (!d.sku) errores.push("Elige un producto.");
    if (!d.nitProveedor) errores.push("Elige un proveedor.");
    if (d.cantidad === "" || isNaN(Number(d.cantidad)) || Number(d.cantidad) <= 0) errores.push("La cantidad debe ser un número mayor que 0.");
    return errores;
  },

  // RF08: la orden queda pendiente de aprobación.
  crear(d) {
    return FerroSuite.api.pedir("ordenes.crear", { sku: d.sku, nitProveedor: d.nitProveedor, cantidad: Number(d.cantidad) });
  },

  // RF09: desde que se aprueba corre el plazo de pago (15 días).
  aprobar(codigo) {
    return FerroSuite.api.pedir("ordenes.aprobar", { codigo });
  },

  pagar(codigo) {
    return FerroSuite.api.pedir("ordenes.pagar", { codigo });
  },
};
