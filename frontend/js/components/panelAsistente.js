// FerroSuite — Componente: muestra las respuestas del asistente de IA.
// Todo el texto se escapa: lo que responda la IA nunca se trata como HTML.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.panelAsistente = {
  // Convierte cada línea del texto en un párrafo.
  parrafos(texto) {
    return texto.split("\n").filter((l) => l.trim()).map((l) => `<p>${FerroSuite.util.esc(l)}</p>`).join("");
  },

  etiquetaFuente(fuente) {
    if (fuente === "gemini") return FerroSuite.etiqueta.html("Respuesta de Gemini", "ok");
    return FerroSuite.etiqueta.html("Respuesta sin IA (reglas)", "neutro");
  },

  mostrarRespuesta(contenedor, pregunta, r) {
    contenedor.innerHTML = `
      <p class="asistente-pregunta-hecha">Tú: ${FerroSuite.util.esc(pregunta)}</p>
      ${this.parrafos(r.respuesta)}
      ${this.etiquetaFuente(r.fuente)}`;
  },

  mostrarRecordatorios(contenedor, r) {
    const esc = FerroSuite.util.esc;
    let items = "";
    for (const rec of r.recordatorios) {
      items += `<li>${FerroSuite.etiqueta.html(rec.tipo, rec.prioridad === "Alta" ? "alerta" : "neutro")}
        <strong>${esc(rec.titulo)}</strong> — ${esc(rec.detalle)}</li>`;
    }
    contenedor.innerHTML = `
      <div class="recordatorio-mensaje">${this.parrafos(r.resumen)}</div>
      ${items ? `<ul class="lista-recordatorios">${items}</ul>` : ""}
      ${this.etiquetaFuente(r.fuente)}`;
  },

  mostrarReposicion(contenedor, r) {
    const { esc, cantidad, pesos } = FerroSuite.util;
    let filas = "";
    for (const s of r.sugerencias) {
      filas += `<tr>
        <td>${FerroSuite.etiqueta.html(s.prioridad, s.prioridad === "Alta" ? "alerta" : "neutro")}</td>
        <td><strong>${esc(s.nombre)}</strong><span class="sub">${esc(s.motivo)}</span></td>
        <td>${cantidad(s.cantidadSugerida, s.unidad)}</td>
        <td>${esc(s.proveedor)}</td>
        <td>${pesos(s.costoEstimado)}</td>
        <td><button type="button" class="btn-chico" data-sku="${esc(s.sku)}" data-nit="${esc(s.nitProveedor)}" data-cantidad="${s.cantidadSugerida}">Crear orden</button></td>
      </tr>`;
    }
    let pagos = "";
    for (const c of r.costosPorProveedor) pagos += `<li>${esc(c.proveedor)}: <strong>${pesos(c.total)}</strong></li>`;

    contenedor.innerHTML = `
      ${this.parrafos(r.resumen)}
      ${filas ? `<div class="table-wrap"><table class="data-table">
        <thead><tr><th>Prioridad</th><th>Producto</th><th>Pedir</th><th>Proveedor</th><th>Costo</th><th></th></tr></thead>
        <tbody>${filas}</tbody></table></div>` : ""}
      ${pagos ? `<h4>Para planear los pagos a proveedores</h4><ul>${pagos}<li>Total: <strong>${pesos(r.costoTotal)}</strong></li></ul>` : ""}
      ${this.etiquetaFuente(r.fuente)}
      <div class="form-mensajes"></div>`;
  },
};
