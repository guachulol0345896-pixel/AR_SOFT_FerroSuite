// FerroSuite — Comunicación con el backend.
// Las pantallas no guardan datos: le piden todo al backend en C#. Cada solicitud es
// { id, accion, datos } (por ejemplo "productos.crear") y el backend responde
// { id, ok, mensaje, datos, errores }. Es el mismo papel que cumple una API REST,
// pero por el canal de mensajes de la ventana de escritorio (WebView2).
window.FerroSuite = window.FerroSuite || {};

FerroSuite.api = {
  esperaMaxima: 10000, // si el backend no responde en 10 s, se avisa al usuario (el asistente de IA espera 30 s)
  pendientes: {},
  siguienteId: 1,

  // ¿La pantalla está abierta dentro del aplicativo FerroSuite?
  disponible() {
    return !!(window.chrome && window.chrome.webview);
  },

  pedir(accion, datos, espera) {
    return new Promise((resolver, rechazar) => {
      if (!this.disponible()) {
        rechazar(new Error("Esta pantalla debe abrirse desde el aplicativo FerroSuite (Visual Studio → F5)."));
        return;
      }
      const id = this.siguienteId++;
      const temporizador = setTimeout(() => {
        delete this.pendientes[id];
        rechazar(new Error("No hay comunicación con el backend. Intenta de nuevo en unos segundos."));
      }, espera || this.esperaMaxima);
      this.pendientes[id] = { resolver, rechazar, temporizador };
      window.chrome.webview.postMessage({ id, accion, datos: datos || null });
    });
  },

  // Llega la respuesta del backend: se busca la solicitud por su id y se entrega el resultado.
  recibir(respuesta) {
    const pendiente = this.pendientes[respuesta.id];
    if (!pendiente) return;
    clearTimeout(pendiente.temporizador);
    delete this.pendientes[respuesta.id];
    if (respuesta.ok) {
      pendiente.resolver({ mensaje: respuesta.mensaje, datos: respuesta.datos });
    } else {
      const error = new Error(respuesta.mensaje || "No se pudo completar la operación.");
      error.errores = respuesta.errores || [];
      pendiente.rechazar(error);
    }
  },
};

if (FerroSuite.api.disponible()) {
  window.chrome.webview.addEventListener("message", (e) => FerroSuite.api.recibir(e.data));
}
