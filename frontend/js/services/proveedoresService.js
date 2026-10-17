// FerroSuite — Servicio de Proveedores (capa de Aplicación del prototipo).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.proveedoresService = {
  validarProveedor(d) {
    const errores = [];
    if (!d.nombre) errores.push("El nombre del proveedor es obligatorio.");
    if (!d.nit) errores.push("El NIT es obligatorio.");
    if (d.nit && modeloProveedores.some((p) => p.nit === d.nit)) errores.push("Ya existe un proveedor con ese NIT.");
    const dias = Number(d.diasEntrega);
    if (d.diasEntrega === "" || isNaN(dias) || dias < 0) errores.push("Los días de entrega deben ser un número igual o mayor que 0.");
    return errores;
  },

  agregarProveedor(d) {
    const errores = this.validarProveedor(d);
    if (errores.length) return errores;
    modeloProveedores.push({
      nombre: d.nombre, nit: d.nit, telefono: d.telefono || "—",
      diasEntrega: Number(d.diasEntrega), cumplimiento: "—",
    });
    return [];
  },

  masRapido() {
    return modeloProveedores.reduce((a, b) => (b.diasEntrega < a.diasEntrega ? b : a));
  },

  buscarPorNit(nit) {
    return modeloProveedores.find((p) => p.nit === nit);
  },
};
