namespace FerroSuite.IA;

/// <summary>
/// Configuración local del programa. La clave de Gemini se guarda en
/// %LocalAppData%\FerroSuite\clave-gemini.txt (carpeta del usuario que Windows no bloquea),
/// fuera del repositorio, para que no se suba a GitHub.
/// </summary>
public static class Configuracion
{
    public static readonly string Carpeta =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FerroSuite");

    private static readonly string ArchivoClave = Path.Combine(Carpeta, "clave-gemini.txt");

    /// <summary>Primero busca la variable de entorno GEMINI_API_KEY; si no, el archivo.</summary>
    public static string LeerClaveGemini()
    {
        var deEntorno = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(deEntorno)) return deEntorno.Trim();
        try
        {
            return File.Exists(ArchivoClave) ? File.ReadAllText(ArchivoClave).Trim() : "";
        }
        catch (IOException)
        {
            return "";
        }
        catch (UnauthorizedAccessException)
        {
            return "";
        }
    }

    /// <summary>Guarda la clave. Devuelve un mensaje de error o null si todo salió bien.</summary>
    public static string? GuardarClaveGemini(string clave)
    {
        try
        {
            Directory.CreateDirectory(Carpeta);
            File.WriteAllText(ArchivoClave, clave.Trim());
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"No se pudo guardar la clave en {ArchivoClave}. Detalle: {ex.Message}";
        }
    }
}