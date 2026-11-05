// FerroSuite — Aplicativo de escritorio.
// Frontend: las pantallas HTML de la carpeta "frontend", mostradas en la ventana.
// Backend: los servicios en C# (carpeta Servicios), con la lógica y las validaciones.
// Base de datos: SQL Server LocalDB (carpetas Datos y BaseDatos).
// Comunicación: mensajes entre las pantallas y el backend (carpeta Comunicacion).
namespace FerroSuite.IA;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new VentanaPrincipal());
    }
}
