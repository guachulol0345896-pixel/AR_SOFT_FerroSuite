// FerroSuite — Componente: mensajes de confirmación o de error.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.aviso = {
  mostrar(contenedor, mensajes, tipo) {
    const lista = Array.isArray(mensajes) ? mensajes : [mensajes];
    if (!lista.length) {
      contenedor.innerHTML = "";
      return;
    }
    const items = lista.map((m) => `<li>${FerroSuite.util.esc(m)}</li>`).join("");
    contenedor.innerHTML = `<ul class="aviso aviso-${tipo || "error"}" role="${tipo === "ok" ? "status" : "alert"}">${items}</ul>`;
  },

  // Muestra un error que vino del backend: el mensaje general y la lista de detalles.
  mostrarError(contenedor, error) {
    const detalles = error.errores && error.errores.length ? error.errores : [];
    this.mostrar(contenedor, detalles.length ? detalles : [error.message], "error");
  },
};
