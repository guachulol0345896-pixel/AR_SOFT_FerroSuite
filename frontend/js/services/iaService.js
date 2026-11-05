// FerroSuite — Servicio del asistente de IA. El backend lee los datos de la base,
// hace las cuentas y, si hay clave, le pide a Gemini que las explique.
window.FerroSuite = window.FerroSuite || {};

FerroSuite.iaService = {
    estado() {
        return FerroSuite.api.pedir("ia.estado");
    },
    sugerirReposicion() {
        return FerroSuite.api.pedir("ia.reposicion", null, 75000);
    },
    recordatorios() {
        return FerroSuite.api.pedir("ia.recordatorios", null, 75000);
    },
    preguntar(pregunta) {
        return FerroSuite.api.pedir("ia.preguntar", { pregunta }, 75000);
    },
};