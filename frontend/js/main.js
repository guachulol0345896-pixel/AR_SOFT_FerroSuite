// FerroSuite — Arranque del frontend.
// Dibuja los componentes comunes (encabezado y pie) en cada pantalla y deja
// listo el menú móvil. Cada pantalla carga después su propio controlador.
window.FerroSuite = window.FerroSuite || {};

document.addEventListener("DOMContentLoaded", () => {
  const pagina = document.body.dataset.pagina || "";

  const zonaEncabezado = document.querySelector('[data-componente="encabezado"]');
  if (zonaEncabezado) {
    zonaEncabezado.innerHTML = FerroSuite.encabezado.html(pagina);
    FerroSuite.encabezado.activarMenuMovil(zonaEncabezado);
  }

  const zonaPie = document.querySelector('[data-componente="pie"]');
  if (zonaPie) zonaPie.innerHTML = FerroSuite.pie.html();
});

// Muestra un aviso de error arriba de la pantalla (por ejemplo, si el backend no responde).
FerroSuite.mostrarErrorPagina = function (error) {
  let zona = document.querySelector("#aviso-pagina");
  if (!zona) {
    zona = document.createElement("div");
    zona.id = "aviso-pagina";
    zona.className = "container aviso-pagina";
    document.querySelector("main").prepend(zona);
  }
  FerroSuite.aviso.mostrarError(zona, error);
};

FerroSuite.ocultarErrorPagina = function () {
  const zona = document.querySelector("#aviso-pagina");
  if (zona) zona.remove();
};
