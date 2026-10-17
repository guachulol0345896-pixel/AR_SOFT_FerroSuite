// FerroSuite — Controlador de la pantalla de ingreso (solo demostración).
// No valida credenciales: lleva al inicio. La autenticación por rol llegará después.
document.addEventListener("DOMContentLoaded", () => {
  FerroSuite.formulario.render(document.querySelector("#form-login"), {
    textoBoton: "Ingresar",
    textoExito: "Ingreso de demostración correcto. Redirigiendo al inicio…",
    campos: [
      { nombre: "usuario", etiqueta: "Usuario", requerido: true },
      { nombre: "clave", etiqueta: "Clave", tipo: "password", requerido: true },
      {
        nombre: "rol", etiqueta: "Rol", tipo: "select",
        opciones: ["Dueño o administrador", "Encargado de bodega", "Vendedor de mostrador", "Contador externo"].map((r) => ({ valor: r, texto: r })),
      },
    ],
    alEnviar(datos) {
      const errores = [];
      if (!datos.usuario) errores.push("Escribe tu usuario.");
      if (!datos.clave) errores.push("Escribe tu clave.");
      if (!errores.length) setTimeout(() => (window.location.href = "index.html"), 900);
      return errores;
    },
  });
});
