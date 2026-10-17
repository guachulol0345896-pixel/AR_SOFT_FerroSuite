// FerroSuite — Controlador de la pantalla de Inicio: resumen del estado del prototipo.
document.addEventListener("DOMContentLoaded", () => {
  const zona = document.querySelector("#resumen-inicio");
  if (!zona) return;
  const enRiesgo = FerroSuite.inventarioService.buscarProductosEnRiesgo().length;
  const pendientes = modeloOrdenes.filter((o) => o.estado !== "Aprobada").length;
  FerroSuite.tarjetaResumen.render(zona, [
    { titulo: "Productos", valor: modeloProductos.length, detalle: "en el catálogo de prueba" },
    { titulo: "En riesgo", valor: enRiesgo, detalle: "por agotarse", tono: enRiesgo ? "alerta" : "ok" },
    { titulo: "Proveedores", valor: modeloProveedores.length, detalle: "registrados" },
    { titulo: "Órdenes pendientes", valor: pendientes, detalle: "por aprobar", tono: pendientes ? "alerta" : "ok" },
  ]);
});
