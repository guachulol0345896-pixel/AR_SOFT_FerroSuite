// FerroSuite — Componente: formulario generado a partir de una lista de campos.
// campos: [{ nombre, etiqueta, tipo, opciones, placeholder, min, requerido }]
// alEnviar(datos) debe devolver una lista de errores; si está vacía, se limpia el formulario.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.formulario = {
  render(contenedor, { campos, textoBoton, textoExito, alEnviar }) {
    const esc = FerroSuite.util.esc;
    const html = campos
      .map((c) => {
        const req = c.requerido ? " required" : "";
        let control;
        if (c.tipo === "select") {
          control = `<select name="${c.nombre}"${req}>${(c.opciones || [])
            .map((o) => `<option value="${esc(o.valor)}">${esc(o.texto)}</option>`)
            .join("")}</select>`;
        } else {
          const min = c.min !== undefined ? ` min="${c.min}"` : "";
          const ph = c.placeholder ? ` placeholder="${esc(c.placeholder)}"` : "";
          control = `<input type="${c.tipo || "text"}" name="${c.nombre}"${min}${ph}${req} />`;
        }
        return `<label>${c.etiqueta}${control}</label>`;
      })
      .join("");

    contenedor.innerHTML = `
      <form class="form-grid" novalidate>
        ${html}
        <button type="submit" class="btn btn-primary">${textoBoton}</button>
      </form>
      <div class="form-mensajes"></div>`;

    const form = contenedor.querySelector("form");
    const zonaMensajes = contenedor.querySelector(".form-mensajes");

    form.addEventListener("submit", (e) => {
      e.preventDefault();
      const datos = {};
      campos.forEach((c) => (datos[c.nombre] = form.elements[c.nombre].value.trim()));
      const errores = alEnviar(datos) || [];
      if (errores.length) {
        FerroSuite.aviso.mostrar(zonaMensajes, errores, "error");
      } else {
        FerroSuite.aviso.mostrar(zonaMensajes, textoExito || "Guardado.", "ok");
        form.reset();
      }
    });
    return {
      // Permite volver a llenar las opciones de un <select> cuando cambian los datos.
      actualizarOpciones(nombre, opciones) {
        const sel = form.elements[nombre];
        sel.innerHTML = opciones
          .map((o) => `<option value="${esc(o.valor)}">${esc(o.texto)}</option>`)
          .join("");
      },
    };
  },
};
