// FerroSuite — Controlador de Proveedores.
document.addEventListener("DOMContentLoaded", () => {
  const servicio = FerroSuite.proveedoresService;
  const esc = FerroSuite.util.esc;
  const zonaResumen = document.querySelector("#resumen-proveedores");
  const zonaTabla = document.querySelector("#tabla-proveedores");

  const columnas = [
    { titulo: "Nombre", valor: (p) => esc(p.nombre) },
    { titulo: "NIT", valor: (p) => esc(p.nit) },
    { titulo: "Teléfono", valor: (p) => esc(p.telefono) },
    { titulo: "Días de entrega", valor: (p) => p.diasEntrega },
    { titulo: "Cumplimiento", valor: (p) => esc(p.cumplimiento) },
  ];

  function pintarTodo() {
    const rapido = servicio.masRapido();
    FerroSuite.tarjetaResumen.render(zonaResumen, [
      { titulo: "Proveedores", valor: modeloProveedores.length, detalle: "registrados" },
      { titulo: "Entrega más rápida", valor: rapido.diasEntrega + " días", detalle: rapido.nombre, tono: "ok" },
    ]);
    FerroSuite.tabla.render(zonaTabla, columnas, modeloProveedores, "Todavía no hay proveedores registrados.");
  }

  FerroSuite.formulario.render(document.querySelector("#form-proveedor"), {
    textoBoton: "Agregar proveedor",
    textoExito: "Proveedor agregado.",
    campos: [
      { nombre: "nombre", etiqueta: "Nombre", requerido: true },
      { nombre: "nit", etiqueta: "NIT", placeholder: "900123456-7", requerido: true },
      { nombre: "telefono", etiqueta: "Teléfono" },
      { nombre: "diasEntrega", etiqueta: "Días de entrega", tipo: "number", min: 0, requerido: true },
    ],
    alEnviar(datos) {
      const errores = servicio.agregarProveedor(datos);
      if (!errores.length) pintarTodo();
      return errores;
    },
  });

  pintarTodo();
});
