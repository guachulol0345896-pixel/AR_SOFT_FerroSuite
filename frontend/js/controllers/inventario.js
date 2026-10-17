// FerroSuite — Controlador de Inventario.
// Conecta la pantalla con el servicio y con los componentes: no tiene reglas del
// negocio propias ni HTML repetido, solo decide qué se pinta y cuándo.
document.addEventListener("DOMContentLoaded", () => {
  const servicio = FerroSuite.inventarioService;
  const esc = FerroSuite.util.esc;

  const zonaResumen = document.querySelector("#resumen-inventario");
  const zonaAlerta = document.querySelector("#alerta-inventario");
  const zonaTabla = document.querySelector("#tabla-productos");
  const zonaMovimientos = document.querySelector("#tabla-movimientos");
  const buscador = document.querySelector("#buscar-producto");

  const columnasProductos = [
    { titulo: "SKU", valor: (p) => esc(p.sku) },
    { titulo: "Nombre", valor: (p) => esc(p.nombre) },
    { titulo: "Unidad", valor: (p) => esc(p.unidad) },
    { titulo: "Stock actual", valor: (p) => p.stockActual },
    { titulo: "Stock mínimo", valor: (p) => p.stockMinimo },
    {
      titulo: "Estado",
      valor: (p) => (servicio.productoEnRiesgo(p) ? FerroSuite.etiqueta.html("Stock bajo", "alerta") : FerroSuite.etiqueta.html("Normal", "neutro")),
    },
  ];

  const columnasMovimientos = [
    { titulo: "Fecha", valor: (m) => esc(m.fecha) },
    { titulo: "Producto", valor: (m) => esc(m.nombre) + " (" + esc(m.sku) + ")" },
    { titulo: "Tipo", valor: (m) => FerroSuite.etiqueta.html(m.tipo, m.tipo === "Entrada" ? "ok" : "neutro") },
    { titulo: "Cantidad", valor: (m) => m.cantidad },
  ];

  const opcionesProductos = () =>
    modeloProductos.map((p) => ({ valor: p.sku, texto: p.nombre + " (" + p.sku + ")" }));

  let formMovimiento;

  function pintarTodo() {
    const enRiesgo = servicio.buscarProductosEnRiesgo();
    FerroSuite.tarjetaResumen.render(zonaResumen, [
      { titulo: "Productos", valor: modeloProductos.length, detalle: "en el catálogo" },
      { titulo: "En riesgo", valor: enRiesgo.length, detalle: "con stock igual o bajo el mínimo", tono: enRiesgo.length ? "alerta" : "ok" },
      { titulo: "Movimientos", valor: modeloMovimientos.length, detalle: "registrados en esta sesión" },
    ]);
    FerroSuite.panelAlerta.render(zonaAlerta, enRiesgo);
    FerroSuite.tabla.render(zonaTabla, columnasProductos, servicio.buscarPorTexto(buscador.value), "No hay productos que coincidan con la búsqueda.");
    FerroSuite.tabla.render(zonaMovimientos, columnasMovimientos, modeloMovimientos, "Todavía no se han registrado movimientos.");
    if (formMovimiento) formMovimiento.actualizarOpciones("sku", opcionesProductos());
  }

  formMovimiento = FerroSuite.formulario.render(document.querySelector("#form-movimiento"), {
    textoBoton: "Registrar movimiento",
    textoExito: "Movimiento registrado y stock actualizado.",
    campos: [
      { nombre: "sku", etiqueta: "Producto", tipo: "select", opciones: opcionesProductos(), requerido: true },
      { nombre: "tipo", etiqueta: "Tipo", tipo: "select", opciones: [{ valor: "Entrada", texto: "Entrada" }, { valor: "Salida", texto: "Salida" }] },
      { nombre: "cantidad", etiqueta: "Cantidad", tipo: "number", min: 1, requerido: true },
    ],
    alEnviar(datos) {
      const errores = servicio.registrarMovimiento(datos);
      if (!errores.length) pintarTodo();
      return errores;
    },
  });

  FerroSuite.formulario.render(document.querySelector("#form-producto"), {
    textoBoton: "Agregar producto",
    textoExito: "Producto agregado al catálogo.",
    campos: [
      { nombre: "sku", etiqueta: "SKU", placeholder: "FER-0001", requerido: true },
      { nombre: "nombre", etiqueta: "Nombre", requerido: true },
      {
        nombre: "unidad", etiqueta: "Unidad de medida", tipo: "select",
        opciones: ["Unidad", "Metro", "Kilogramo", "Varilla"].map((u) => ({ valor: u, texto: u })),
      },
      { nombre: "stockActual", etiqueta: "Stock actual", tipo: "number", min: 0, requerido: true },
      { nombre: "stockMinimo", etiqueta: "Stock mínimo", tipo: "number", min: 0, requerido: true },
    ],
    alEnviar(datos) {
      const errores = servicio.agregarProducto(datos);
      if (!errores.length) pintarTodo();
      return errores;
    },
  });

  buscador.addEventListener("input", pintarTodo);
  pintarTodo();
});
