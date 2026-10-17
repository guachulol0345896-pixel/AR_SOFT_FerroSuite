// FerroSuite — Componente: mensaje breve de error o de éxito sobre un formulario.
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
};
