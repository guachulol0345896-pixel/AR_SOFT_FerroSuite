// FerroSuite — Controlador de la pantalla de ingreso (solo demostración).
// No valida credenciales: guarda quién ingresó para registrarlo en cada movimiento.
document.addEventListener("DOMContentLoaded", () => {
  FerroSuite.formulario.render(document.querySelector("#form-login"), {
    textoBoton: "Ingresar",
    campos: [
      { nombre: "usuario", etiqueta: "Usuario", requerido: true },
      { nombre: "clave", etiqueta: "Clave", tipo: "password", requerido: true },
      {
        nombre: "rol", etiqueta: "Rol", tipo: "select",
        opciones: ["Dueño o administrador", "Encargado de bodega", "Vendedor de mostrador", "Contador externo"].map((r) => ({ valor: r, texto: r })),
      },
    ],
    validar(datos) {
      const errores = [];
      if (!datos.usuario) errores.push("Escribe tu usuario.");
      if (!datos.clave) errores.push("Escribe tu clave.");
      return errores;
    },
    async alEnviar(datos) {
      try {
        sessionStorage.setItem("ferrosuite-usuario", datos.usuario + " (" + datos.rol + ")");
      } catch (e) {}
      setTimeout(() => (window.location.href = "index.html"), 900);
      return "Ingreso de demostración correcto. Redirigiendo al inicio…";
    },
  });
});
