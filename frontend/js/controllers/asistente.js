// FerroSuite — Controlador del asistente de IA (pantalla de Inventario).
// Si el backend no responde, se muestra el error y el resto de la pantalla sigue funcionando.
document.addEventListener("DOMContentLoaded", () => {
  const ia = FerroSuite.iaService;
  const panel = FerroSuite.panelAsistente;
  const estado = document.querySelector("#estado-asistente");
  const zona = document.querySelector("#respuesta-asistente");
  const campo = document.querySelector("#texto-pregunta");

  // Muestra en la esquina si el asistente usa Gemini o reglas.
  ia.estado()
    .then((r) => {
      estado.textContent = r.datos.iaConfigurada ? "Conectado a Gemini" : "Sin clave de Gemini (respuestas con reglas)";
      estado.className = "etiqueta " + (r.datos.iaConfigurada ? "etiqueta-ok" : "etiqueta-neutro");
    })
    .catch(() => {
      estado.textContent = "Sin conexión con el backend";
      estado.className = "etiqueta etiqueta-alerta";
    });

  // Ejecuta una consulta al asistente mostrando "cargando" y el error si falla.
  async function consultar(textoCarga, accion) {
    zona.innerHTML = `<p>${textoCarga}</p>`;
    try {
      await accion();
    } catch (error) {
      zona.innerHTML = "";
      FerroSuite.aviso.mostrarError(zona, error);
    }
  }

  document.querySelector("#btn-reposicion").addEventListener("click", () =>
    consultar("Revisando el inventario y las ventas…", async () => panel.mostrarReposicion(zona, (await ia.sugerirReposicion()).datos))
  );

  document.querySelector("#btn-recordatorios").addEventListener("click", () =>
    consultar("Revisando pagos, órdenes y pedidos pendientes…", async () => panel.mostrarRecordatorios(zona, (await ia.recordatorios()).datos))
  );

  function preguntar(pregunta) {
    if (!pregunta.trim()) {
      FerroSuite.aviso.mostrar(zona, "Escribe una pregunta.", "error");
      return;
    }
    consultar("Pensando la respuesta…", async () => panel.mostrarRespuesta(zona, pregunta, (await ia.preguntar(pregunta)).datos));
  }

  document.querySelector("#form-pregunta").addEventListener("submit", (e) => {
    e.preventDefault();
    preguntar(campo.value);
  });

  document.querySelectorAll("[data-pregunta]").forEach((boton) => {
    boton.addEventListener("click", () => {
      campo.value = boton.dataset.pregunta;
      preguntar(boton.dataset.pregunta);
    });
  });

  // "Crear orden" en una sugerencia: se guarda en la base y queda pendiente de aprobación (RF08).
  zona.addEventListener("click", async (e) => {
    const boton = e.target.closest("[data-sku]");
    if (!boton) return;
    const mensajes = zona.querySelector(".form-mensajes");
    boton.disabled = true;
    try {
      const r = await FerroSuite.ordenesService.crear({ sku: boton.dataset.sku, nitProveedor: boton.dataset.nit, cantidad: boton.dataset.cantidad });
      boton.textContent = "Orden creada";
      FerroSuite.aviso.mostrar(mensajes, r.mensaje, "ok");
      document.dispatchEvent(new CustomEvent("ferrosuite:datos-cambiaron"));
    } catch (error) {
      boton.disabled = false;
      FerroSuite.aviso.mostrarError(mensajes, error);
    }
  });
});
