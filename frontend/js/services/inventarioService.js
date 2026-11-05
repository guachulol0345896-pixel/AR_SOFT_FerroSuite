// FerroSuite — Servicio de Inventario del frontend.
// Hace una validación básica antes de enviar (para avisar rápido al usuario) y
// pide al backend las operaciones. El backend vuelve a validar todo.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.inventarioService = {
  listar() {
    return FerroSuite.api.pedir("productos.listar");
  },

  listarMovimientos(limite) {
    return FerroSuite.api.pedir("movimientos.listar", { limite });
  },

  resumen() {
    return FerroSuite.api.pedir("resumen");
  },

  // Validación básica en pantalla: campos obligatorios y números.
  validarProducto(d) {
    const errores = [];
    if (!d.sku) errores.push("El SKU es obligatorio.");
    if (!d.nombre) errores.push("El nombre del producto es obligatorio.");
    if (d.stockActual === "" || isNaN(Number(d.stockActual))) errores.push("El stock actual debe ser un número.");
    if (d.stockMinimo === "" || isNaN(Number(d.stockMinimo))) errores.push("El stock mínimo debe ser un número.");
    if (d.precioCompra !== "" && isNaN(Number(d.precioCompra))) errores.push("El precio de compra debe ser un número.");
    return errores;
  },

  // Convierte lo que se escribió en el formulario al formato que espera el backend.
  paraEnviar(d) {
    return {
      sku: d.sku, nombre: d.nombre, categoria: d.categoria, unidad: d.unidad,
      stockActual: Number(d.stockActual), stockMinimo: Number(d.stockMinimo),
      precioCompra: d.precioCompra === "" ? null : Number(d.precioCompra),
      nitProveedor: d.nitProveedor || null,
    };
  },

  crear(d) {
    return FerroSuite.api.pedir("productos.crear", this.paraEnviar(d));
  },

  actualizar(d) {
    return FerroSuite.api.pedir("productos.actualizar", this.paraEnviar(d));
  },

  eliminar(sku) {
    return FerroSuite.api.pedir("productos.eliminar", { sku });
  },

  validarMovimiento(d) {
    const errores = [];
    if (!d.sku) errores.push("Elige un producto.");
    if (d.cantidad === "" || isNaN(Number(d.cantidad)) || Number(d.cantidad) <= 0) errores.push("La cantidad debe ser un número mayor que 0.");
    return errores;
  },

  registrarMovimiento(d) {
    let usuario = "Sin sesión";
    try {
      usuario = sessionStorage.getItem("ferrosuite-usuario") || usuario;
    } catch (e) {}
    return FerroSuite.api.pedir("movimientos.registrar", {
      sku: d.sku, tipo: d.tipo, cantidad: Number(d.cantidad), motivo: d.motivo, usuario,
    });
  },

  restablecerDatos() {
    return FerroSuite.api.pedir("datos.restablecer");
  },
};
