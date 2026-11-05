using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FerroSuite.IA.Datos;
using FerroSuite.IA.Modelos;
using FerroSuite.IA.Servicios;
using Microsoft.Data.SqlClient;

namespace FerroSuite.IA.Comunicacion;

/// <summary>
/// Comunicación entre el frontend y el backend. Cumple el papel de una API: el
/// frontend envía { id, accion, datos } (por ejemplo "productos.crear") y el
/// backend responde { id, ok, mensaje, datos, errores }.
/// </summary>
public class Puente(InventarioServicio inventario, ProveedorServicio proveedores, OrdenServicio ordenes,
    AsistenteInventario asistente, ClienteGemini gemini)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Para la prueba 4: si está activo, el backend no responde (como si se cayera la conexión).</summary>
    public bool SimularDesconexion { get; set; }

    /// <summary>Atiende un mensaje del frontend. Devuelve la respuesta en JSON, o null si no debe responder.</summary>
    public async Task<string?> AtenderAsync(string mensajeJson)
    {
        if (SimularDesconexion) return null;

        JsonNode? id = null;
        try
        {
            var mensaje = JsonNode.Parse(mensajeJson) ?? throw new JsonException();
            id = mensaje["id"]?.DeepClone();
            var accion = mensaje["accion"]?.GetValue<string>() ?? "";
            var datos = mensaje["datos"];

            var (mensajeOk, resultado) = await EjecutarAsync(accion, datos);
            return Responder(new { id, ok = true, mensaje = mensajeOk, datos = resultado });
        }
        catch (ErrorValidacion ex)
        {
            return Responder(new { id, ok = false, mensaje = "Revisa los datos ingresados.", errores = ex.Errores });
        }
        catch (SqlException ex)
        {
            Debug.WriteLine("Error de SQL Server: " + ex.Message);
            return Responder(new
            {
                id, ok = false,
                mensaje = "No se pudo completar la operación en la base de datos.",
                errores = new[] { "Revisa que SQL Server LocalDB esté instalado y vuelve a intentar." },
            });
        }
        catch (JsonException)
        {
            return Responder(new { id, ok = false, mensaje = "Los datos enviados tienen un formato incorrecto.", errores = Array.Empty<string>() });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error inesperado: " + ex);
            return Responder(new { id, ok = false, mensaje = "Ocurrió un error inesperado: " + ex.Message, errores = Array.Empty<string>() });
        }
    }

    // Cada acción es como una ruta de una API: indica qué servicio del backend la atiende.
    private async Task<(string? Mensaje, object? Datos)> EjecutarAsync(string accion, JsonNode? datos)
    {
        switch (accion)
        {
            // Inventario
            case "productos.listar": return (null, inventario.ListarProductos());
            case "productos.crear": inventario.CrearProducto(Leer<Producto>(datos)); return ("Producto guardado en la base de datos.", null);
            case "productos.actualizar": inventario.ActualizarProducto(Leer<Producto>(datos)); return ("Cambios guardados.", null);
            case "productos.eliminar": inventario.EliminarProducto(Texto(datos, "sku")); return ("Producto eliminado.", null);
            case "movimientos.listar": return (null, inventario.ListarMovimientos(Numero(datos, "limite", 20)));
            case "movimientos.registrar":
                var stock = inventario.RegistrarMovimiento(Leer<Movimiento>(datos));
                return ($"Movimiento registrado. Nuevo stock: {stock:0.##}.", null);
            case "resumen": return (null, inventario.Resumen());
            case "datos.restablecer": BaseDatos.Restablecer(); return ("Se cargaron de nuevo los datos de prueba.", null);

            // Proveedores
            case "proveedores.listar": return (null, proveedores.Listar());
            case "proveedores.crear": proveedores.Crear(Leer<Proveedor>(datos)); return ("Proveedor guardado en la base de datos.", null);

            // Órdenes de compra
            case "ordenes.listar": return (null, ordenes.Listar());
            case "ordenes.crear":
                var codigo = ordenes.Crear(Leer<OrdenCompra>(datos));
                return ($"Orden {codigo} creada. Queda pendiente de aprobación.", null);
            case "ordenes.aprobar": ordenes.Aprobar(Texto(datos, "codigo")); return ("Orden aprobada. El pago vence en 15 días.", null);
            case "ordenes.pagar": ordenes.MarcarPagada(Texto(datos, "codigo")); return ("Orden marcada como pagada.", null);

            // Asistente de IA (lee los datos de la base y, si hay clave, usa Gemini)
            case "ia.estado": return (null, new EstadoServicio("FerroSuite", gemini.Configurado, gemini.Modelo));
            case "ia.reposicion": return (null, await asistente.SugerirReposicionAsync(inventario.DatosParaAsistente(), CancellationToken.None));
            case "ia.recordatorios": return (null, await asistente.RecordatoriosAsync(inventario.DatosParaAsistente(), CancellationToken.None));
            case "ia.preguntar":
                var pregunta = Texto(datos, "pregunta");
                if (pregunta.Length > 500) throw new ErrorValidacion(["La pregunta no puede pasar de 500 caracteres."]);
                var d = inventario.DatosParaAsistente();
                return (null, await asistente.PreguntarAsync(new SolicitudPregunta(pregunta, d.Productos, d.Movimientos, d.Ordenes), CancellationToken.None));

            default: throw new ErrorValidacion([$"Acción desconocida: {accion}."]);
        }
    }

    private static T Leer<T>(JsonNode? datos) =>
        datos.Deserialize<T>(Json) ?? throw new ErrorValidacion(["No llegaron datos."]);

    private static string Texto(JsonNode? datos, string campo)
    {
        var valor = datos?[campo]?.ToString().Trim() ?? "";
        if (valor.Length == 0) throw new ErrorValidacion([$"Falta el dato \"{campo}\"."]);
        return valor;
    }

    private static int Numero(JsonNode? datos, string campo, int porDefecto) =>
        int.TryParse(datos?[campo]?.ToString(), out var n) ? n : porDefecto;

    private static string Responder(object respuesta) => JsonSerializer.Serialize(respuesta, Json);
}
