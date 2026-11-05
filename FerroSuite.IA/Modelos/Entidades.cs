namespace FerroSuite.IA.Modelos;

// Entidades del negocio. Cada una corresponde a una tabla de la base de datos.

public class Proveedor
{
    public string Nit { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
    public int DiasEntrega { get; set; }
    public decimal? Cumplimiento { get; set; }
}

public class Producto
{
    public string Sku { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal? PrecioCompra { get; set; }
    public string? NitProveedor { get; set; }

    // Datos calculados por el backend (no se guardan en la tabla).
    public string? NombreProveedor { get; set; }
    public int? DiasEntregaProveedor { get; set; }
    public decimal ConsumoDiario { get; set; }
    public int? DiasRestantes { get; set; }
    public string Estado { get; set; } = "";
}

public class Movimiento
{
    public int MovimientoId { get; set; }
    public string Sku { get; set; } = "";
    public string? NombreProducto { get; set; }
    public string? Unidad { get; set; }
    public string Tipo { get; set; } = "";
    public decimal Cantidad { get; set; }
    public string Motivo { get; set; } = "";
    public string Usuario { get; set; } = "";
    public DateTime Fecha { get; set; }
}

public class OrdenCompra
{
    public int OrdenId { get; set; }
    public string Codigo { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? NombreProducto { get; set; }
    public string NitProveedor { get; set; } = "";
    public string? NombreProveedor { get; set; }
    public decimal Cantidad { get; set; }
    public string Estado { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public bool Pagada { get; set; }

    // Calculados por el backend.
    public decimal? Valor { get; set; }
    public DateTime? VencePago { get; set; }
}

/// <summary>Error de datos ingresados: lleva la lista de mensajes para mostrar al usuario.</summary>
public class ErrorValidacion(List<string> errores) : Exception(string.Join(" ", errores))
{
    public List<string> Errores { get; } = errores;
}
