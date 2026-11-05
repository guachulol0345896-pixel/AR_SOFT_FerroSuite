using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Servicios;

/// <summary>
/// Hace las cuentas del inventario con reglas fijas: consumo diario, días que
/// alcanza el stock y cuánto pedir. La IA no calcula cifras; solo las explica.
/// </summary>
public sealed class CalculadoraReposicion
{
    public const int DiasConsumo = 30;      // ventana para calcular cuánto se vende por día
    public const int DiasCobertura = 15;    // el pedido sugerido alcanza para ~15 días más
    public const int MargenDias = 3;        // avisar si se acaba antes de entrega + 3 días
    private const int DiasEntregaPorDefecto = 3;

    public IReadOnlyList<AnalisisProducto> Analizar(SolicitudInventario solicitud, DateTimeOffset ahora)
    {
        var productos = solicitud.Productos ?? [];
        var desde = ahora.AddDays(-DiasConsumo);

        var vendido = (solicitud.Movimientos ?? [])
            .Where(m => m.Tipo == "Salida" && m.Fecha >= desde && m.Fecha <= ahora)
            .GroupBy(m => m.Sku)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Cantidad));

        return productos.Select(p =>
        {
            var vendido30 = vendido.GetValueOrDefault(p.Sku);
            var consumo = Math.Round(vendido30 / DiasConsumo, 2);
            int? dias = consumo > 0 ? (int)Math.Floor(p.StockActual / consumo) : null;
            var estado = p.StockActual <= 0 ? "Agotado" : p.StockActual <= p.StockMinimo ? "Stock bajo" : "Normal";

            return new AnalisisProducto(
                p.Sku, p.Nombre, p.Categoria, p.Unidad, p.StockActual, p.StockMinimo,
                consumo, vendido30, dias,
                p.NombreProveedor ?? "Sin proveedor",
                p.DiasEntregaProveedor ?? DiasEntregaPorDefecto,
                p.PrecioCompra, estado);
        }).ToList();
    }

    // yaPedidos: productos que ya tienen una orden de compra reciente (no se vuelven a sugerir).
    public ResultadoReposicion Calcular(IReadOnlyList<AnalisisProducto> analisis, IReadOnlyList<ProductoDto> productos, ISet<string>? yaPedidos = null)
    {
        var nits = productos.ToDictionary(p => p.Sku, p => p.NitProveedor);

        var sugerencias = analisis
            .Where(a => NecesitaReponer(a) && (yaPedidos is null || !yaPedidos.Contains(a.Sku)))
            .Select(a =>
            {
                var cantidad = CantidadAPedir(a);
                var prioridad = a.StockActual <= 0 || (a.DiasRestantes is int d && d <= a.DiasEntrega) ? "Alta" : "Media";
                return new SugerenciaReposicion(
                    a.Sku, a.Nombre, a.Unidad, a.StockActual, a.DiasRestantes, cantidad,
                    a.Proveedor, nits.GetValueOrDefault(a.Sku), a.DiasEntrega,
                    a.PrecioCompra is decimal precio ? Math.Round(precio * cantidad, 0) : null,
                    prioridad, Motivo(a));
            })
            .OrderBy(s => s.Prioridad == "Alta" ? 0 : 1)
            .ThenBy(s => s.DiasRestantes ?? int.MaxValue)
            .ToList();

        var costos = sugerencias
            .GroupBy(s => s.Proveedor)
            .Select(g => new CostoPorProveedor(g.Key, g.Count(), g.Sum(s => s.CostoEstimado ?? 0)))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new ResultadoReposicion(sugerencias, costos, costos.Sum(c => c.Total));
    }

    // Hay que pedir si ya está en el mínimo o si se acaba antes de que llegue un pedido.
    private static bool NecesitaReponer(AnalisisProducto a) =>
        a.StockActual <= a.StockMinimo ||
        (a.DiasRestantes is int d && d <= a.DiasEntrega + MargenDias);

    // Lo que se vende mientras llega el pedido y durante la cobertura, más el mínimo,
    // menos lo que ya hay. Nunca menos de lo necesario para volver al doble del mínimo.
    private static decimal CantidadAPedir(AnalisisProducto a)
    {
        var porConsumo = a.ConsumoDiario * (a.DiasEntrega + DiasCobertura) + a.StockMinimo - a.StockActual;
        var porMinimo = a.StockMinimo * 2 - a.StockActual;
        var cantidad = Math.Max(porConsumo, porMinimo);
        return cantidad <= 0 ? Math.Max(a.StockMinimo, 1) : Math.Ceiling(cantidad);
    }

    private static string Motivo(AnalisisProducto a)
    {
        if (a.StockActual <= 0) return "Está agotado.";
        if (a.DiasRestantes is int d && d <= a.DiasEntrega)
            return $"Se acaba en {d} día(s) y el proveedor tarda {a.DiasEntrega} en entregar.";
        if (a.StockActual <= a.StockMinimo) return "Está en el stock mínimo o por debajo.";
        return $"Se acaba en {a.DiasRestantes} día(s), muy cerca del tiempo de entrega.";
    }
}
