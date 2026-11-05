// FerroSuite — Servicio de Proveedores del frontend.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.proveedoresService = {
  listar() {
    return FerroSuite.api.pedir("proveedores.listar");
  },

  validar(d) {
    const errores = [];
    if (!d.nombre) errores.push("El nombre del proveedor es obligatorio.");
    if (!d.nit) errores.push("El NIT es obligatorio.");
    if (d.diasEntrega === "" || !Number.isInteger(Number(d.diasEntrega))) errores.push("Los días de entrega deben ser un número entero.");
    return errores;
  },

  crear(d) {
    return FerroSuite.api.pedir("proveedores.crear", {
      nombre: d.nombre, nit: d.nit, telefono: d.telefono || null, diasEntrega: Number(d.diasEntrega),
    });
  },
};
