// FerroSuite — Servicio de Inventario (capa de Aplicación del prototipo).
// Aquí viven las reglas del negocio, separadas de cómo se ve la pantalla.
// Regla de riesgo: un producto está en riesgo si su cantidad es menor o igual
// al mínimo definido (la misma regla del prototipo en C#).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.inventarioService = {
  productoEnRiesgo(p) {
    return p.stockActual <= p.stockMinimo;
  },

  buscarProductosEnRiesgo() {
    return modeloProductos.filter((p) => this.productoEnRiesgo(p));
  },

  buscarPorTexto(texto) {
    const t = texto.trim().toLowerCase();
    if (!t) return modeloProductos.slice();
    return modeloProductos.filter((p) => p.sku.toLowerCase().includes(t) || p.nombre.toLowerCase().includes(t));
  },

  // RF12: devuelve la lista de errores de un producto antes de guardarlo.
  validarProducto(d) {
    const errores = [];
    if (!d.nombre) errores.push("El nombre del producto es obligatorio.");
    if (!d.sku) errores.push("El SKU es obligatorio.");
    if (d.sku && modeloProductos.some((p) => p.sku.toLowerCase() === d.sku.toLowerCase()))
      errores.push("Ya existe un producto con ese SKU.");
    const actual = Number(d.stockActual), minimo = Number(d.stockMinimo);
    if (d.stockActual === "" || isNaN(actual) || actual < 0) errores.push("El stock actual debe ser un número igual o mayor que 0.");
    if (d.stockMinimo === "" || isNaN(minimo) || minimo < 0) errores.push("El stock mínimo debe ser un número igual o mayor que 0.");
    return errores;
  },

  agregarProducto(d) {
    const errores = this.validarProducto(d);
    if (errores.length) return errores;
    modeloProductos.push({
      sku: d.sku, nombre: d.nombre, unidad: d.unidad,
      stockActual: Number(d.stockActual), stockMinimo: Number(d.stockMinimo),
    });
    return [];
  },

  // RF05: registra una entrada o una salida y actualiza el stock del producto.
  registrarMovimiento(d) {
    const errores = [];
    const producto = modeloProductos.find((p) => p.sku === d.sku);
    const cantidad = Number(d.cantidad);
    if (!producto) errores.push("Elige un producto.");
    if (d.cantidad === "" || isNaN(cantidad) || cantidad <= 0) errores.push("La cantidad debe ser mayor que 0.");
    if (producto && d.tipo === "Salida" && cantidad > producto.stockActual)
      errores.push("No hay stock suficiente para esa salida (disponible: " + producto.stockActual + ").");
    if (errores.length) return errores;

    producto.stockActual += d.tipo === "Entrada" ? cantidad : -cantidad;
    modeloMovimientos.unshift({
      fecha: new Date().toLocaleString("es-CO", { dateStyle: "short", timeStyle: "short" }),
      sku: producto.sku, nombre: producto.nombre, tipo: d.tipo, cantidad,
    });
    return [];
  },
};
