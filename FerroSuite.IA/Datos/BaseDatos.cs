using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace FerroSuite.IA.Datos;

/// <summary>
/// Conexión con SQL Server LocalDB (viene con Visual Studio) y creación de la base.
/// Al abrir el programa se ejecutan los scripts de la carpeta BaseDatos: crean la
/// base y las tablas si no existen y cargan los datos de prueba si está vacía.
/// </summary>
public static class BaseDatos
{
    private const string Servidor = @"(localdb)\MSSQLLocalDB";

    /// <summary>Para la prueba 4: si está activo, se intenta conectar a un servidor que no existe.</summary>
    public static bool SimularCaida { get; set; }

    public static string CadenaConexion => SimularCaida
        ? @"Server=(localdb)\ServidorQueNoExiste;Database=FerroSuite;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=3"
        : $"Server={Servidor};Database=FerroSuite;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=30";

    private static string CadenaMaster =>
        $"Server={Servidor};Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=30";

    private static string CarpetaScripts => Path.Combine(AppContext.BaseDirectory, "BaseDatos");

    public static SqlConnection Conectar()
    {
        var conexion = new SqlConnection(CadenaConexion);
        conexion.Open();
        return conexion;
    }

    /// <summary>Crea la base, las tablas y los datos de prueba si hace falta.</summary>
    public static void Preparar()
    {
        EjecutarScript("crear_base_datos.sql");
        EjecutarScript("datos_prueba.sql");
    }

    /// <summary>Borra todos los registros y vuelve a cargar los datos de prueba.</summary>
    public static void Restablecer()
    {
        using (var conexion = Conectar())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                DELETE FROM dbo.OrdenCompra;
                DELETE FROM dbo.MovimientoInventario;
                DELETE FROM dbo.Producto;
                DELETE FROM dbo.Proveedor;
                DBCC CHECKIDENT ('dbo.OrdenCompra', RESEED, 0);
                DBCC CHECKIDENT ('dbo.MovimientoInventario', RESEED, 0);
                """;
            comando.ExecuteNonQuery();
        }
        EjecutarScript("datos_prueba.sql");
    }

    // Un script de SQL Server se divide en lotes con la palabra GO; cada lote se ejecuta aparte.
    private static void EjecutarScript(string archivo)
    {
        var ruta = Path.Combine(CarpetaScripts, archivo);
        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No se encontró el script {archivo} en la carpeta BaseDatos.");

        var lotes = Regex.Split(File.ReadAllText(ruta), @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        using var conexion = new SqlConnection(CadenaMaster);
        conexion.Open();
        foreach (var lote in lotes)
        {
            if (string.IsNullOrWhiteSpace(lote)) continue;
            using var comando = conexion.CreateCommand();
            comando.CommandText = lote;
            comando.ExecuteNonQuery();
        }
    }
}
