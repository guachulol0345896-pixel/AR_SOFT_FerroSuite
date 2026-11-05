using FerroSuite.IA.Datos;
using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Servicios;

/// <summary>
/// Lógica de negocio del módulo de Inventario: valida lo que llega del frontend,
/// aplica las reglas y usa la base de datos para consultar o guardar.
/// </summary>
public class InventarioServicio(IAlmacenDatos almacen, CalculadoraReposicion calculadora)
{
    public static readonly string[] Unidades = ["Unidad", "Metro", "Kilogramo", "Varilla", "Bulto", "Galón"];
    public static readonly string[] UnidadesConDecimales = ["Metro", "Kilogramo"];
    public static readonly string[] Categorias = ["Construcción", "Plomería", "Eléctricos", "Tornillería", "Pinturas", "Herramientas"];

    // Motivo de movimiento y el tipo con el que tiene sentido ("Ambos" = entrada o salida).
    public static readonly Dictionary<string, string> Motivos = new()
    {
        ["Compra a proveedor"] = "Entrada",
        ["Venta"] = "Salida",
        ["Devolución"] = "Entrada",
        ["Ajuste por conteo"] = "Ambos",
        ["Daño o pérdida"] = "Salida",
    };

    // ---------------- Consultas ----------------

    /// <summary>Productos con consumo diario, días que alcanza el stock y estado.</summary>
    public List<Producto> ListarProductos()
    {
        var productos = almacen.ListarProductos();
        var analisis = calculadora.Analizar(DatosParaAsistente(productos), DateTimeOffset.Now)
            .ToDictionary(a => a.Sku);
        foreach (var p in productos)
        {
            var a = analisis[p.Sku];
            p.ConsumoDiario = a.ConsumoDiario;
            p.DiasRestantes = a.DiasRestantes;
            p.Estado = a.Estado;
        }
        return productos;
    }

    public List<Movimiento> ListarMovimientos(int limite) =>
        almacen.ListarMovimientos(3650).Take(Math.Clamp(limite, 1, 500)).ToList();

    public object Resumen()
    {
        var productos = ListarProductos();
        var ordenes = almacen.ListarOrdenes();
        return new
        {
            Productos = productos.Count,
            EnRiesgo = productos.Count(p => p.Estado != "Normal"),
            Agotados = productos.Count(p => p.StockActual <= 0),
            ValorBodega = productos.Sum(p => (p.PrecioCompra ?? 0) * p.StockActual),
            Proveedores = almacen.ListarProveedores().Count,
            OrdenesPendientes = ordenes.Count(o => o.Estado != "Aprobada"),
            PorPagar = ordenes.Where(o => o.Estado == "Aprobada" && !o.Pagada).Sum(o => o.Valor ?? 0),
        };
    }

    // ---------------- Registro, actualización y eliminación ----------------

    public void CrearProducto(Producto d)
    {
        Limpiar(d);
        var errores = Validar(d);
        if (almacen.BuscarProducto(d.Sku) is not null) errores.Add("Ya existe un producto con ese SKU.");
        if (errores.Count > 0) throw new ErrorValidacion(errores);
        almacen.CrearProducto(d);
    }

    // RF04: el stock no se edita aquí; solo cambia con movimientos, para que quede registro.
    public void ActualizarProducto(Producto d)
    {
        Limpiar(d);
        var actual = almacen.BuscarProducto(d.Sku) ?? throw new ErrorValidacion(["El producto ya no existe."]);
        d.StockActual = actual.StockActual;
        var errores = Validar(d);
        if (errores.Count > 0) throw new ErrorValidacion(errores);
        almacen.ActualizarProducto(d);
    }

    public void EliminarProducto(string sku)
    {
        if (almacen.BuscarProducto(sku) is null) throw new ErrorValidacion(["El producto ya no existe."]);
        if (almacen.ListarOrdenes().Any(o => o.Sku == sku))
            throw new ErrorValidacion(["No se puede eliminar: el producto tiene órdenes de compra registradas."]);
        almacen.EliminarProducto(sku);
    }

    // RF05: registra una entrada o salida y actualiza el stock.
    public decimal RegistrarMovimiento(Movimiento m)
    {
        var errores = new List<string>();
        var producto = almacen.BuscarProducto(m.Sku ?? "");
        if (producto is null) errores.Add("Elige un producto.");
        if (m.Tipo != "Entrada" && m.Tipo != "Salida") errores.Add("El tipo debe ser Entrada o Salida.");
        if (!Motivos.TryGetValue(m.Motivo ?? "", out var tipoMotivo)) errores.Add("Elige un motivo válido.");
        else if (tipoMotivo != "Ambos" && tipoMotivo != m.Tipo)
            errores.Add($"El motivo \"{m.Motivo}\" corresponde a una {tipoMotivo.ToLower()}, no a una {m.Tipo?.ToLower()}.");
        if (m.Cantidad <= 0) errores.Add("La cantidad debe ser mayor que 0.");
        if (producto is not null && !UnidadesConDecimales.Contains(producto.Unidad) && m.Cantidad != Math.Floor(m.Cantidad))
            errores.Add($"La cantidad debe ser un número entero para {producto.Unidad.ToLower()}.");
        if (producto is not null && m.Tipo == "Salida" && m.Cantidad > producto.StockActual)
            errores.Add($"No hay stock suficiente (disponible: {producto.StockActual:0.##}).");
        if (errores.Count > 0) throw new ErrorValidacion(errores);

        var nuevoStock = m.Tipo == "Entrada" ? producto!.StockActual + m.Cantidad : producto!.StockActual - m.Cantidad;
        m.Usuario = string.IsNullOrWhiteSpace(m.Usuario) ? "Sin sesión" : m.Usuario.Trim();
        almacen.RegistrarMovimiento(m, nuevoStock);
        return nuevoStock;
    }

    // ---------------- Validaciones (RF12) ----------------

    private static void Limpiar(Producto d)
    {
        d.Sku = (d.Sku ?? "").Trim().ToUpperInvariant();
        d.Nombre = (d.Nombre ?? "").Trim();
        d.Categoria = (d.Categoria ?? "").Trim();
        d.NitProveedor = string.IsNullOrWhiteSpace(d.NitProveedor) ? null : d.NitProveedor.Trim();
    }

    private List<string> Validar(Producto d)
    {
        var errores = new List<string>();
        if (d.Sku.Length == 0) errores.Add("El SKU es obligatorio.");
        else if (d.Sku.Length > 20) errores.Add("El SKU no puede tener más de 20 caracteres.");
        if (d.Nombre.Length == 0) errores.Add("El nombre del producto es obligatorio.");
        else if (d.Nombre.Length > 120) errores.Add("El nombre no puede tener más de 120 caracteres.");
        if (!Categorias.Contains(d.Categoria)) errores.Add("Elige una categoría válida.");
        if (!Unidades.Contains(d.Unidad)) errores.Add("Elige una unidad de medida válida.");
        if (d.StockActual < 0) errores.Add("El stock actual no puede ser negativo.");
        if (d.StockMinimo < 0) errores.Add("El stock mínimo no puede ser negativo.");
        if (Unidades.Contains(d.Unidad) && !UnidadesConDecimales.Contains(d.Unidad) &&
            (d.StockActual != Math.Floor(d.StockActual) || d.StockMinimo != Math.Floor(d.StockMinimo)))
            errores.Add($"Las cantidades deben ser números enteros para {d.Unidad.ToLower()}.");
        if (d.PrecioCompra < 0) errores.Add("El precio de compra no puede ser negativo.");
        if (d.NitProveedor is not null && almacen.BuscarProveedor(d.NitProveedor) is null)
            errores.Add("El proveedor elegido no existe.");
        return errores;
    }

    // ---------------- Datos para el asistente de IA ----------------

    /// <summary>Arma lo que necesita el asistente: productos, ventas de 60 días y órdenes.</summary>
    public SolicitudInventario DatosParaAsistente(List<Producto>? productos = null)
    {
        productos ??= almacen.ListarProductos();
        var proveedores = almacen.ListarProveedores().ToDictionary(p => p.Nit);

        var productosDto = productos.Select(p => new ProductoDto(
            p.Sku, p.Nombre, p.Categoria, p.Unidad, p.StockActual, p.StockMinimo, p.PrecioCompra,
            p.NitProveedor, p.NombreProveedor, p.DiasEntregaProveedor)).ToList();

        var movimientosDto = almacen.ListarMovimientos(60)
            .Select(m => new MovimientoDto(m.Sku, m.Tipo, m.Cantidad, new DateTimeOffset(m.Fecha))).ToList();

        var ordenesDto = almacen.ListarOrdenes().Select(o => new OrdenDto(
            o.Codigo, o.Sku, o.NombreProducto, o.NombreProveedor, o.Cantidad, o.Estado, o.Valor,
            new DateTimeOffset(o.FechaCreacion),
            o.FechaAprobacion is DateTime f ? new DateTimeOffset(f) : null,
            o.Pagada)).ToList();

        return new SolicitudInventario(productosDto, movimientosDto, ordenesDto);
    }
}
