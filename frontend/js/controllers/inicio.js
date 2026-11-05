// FerroSuite — Controlador de la pantalla de Inicio: resumen tomado de la base de datos.
document.addEventListener("DOMContentLoaded", async () => {
  const zona = document.querySelector("#resumen-inicio");
  if (!zona) return;
  try {
    const r = (await FerroSuite.inventarioService.resumen()).datos;
    FerroSuite.tarjetaResumen.render(zona, [
      { titulo: "Productos", valor: r.productos, detalle: "en el catálogo" },
      { titulo: "En riesgo", valor: r.enRiesgo, detalle: "por agotarse", tono: r.enRiesgo ? "alerta" : "ok" },
      { titulo: "Proveedores", valor: r.proveedores, detalle: "registrados" },
      { titulo: "Órdenes pendientes", valor: r.ordenesPendientes, detalle: "por aprobar", tono: r.ordenesPendientes ? "alerta" : "ok" },
    ]);
  } catch (error) {
    FerroSuite.mostrarErrorPagina(error);
  }
});
