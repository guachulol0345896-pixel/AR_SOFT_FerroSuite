// FerroSuite — Componente: encabezado con marca y menú de navegación.
// Se dibuja en cualquier <div data-componente="encabezado"> de la página, para
// no repetir el mismo bloque HTML en cada pantalla. La página activa se toma
// del atributo data-pagina del <body>.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.menuPrincipal = [
  { id: "inicio", texto: "Inicio", enlace: "index.html" },
  { id: "inventario", texto: "Inventario", enlace: "inventario.html" },
  { id: "proveedores", texto: "Proveedores", enlace: "proveedores.html" },
  { id: "ordenes", texto: "Órdenes de compra", enlace: "ordenes.html" },
];

FerroSuite.encabezado = {
  html(paginaActiva) {
    const items = FerroSuite.menuPrincipal
      .map((m) => {
        const actual = m.id === paginaActiva ? ' aria-current="page"' : "";
        return `<li><a href="${m.enlace}"${actual}>${m.texto}</a></li>`;
      })
      .join("");
    const loginActual = paginaActiva === "login" ? ' aria-current="page"' : "";
    return `
    <header class="site-header">
      <div class="container">
        <a class="brand" href="index.html">
          <svg class="brand-mark" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <path d="M22 4a6 6 0 0 0-7.9 6.4L4 20.5V26h5.5l10.1-10.1A6 6 0 0 0 22 4Z" stroke="#F2994A" stroke-width="2" stroke-linejoin="round"/>
            <circle cx="21.5" cy="8.5" r="1.6" fill="#F2994A"/>
          </svg>
          <span class="brand-name">FerroSuite<small>ERP para ferreterías pyme</small></span>
        </a>
        <button class="nav-toggle" aria-label="Abrir menú" aria-expanded="false">☰</button>
        <nav class="site-nav" aria-label="Navegación principal">
          <ul>${items}<li class="nav-ingresar"><a href="login.html"${loginActual}>Ingresar</a></li></ul>
        </nav>
      </div>
    </header>`;
  },

  // Abre y cierra el menú en pantallas pequeñas.
  activarMenuMovil(raiz) {
    const boton = raiz.querySelector(".nav-toggle");
    const nav = raiz.querySelector(".site-nav");
    if (!boton || !nav) return;
    boton.addEventListener("click", () => {
      const abierto = nav.classList.toggle("is-open");
      boton.setAttribute("aria-expanded", abierto ? "true" : "false");
    });
  },
};
