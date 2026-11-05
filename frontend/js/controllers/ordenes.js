// FerroSuite — Controlador de Órdenes de compra.
document.addEventListener("DOMContentLoaded", () => {
  const servicio = FerroSuite.ordenesService;
  const { esc, numero, pesos } = FerroSuite.util;
  const zonaResumen = document.querySelector("#resumen-ordenes");
  const zonaTabla = document.querySelector("#tabla-ordenes");
  const mensajesTabla = document.querySelector("#mensajes-ordenes");

  const columnas = [
    { titulo: "Código", valor: (o) => esc(o.codigo) },
    { titulo: "Producto", valor: (o) => esc(o.nombreProducto) },
    { titulo: "Proveedor", valor: (o) => esc(o.nombreProveedor) },
    { titulo: "Cantidad", valor: (o) => numero(o.cantidad) },
    { titulo: "Valor", valor: (o) => pesos(o.valor) },
    { titulo: "Estado", valor: (o) => FerroSuite.etiqueta.html(o.estado, o.estado === "Aprobada" ? "ok" : "alerta") },
    {
      titulo: "Pago",
      valor: (o) => {
        if (o.pagada) return FerroSuite.etiqueta.html("Pagada", "ok");
        if (!o.vencePago) return "—";
        const vence = new Date(o.vencePago);
        const texto = "Vence " + vence.toLocaleDateString("es-CO", { day: "2-digit", month: "2-digit" });
        return FerroSuite.etiqueta.html(texto, vence < new Date() ? "alerta" : "neutro");
      },
    },
    {
      titulo: "Acción",
      valor: (o) => {
        if (o.estado !== "Aprobada") return `<button type="button" class="btn-chico" data-aprobar="${esc(o.codigo)}">Aprobar</button>`;
        if (!o.pagada) return `<button type="button" class="btn-chico" data-pagar="${esc(o.codigo)}">Marcar pagada</button>`;
        return "—";
      },
    },
  ];

  async function cargar() {
    try {
      const [rOrdenes, rProductos, rProveedores] = await Promise.all([
        servicio.listar(), FerroSuite.inventarioService.listar(), FerroSuite.proveedoresService.listar(),
      ]);
      FerroSuite.ocultarErrorPagina();
      const ordenes = rOrdenes.datos;

      let porPagar = 0;
      for (const o of ordenes) if (o.estado === "Aprobada" && !o.pagada) porPagar += o.valor || 0;
      const pendientes = ordenes.filter((o) => o.estado !== "Aprobada").length;
      FerroSuite.tarjetaResumen.render(zonaResumen, [
        { titulo: "Órdenes", valor: ordenes.length, detalle: "registradas" },
        { titulo: "Pendientes", valor: pendientes, detalle: "esperan aprobación del dueño", tono: pendientes ? "alerta" : "ok" },
        { titulo: "Por pagar", valor: pesos(porPagar), detalle: "a proveedores (plazo de 15 días)", tono: porPagar ? "alerta" : "ok" },
      ]);
      FerroSuite.tabla.render(zonaTabla, columnas, ordenes, "Todavía no hay órdenes de compra.");

      // Los productos en riesgo aparecen primero en la lista.
      const productos = rProductos.datos.slice().sort((a, b) => (a.estado === "Normal") - (b.estado === "Normal"));
      form.actualizarOpciones("sku", productos.map((p) => ({ valor: p.sku, texto: (p.estado !== "Normal" ? "⚠ " : "") + p.nombre + " (" + p.sku + ")" })));
      form.actualizarOpciones("nitProveedor", rProveedores.datos.map((p) => ({ valor: p.nit, texto: p.nombre + " · entrega en " + p.diasEntrega + " días" })));
    } catch (error) {
      FerroSuite.mostrarErrorPagina(error);
    }
  }

  zonaTabla.addEventListener("click", async (e) => {
    const { aprobar, pagar } = e.target.dataset;
    if (!aprobar && !pagar) return;
    try {
      const r = aprobar ? await servicio.aprobar(aprobar) : await servicio.pagar(pagar);
      FerroSuite.aviso.mostrar(mensajesTabla, r.mensaje, "ok");
      await cargar();
    } catch (error) {
      FerroSuite.aviso.mostrarError(mensajesTabla, error);
    }
  });

  const form = FerroSuite.formulario.render(document.querySelector("#form-orden"), {
    textoBoton: "Generar orden",
    campos: [
      { nombre: "sku", etiqueta: "Producto", tipo: "select", opciones: [] },
      { nombre: "nitProveedor", etiqueta: "Proveedor", tipo: "select", opciones: [] },
      { nombre: "cantidad", etiqueta: "Cantidad a pedir", tipo: "number", requerido: true },
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
