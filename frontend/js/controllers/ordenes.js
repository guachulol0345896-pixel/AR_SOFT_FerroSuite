// FerroSuite — Controlador de Órdenes de compra.
document.addEventListener("DOMContentLoaded", () => {
  const esc = FerroSuite.util.esc;
  const inventario = FerroSuite.inventarioService;
  const zonaResumen = document.querySelector("#resumen-ordenes");
  const zonaTabla = document.querySelector("#tabla-ordenes");

  const nombreProducto = (sku) => {
    const p = modeloProductos.find((x) => x.sku === sku);
    return p ? p.nombre : sku;
  };
  const nombreProveedor = (nit) => {
    const p = FerroSuite.proveedoresService.buscarPorNit(nit);
    return p ? p.nombre : nit;
  };

  const columnas = [
    { titulo: "Código", valor: (o) => esc(o.codigo) },
    { titulo: "Producto", valor: (o) => esc(nombreProducto(o.sku)) },
    { titulo: "Proveedor", valor: (o) => esc(nombreProveedor(o.nit)) },
    { titulo: "Cantidad", valor: (o) => o.cantidad },
    {
      titulo: "Estado",
      valor: (o) => FerroSuite.etiqueta.html(o.estado, o.estado === "Aprobada" ? "ok" : "alerta"),
    },
    {
      titulo: "Acción",
      valor: (o) => (o.estado === "Aprobada" ? "—" : `<button type="button" class="btn-chico" data-aprobar="${esc(o.codigo)}">Aprobar</button>`),
    },
  ];

  function pintarTodo() {
    const pendientes = modeloOrdenes.filter((o) => o.estado !== "Aprobada").length;
    FerroSuite.tarjetaResumen.render(zonaResumen, [
      { titulo: "Órdenes", valor: modeloOrdenes.length, detalle: "registradas" },
      { titulo: "Pendientes", valor: pendientes, detalle: "esperan aprobación del dueño", tono: pendientes ? "alerta" : "ok" },
    ]);
    FerroSuite.tabla.render(zonaTabla, columnas, modeloOrdenes, "Todavía no hay órdenes de compra.");
  }

  // Los productos en riesgo se muestran primero en la lista.
  function opcionesProductos() {
    const ordenados = modeloProductos.slice().sort((a, b) => Number(inventario.productoEnRiesgo(b)) - Number(inventario.productoEnRiesgo(a)));
    return ordenados.map((p) => ({
      valor: p.sku,
      texto: (inventario.productoEnRiesgo(p) ? "⚠ " : "") + p.nombre + " (" + p.sku + ")",
    }));
  }

  zonaTabla.addEventListener("click", (e) => {
    const boton = e.target.closest("[data-aprobar]");
    if (!boton) return;
    FerroSuite.ordenesService.aprobar(boton.dataset.aprobar);
    pintarTodo();
  });

  FerroSuite.formulario.render(document.querySelector("#form-orden"), {
    textoBoton: "Generar orden",
    textoExito: "Orden creada. Queda pendiente de aprobación.",
    campos: [
      { nombre: "sku", etiqueta: "Producto", tipo: "select", opciones: opcionesProductos(), requerido: true },
      {
        nombre: "nit", etiqueta: "Proveedor", tipo: "select", requerido: true,
        opciones: modeloProveedores.map((p) => ({ valor: p.nit, texto: p.nombre + " · entrega en " + p.diasEntrega + " días" })),
      },
      { nombre: "cantidad", etiqueta: "Cantidad a pedir", tipo: "number", min: 1, requerido: true },
    ],
    alEnviar(datos) {
      const errores = FerroSuite.ordenesService.crearOrden(datos);
      if (!errores.length) pintarTodo();
      return errores;
    },
  });

  pintarTodo();
});
