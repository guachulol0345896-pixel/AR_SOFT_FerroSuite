using System.Diagnostics;
using System.Net;
using FerroSuite.IA.Comunicacion;
using FerroSuite.IA.Datos;
using FerroSuite.IA.Servicios;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace FerroSuite.IA;

/// <summary>
/// Ventana del aplicativo. Muestra las pantallas (carpeta frontend) dentro de un
/// navegador integrado (WebView2) y pasa sus mensajes al backend a través del Puente.
/// No abre ningún puerto ni usa localhost.
/// </summary>
public class VentanaPrincipal : Form
{
    private const string Dominio = "ferrosuite.example"; // dirección interna, solo existe dentro del programa

    private readonly WebView2 vista = new() { Dock = DockStyle.Fill };
    private readonly ClienteGemini gemini;
    private readonly Puente puente;

    public VentanaPrincipal()
    {
        Text = "FerroSuite";
        Width = 1280;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;

        // Se arman las capas: datos → servicios → comunicación.
        var almacen = new AlmacenSqlServer();
        var calculadora = new CalculadoraReposicion();
        gemini = new ClienteGemini(new HttpClient(), new OpcionesGemini { ApiKey = Configuracion.LeerClaveGemini() });
        var asistente = new AsistenteInventario(calculadora, gemini, TimeProvider.System);
        puente = new Puente(new InventarioServicio(almacen, calculadora), new ProveedorServicio(almacen),
            new OrdenServicio(almacen), asistente, gemini);

        Controls.Add(vista);
        Controls.Add(CrearMenu());
        Load += async (s, e) => await IniciarAsync();
    }

    private MenuStrip CrearMenu()
    {
        var menu = new MenuStrip();
        var opciones = new ToolStripMenuItem("Opciones");
        opciones.DropDownItems.Add("Clave de Gemini…", null, (s, e) => PedirClaveGemini());
        opciones.DropDownItems.Add("Probar conexión con Gemini", null, async (s, e) => await ProbarGeminiAsync());
        opciones.DropDownItems.Add("Recargar pantalla", null, (s, e) => vista.Reload());

        // Opciones para la prueba de integración 4 (error de comunicación o de conexión).
        var sinComunicacion = new ToolStripMenuItem("Simular error de comunicación") { CheckOnClick = true };
        sinComunicacion.CheckedChanged += (s, e) => puente.SimularDesconexion = sinComunicacion.Checked;
        var sinBaseDatos = new ToolStripMenuItem("Simular caída de la base de datos") { CheckOnClick = true };
        sinBaseDatos.CheckedChanged += (s, e) => BaseDatos.SimularCaida = sinBaseDatos.Checked;
        opciones.DropDownItems.Add(new ToolStripSeparator());
        opciones.DropDownItems.Add(sinComunicacion);
        opciones.DropDownItems.Add(sinBaseDatos);

        menu.Items.Add(opciones);
        MainMenuStrip = menu;
        return menu;
    }

    private async Task IniciarAsync()
    {
        try
        {
            await vista.EnsureCoreWebView2Async();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show("Falta Microsoft Edge WebView2 Runtime. Instálalo desde microsoft.com y vuelve a abrir FerroSuite.",
                "FerroSuite", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        vista.CoreWebView2.NavigateToString(Pagina("Preparando la base de datos…", "La primera vez puede tardar unos segundos."));

        // La base de datos se prepara en segundo plano para no congelar la ventana.
        try
        {
            await Task.Run(BaseDatos.Preparar);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            vista.CoreWebView2.NavigateToString(Pagina("No se pudo conectar con la base de datos",
                "Revisa que SQL Server Express LocalDB esté instalado (viene con Visual Studio, en la carga de trabajo " +
                "\"Desarrollo de ASP.NET y web\") y vuelve a abrir el programa. Detalle: " + ex.Message));
            return;
        }

        var carpetaFrontend = Path.Combine(AppContext.BaseDirectory, "frontend");
        vista.CoreWebView2.SetVirtualHostNameToFolderMapping(Dominio, carpetaFrontend, CoreWebView2HostResourceAccessKind.Allow);
        vista.CoreWebView2.WebMessageReceived += AlRecibirMensaje;
        vista.CoreWebView2.Navigate($"https://{Dominio}/index.html");
    }

    // Llega una solicitud del frontend: el Puente la atiende y se devuelve la respuesta.
    private async void AlRecibirMensaje(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var respuesta = await puente.AtenderAsync(e.WebMessageAsJson);
        if (respuesta is null || vista.CoreWebView2 is null) return;
        vista.CoreWebView2.PostWebMessageAsJson(respuesta);
    }

    private void PedirClaveGemini()
    {
        using var ventana = new Form
        {
            Text = "Clave de Gemini",
            Width = 520,
            Height = 190,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
        };
        var texto = new Label { Text = "Pega la clave que sacaste en aistudio.google.com/apikey:", Left = 16, Top = 16, Width = 470 };
        var campo = new TextBox { Left = 16, Top = 44, Width = 470, UseSystemPasswordChar = true, Text = Configuracion.LeerClaveGemini() };
        var guardar = new Button { Text = "Guardar", Left = 300, Top = 90, Width = 90, DialogResult = DialogResult.OK };
        var cancelar = new Button { Text = "Cancelar", Left = 396, Top = 90, Width = 90, DialogResult = DialogResult.Cancel };
        ventana.Controls.AddRange([texto, campo, guardar, cancelar]);
        ventana.AcceptButton = guardar;
        ventana.CancelButton = cancelar;

        if (ventana.ShowDialog(this) != DialogResult.OK) return;

        // La clave se usa de inmediato; si no se pudo guardar en el archivo, se avisa
        // (en ese caso habrá que ponerla otra vez al abrir el programa).
        gemini.CambiarClave(campo.Text);
        var error = Configuracion.GuardarClaveGemini(campo.Text);
        if (error is not null)
            MessageBox.Show(error + "\n\nPor ahora la clave sí se usa, pero tendrás que ponerla otra vez al abrir el programa.",
                "FerroSuite", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        else
            MessageBox.Show(gemini.Configurado ? "Clave guardada. El asistente ya usa Gemini." : "Clave borrada. El asistente responderá con reglas.",
                "FerroSuite", MessageBoxButtons.OK, MessageBoxIcon.Information);
        vista.Reload();
    }

    // Hace una pregunta de prueba a Gemini y muestra el resultado o el error exacto.
    private async Task ProbarGeminiAsync()
    {
        Cursor = Cursors.WaitCursor;
        var resultado = await gemini.ProbarAsync();
        Cursor = Cursors.Default;
        MessageBox.Show(resultado, "Prueba de Gemini", MessageBoxButtons.OK,
            gemini.UltimoError is null ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private static string Pagina(string titulo, string detalle) => $"""
        <html><body style="font-family:Segoe UI,Arial;background:#efede8;color:#202d3f;padding:48px">
        <h2>{WebUtility.HtmlEncode(titulo)}</h2><p>{WebUtility.HtmlEncode(detalle)}</p></body></html>
        """;
}