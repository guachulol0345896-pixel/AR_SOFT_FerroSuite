using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Servicios;

/// <summary>
/// Arma la lista de pendientes del dueño con reglas fijas:
/// pagos a proveedores por vencer, órdenes por aprobar y pedidos por hacer.
/// </summary>
public static class GeneradorRecordatorios
{
    public const int PlazoPagoDias = 15;      // el pago vence 15 días después de aprobar la orden
    public const int AvisarPagoConDias = 5;   // se recuerda cuando faltan 5 días o menos
    public const int DiasOrdenReciente = 15;  // si hay una orden de hace menos de 15 días, ya se pidió

    public static List<Recordatorio> Generar(ResultadoReposicion reposicion, IReadOnlyList<OrdenDto> ordenes, DateTimeOffset ahora)
    {
        var lista = new List<Recordatorio>();

        // 1. Pagos de órdenes aprobadas que no se han pagado.
        foreach (var o in ordenes.Where(o => o.Estado == "Aprobada" && !o.Pagada && o.FechaAprobacion is not null))
        {
            var vence = o.FechaAprobacion!.Value.AddDays(PlazoPagoDias);
            var dias = (int)Math.Ceiling((vence - ahora).TotalDays);
            if (dias > AvisarPagoConDias) continue;

            var valor = o.Valor is decimal v ? AsistenteInventario.Pesos(v) : "sin valor";
            var detalle = dias < 0 ? $"La orden {o.Codigo} ({valor}) venció hace {-dias} día(s)."
                        : dias == 0 ? $"La orden {o.Codigo} ({valor}) vence hoy."
                        : $"La orden {o.Codigo} ({valor}) vence en {dias} día(s), el {vence:dd/MM}.";
            lista.Add(new Recordatorio("Pago", dias <= 2 ? "Alta" : "Media",
                (dias < 0 ? "Pago vencido a " : "Pagar a ") + o.Proveedor, detalle));
        }

        // 2. Órdenes que esperan la aprobación del dueño.
        foreach (var o in ordenes.Where(o => o.Estado != "Aprobada"))
        {
            var hace = o.FechaCreacion is DateTimeOffset f ? (int)(ahora - f).TotalDays : 0;
            lista.Add(new Recordatorio("Aprobación", hace >= 2 ? "Alta" : "Media",
                $"Aprobar la orden {o.Codigo}",
                $"{o.Cantidad:0.##} de {o.NombreProducto} a {o.Proveedor}, creada " + (hace > 0 ? $"hace {hace} día(s)." : "hoy.")));
        }

        // 3. Productos que hay que pedir (la reposición ya excluye los que tienen orden reciente).
        foreach (var s in reposicion.Sugerencias)
        {
            lista.Add(new Recordatorio("Pedido", s.Prioridad, $"Pedir {s.Nombre}",
                $"{AsistenteInventario.Cantidad(s.CantidadSugerida, s.Unidad)} a {s.Proveedor}. {s.Motivo}"));
        }

        // Primero lo urgente; dentro de cada prioridad: pagos, aprobaciones y pedidos.
        string[] orden = ["Pago", "Aprobación", "Pedido"];
        return lista
            .OrderBy(r => r.Prioridad == "Alta" ? 0 : 1)
            .ThenBy(r => Array.IndexOf(orden, r.Tipo))
            .ToList();
    }

    // Productos con una orden de compra creada hace poco: ya se pidieron.
    public static HashSet<string> SkusYaPedidos(IReadOnlyList<OrdenDto> ordenes, DateTimeOffset ahora) =>
        ordenes
            .Where(o => o.FechaCreacion is null || (ahora - o.FechaCreacion.Value).TotalDays <= DiasOrdenReciente)
            .Select(o => o.Sku)
            .ToHashSet();

    // Encabezado: "Tienes 3 pendiente(s), 1 urgente(s)."
    public static string Encabezado(List<Recordatorio> lista)
    {
        if (lista.Count == 0) return "No tienes pendientes por ahora: no hay pagos por vencer, órdenes por aprobar ni productos por pedir.";
        var urgentes = lista.Count(r => r.Prioridad == "Alta");
        return $"Tienes {lista.Count} pendiente(s)" + (urgentes > 0 ? $", {urgentes} urgente(s)." : ".");
    }

    // Encabezado más la lista, para responder preguntas sin IA.
    public static string Resumen(List<Recordatorio> lista) =>
        Encabezado(lista) + string.Concat(lista.Take(8).Select(r => $"\n- {r.Titulo}: {r.Detalle}"));
}
