// FerroSuite — Componente: formulario generado a partir de una lista de campos.
// campos: [{ nombre, etiqueta, tipo, opciones, placeholder, requerido }]
// validar(datos): validación básica en pantalla; devuelve una lista de errores.
// alEnviar(datos): envía al backend; devuelve el mensaje de confirmación o lanza un error.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.formulario = {
  render(contenedor, { campos, textoBoton, validar, alEnviar }) {
    const esc = FerroSuite.util.esc;
    const opcionesHtml = (opciones) => opciones.map((o) => `<option value="${esc(o.valor)}">${esc(o.texto)}</option>`).join("");

    let html = "";
    for (const c of campos) {
      const req = c.requerido ? " required" : "";
      let control;
      if (c.tipo === "select") control = `<select name="${c.nombre}"${req}>${opcionesHtml(c.opciones || [])}</select>`;
      else control = `<input type="${c.tipo || "text"}" name="${c.nombre}" step="any" placeholder="${esc(c.placeholder || "")}"${req} />`;
      html += `<label>${c.etiqueta}${control}</label>`;
    }

    contenedor.innerHTML = `
      <form class="form-grid" novalidate>
        ${html}
        <button type="submit" class="btn btn-primary">${textoBoton}</button>
      </form>
      <div class="form-mensajes"></div>`;

    const form = contenedor.querySelector("form");
    const mensajes = contenedor.querySelector(".form-mensajes");
    const boton = form.querySelector("button");

    form.addEventListener("submit", async (e) => {
      e.preventDefault();
      const datos = {};
      for (const c of campos) datos[c.nombre] = form.elements[c.nombre].value.trim();

      // 1. Validación básica en pantalla, antes de enviar.
      const errores = validar ? validar(datos) : [];
      if (errores.length) {
        FerroSuite.aviso.mostrar(mensajes, errores, "error");
        return;
      }

      // 2. Envío al backend. Mientras responde, el botón queda desactivado.
      boton.disabled = true;
      FerroSuite.aviso.mostrar(mensajes, "Enviando…", "ok");
      try {
        const confirmacion = await alEnviar(datos);
        FerroSuite.aviso.mostrar(mensajes, confirmacion || "Guardado.", "ok");
        form.reset();
      } catch (error) {
        FerroSuite.aviso.mostrarError(mensajes, error);
      } finally {
        boton.disabled = false;
      }
    });

    // Funciones para que el controlador pueda manejar el formulario.
    return {
      form,
      actualizarOpciones(nombre, opciones) {
        form.elements[nombre].innerHTML = opcionesHtml(opciones);
      },
      llenar(valores) {
        for (const nombre in valores) if (form.elements[nombre]) form.elements[nombre].value = valores[nombre] ?? "";
        mensajes.innerHTML = "";
      },
      cambiarBoton(texto) {
        boton.textContent = texto;
      },
      limpiarMensajes() {
        mensajes.innerHTML = "";
      },
    };
  },
};
