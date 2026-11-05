using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FerroSuite.IA.Servicios;

public sealed class OpcionesGemini
{
    public string ApiKey { get; set; } = "";
    public string Modelo { get; set; } = "gemini-flash-latest";
    public string UrlBase { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public int TimeoutSegundos { get; set; } = 60;
}

/// <summary>
/// Llama a la API de Gemini (generateContent) por internet. Si no hay clave, si se
/// demora demasiado o si responde con error, devuelve null y el asistente usa
/// sus reglas de respaldo: la pantalla nunca se queda sin respuesta.
/// El motivo del último fallo queda en UltimoError para poder revisarlo.
/// </summary>
public sealed class ClienteGemini(HttpClient http, OpcionesGemini opciones)
{
    public bool Configurado => !string.IsNullOrWhiteSpace(opciones.ApiKey);
    public string Modelo => opciones.Modelo;

    /// <summary>Por qué falló la última consulta (null si salió bien).</summary>
    public string? UltimoError { get; private set; }

    public void CambiarClave(string clave) => opciones.ApiKey = clave.Trim();

    public async Task<string?> GenerarAsync(string instrucciones, string mensaje, CancellationToken ct)
    {
        if (!Configurado) return Fallo("No hay clave de Gemini configurada.");

        var cuerpo = new JsonObject
        {
            ["systemInstruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = instrucciones })
            },
            ["contents"] = new JsonArray(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray(new JsonObject { ["text"] = mensaje })
            }),
            // Límite amplio: los modelos nuevos "piensan" antes de responder y eso también gasta tokens.
            ["generationConfig"] = new JsonObject
            {
                ["temperature"] = 0.3,
                ["maxOutputTokens"] = 8192
            }
        };

        var url = $"{opciones.UrlBase.TrimEnd('/')}/models/{Uri.EscapeDataString(opciones.Modelo)}:generateContent";
        using var peticion = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(cuerpo.ToJsonString(), Encoding.UTF8, "application/json")
        };
        // La clave va en un encabezado, no en la dirección, para que no quede en registros.
        peticion.Headers.Add("x-goog-api-key", opciones.ApiKey);

        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(TimeSpan.FromSeconds(opciones.TimeoutSegundos));

        try
        {
            using var respuesta = await http.SendAsync(peticion, limite.Token);
            var texto = await respuesta.Content.ReadAsStringAsync(limite.Token);
            var json = JsonNode.Parse(texto);

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = json?["error"]?["message"]?.GetValue<string>() ?? "sin detalle";
                return Fallo($"Gemini respondió {(int)respuesta.StatusCode}: {detalle}");
            }

            var candidato = json?["candidates"]?[0];
            var partes = candidato?["content"]?["parts"]?.AsArray();
            var resultado = partes is null ? "" : string.Concat(partes
                .Where(p => p?["thought"]?.GetValue<bool>() != true)
                .Select(p => p?["text"]?.GetValue<string>() ?? ""));

            if (string.IsNullOrWhiteSpace(resultado))
            {
                var razon = candidato?["finishReason"]?.GetValue<string>()
                            ?? json?["promptFeedback"]?["blockReason"]?.GetValue<string>()
                            ?? "desconocida";
                return Fallo($"Gemini no devolvió texto (razón: {razon}).");
            }

            UltimoError = null;
            return resultado.Trim();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Fallo($"Gemini tardó más de {opciones.TimeoutSegundos} segundos.");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException)
        {
            return Fallo($"No se pudo consultar Gemini ({ex.GetType().Name}): {ex.Message}");
        }
    }

    /// <summary>Hace una pregunta corta para comprobar la clave y el modelo. Devuelve un mensaje para el usuario.</summary>
    public async Task<string> ProbarAsync()
    {
        var respuesta = await GenerarAsync("Responde en español, en una sola frase corta.", "Saluda al dueño de una ferretería.", CancellationToken.None);
        return respuesta is null
            ? $"No funcionó.\n\nModelo: {Modelo}\nMotivo: {UltimoError}"
            : $"Conexión correcta con Gemini ({Modelo}).\n\nRespuesta de prueba: {respuesta}";
    }

    private string? Fallo(string motivo)
    {
        UltimoError = motivo;
        Debug.WriteLine(motivo + " Se usan las reglas de respaldo.");
        return null;
    }
}