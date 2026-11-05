// FerroSuite — Controlador de Proveedores.
document.addEventListener("DOMContentLoaded", () => {
  const servicio = FerroSuite.proveedoresService;
  const esc = FerroSuite.util.esc;
  const zonaResumen = document.querySelector("#resumen-proveedores");
  const zonaTabla = document.querySelector("#tabla-proveedores");

  const columnas = [
    { titulo: "Nombre", valor: (p) => esc(p.nombre) },
    { titulo: "NIT", valor: (p) => esc(p.nit) },
    { titulo: "Teléfono", valor: (p) => esc(p.telefono || "—") },
    { titulo: "Días de entrega", valor: (p) => p.diasEntrega },
    { titulo: "Cumplimiento", valor: (p) => (p.cumplimiento == null ? "—" : p.cumplimiento + " %") },
  ];

  async function cargar() {
    try {
      const proveedores = (await servicio.listar()).datos;
      FerroSuite.ocultarErrorPagina();
      let rapido = null;
      for (const p of proveedores) if (!rapido || p.diasEntrega < rapido.diasEntrega) rapido = p;
      FerroSuite.tarjetaResumen.render(zonaResumen, [
        { titulo: "Proveedores", valor: proveedores.length, detalle: "registrados en la base de datos" },
        { titulo: "Entrega más rápida", valor: rapido ? rapido.diasEntrega + " días" : "—", detalle: rapido ? rapido.nombre : "", tono: "ok" },
      ]);
      FerroSuite.tabla.render(zonaTabla, columnas, proveedores, "Todavía no hay proveedores registrados.");
    } catch (error) {
      FerroSuite.mostrarErrorPagina(error);
    }
  }

  FerroSuite.formulario.render(document.querySelector("#form-proveedor"), {
    textoBoton: "Agregar proveedor",
    campos: [
      { nombre: "nombre", etiqueta: "Nombre", requerido: true },
      { nombre: "nit", etiqueta: "NIT", placeholder: "900123456-7", requerido: true },
      { nombre: "telefono", etiqueta: "Teléfono" },
      { nombre: "diasEntrega", etiqueta: "Días de entrega", tipo: "number", requerido: true },
    ],
    validar: (d) => servicio.validar(d),
    async alEnviar(datos) {
      const r = await servicio.crear(datos);
      await cargar();
      return r.mensaje;
    },
  });

  cargar();
});
