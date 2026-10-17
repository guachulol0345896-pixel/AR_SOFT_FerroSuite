// FerroSuite — Arranque del frontend.
// Dibuja los componentes comunes (encabezado y pie) en cada pantalla y deja
// listo el menú móvil. Cada pantalla carga después su propio controlador.
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
