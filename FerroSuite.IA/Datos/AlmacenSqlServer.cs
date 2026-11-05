using FerroSuite.IA.Modelos;
using Microsoft.Data.SqlClient;

namespace FerroSuite.IA.Datos;

/// <summary>
/// Consultas y cambios en SQL Server. Todas las consultas usan parámetros
/// (@sku, @nombre...) para evitar errores e inyección de SQL.
/// </summary>
public class AlmacenSqlServer : IAlmacenDatos
{
    // ---------------- Productos ----------------

    private const string ConsultaProductos = """
        SELECT p.Sku, p.Nombre, p.Categoria, p.Unidad, p.StockActual, p.StockMinimo,
               p.PrecioCompra, p.NitProveedor, pr.Nombre AS NombreProveedor, pr.DiasEntrega
        FROM dbo.Producto p
        LEFT JOIN dbo.Proveedor pr ON pr.Nit = p.NitProveedor
        """;

    public List<Producto> ListarProductos()
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand(ConsultaProductos + " ORDER BY p.Nombre", conexion);
        using var lector = comando.ExecuteReader();
        var lista = new List<Producto>();
        while (lector.Read()) lista.Add(LeerProducto(lector));
        return lista;
    }

    public Producto? BuscarProducto(string sku)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand(ConsultaProductos + " WHERE p.Sku = @sku", conexion);
        comando.Parameters.AddWithValue("@sku", sku);
        using var lector = comando.ExecuteReader();
        return lector.Read() ? LeerProducto(lector) : null;
    }

    public void CrearProducto(Producto p)
    {
        Ejecutar("""
            INSERT INTO dbo.Producto (Sku, Nombre, Categoria, Unidad, StockActual, StockMinimo, PrecioCompra, NitProveedor)
            VALUES (@sku, @nombre, @categoria, @unidad, @stockActual, @stockMinimo, @precio, @nit)
            """, ParametrosProducto(p));
    }

    public void ActualizarProducto(Producto p)
    {
        Ejecutar("""
            UPDATE dbo.Producto
            SET Nombre = @nombre, Categoria = @categoria, Unidad = @unidad, StockMinimo = @stockMinimo,
                PrecioCompra = @precio, NitProveedor = @nit
            WHERE Sku = @sku
            """, ParametrosProducto(p));
    }

    public void EliminarProducto(string sku)
    {
        Ejecutar("DELETE FROM dbo.Producto WHERE Sku = @sku", ("@sku", sku));
    }

    // ---------------- Proveedores ----------------

    public List<Proveedor> ListarProveedores()
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand("SELECT Nit, Nombre, Telefono, DiasEntrega, Cumplimiento FROM dbo.Proveedor ORDER BY Nombre", conexion);
        using var lector = comando.ExecuteReader();
        var lista = new List<Proveedor>();
        while (lector.Read()) lista.Add(LeerProveedor(lector));
        return lista;
    }

    public Proveedor? BuscarProveedor(string nit)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand("SELECT Nit, Nombre, Telefono, DiasEntrega, Cumplimiento FROM dbo.Proveedor WHERE Nit = @nit", conexion);
        comando.Parameters.AddWithValue("@nit", nit);
        using var lector = comando.ExecuteReader();
        return lector.Read() ? LeerProveedor(lector) : null;
    }

    public void CrearProveedor(Proveedor p)
    {
        Ejecutar("""
            INSERT INTO dbo.Proveedor (Nit, Nombre, Telefono, DiasEntrega, Cumplimiento)
            VALUES (@nit, @nombre, @telefono, @dias, NULL)
            """, ("@nit", p.Nit), ("@nombre", p.Nombre), ("@telefono", p.Telefono), ("@dias", p.DiasEntrega));
    }

    // ---------------- Movimientos ----------------

    public List<Movimiento> ListarMovimientos(int ultimosDias)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand("""
            SELECT m.MovimientoId, m.Sku, p.Nombre, p.Unidad, m.Tipo, m.Cantidad, m.Motivo, m.Usuario, m.Fecha
            FROM dbo.MovimientoInventario m
            JOIN dbo.Producto p ON p.Sku = m.Sku
            WHERE m.Fecha >= DATEADD(DAY, -@dias, SYSDATETIME())
            ORDER BY m.Fecha DESC, m.MovimientoId DESC
            """, conexion);
        comando.Parameters.AddWithValue("@dias", ultimosDias);
        using var lector = comando.ExecuteReader();
        var lista = new List<Movimiento>();
        while (lector.Read())
        {
            lista.Add(new Movimiento
            {
                MovimientoId = lector.GetInt32(0),
                Sku = lector.GetString(1),
                NombreProducto = lector.GetString(2),
                Unidad = lector.GetString(3),
                Tipo = lector.GetString(4),
                Cantidad = lector.GetDecimal(5),
                Motivo = lector.GetString(6),
                Usuario = lector.GetString(7),
                Fecha = lector.GetDateTime(8),
            });
        }
        return lista;
    }

    public void RegistrarMovimiento(Movimiento m, decimal nuevoStock)
    {
        // Las dos operaciones van juntas: si una falla, ninguna queda guardada.
        using var conexion = BaseDatos.Conectar();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var insertar = new SqlCommand("""
                INSERT INTO dbo.MovimientoInventario (Sku, Tipo, Cantidad, Motivo, Usuario)
                VALUES (@sku, @tipo, @cantidad, @motivo, @usuario)
                """, conexion, transaccion))
            {
                insertar.Parameters.AddWithValue("@sku", m.Sku);
                insertar.Parameters.AddWithValue("@tipo", m.Tipo);
                insertar.Parameters.AddWithValue("@cantidad", m.Cantidad);
                insertar.Parameters.AddWithValue("@motivo", m.Motivo);
                insertar.Parameters.AddWithValue("@usuario", m.Usuario);
                insertar.ExecuteNonQuery();
            }
            using (var actualizar = new SqlCommand("UPDATE dbo.Producto SET StockActual = @stock WHERE Sku = @sku", conexion, transaccion))
            {
                actualizar.Parameters.AddWithValue("@stock", nuevoStock);
                actualizar.Parameters.AddWithValue("@sku", m.Sku);
                actualizar.ExecuteNonQuery();
            }
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    // ---------------- Órdenes de compra ----------------

    private const string ConsultaOrdenes = """
        SELECT o.OrdenId, o.Codigo, o.Sku, p.Nombre, o.NitProveedor, pr.Nombre, o.Cantidad, o.Estado,
               o.FechaCreacion, o.FechaAprobacion, o.Pagada, p.PrecioCompra
        FROM dbo.OrdenCompra o
        JOIN dbo.Producto p ON p.Sku = o.Sku
        JOIN dbo.Proveedor pr ON pr.Nit = o.NitProveedor
        """;

    public List<OrdenCompra> ListarOrdenes()
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand(ConsultaOrdenes + " ORDER BY o.OrdenId", conexion);
        using var lector = comando.ExecuteReader();
        var lista = new List<OrdenCompra>();
        while (lector.Read()) lista.Add(LeerOrden(lector));
        return lista;
    }

    public OrdenCompra? BuscarOrden(string codigo)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand(ConsultaOrdenes + " WHERE o.Codigo = @codigo", conexion);
        comando.Parameters.AddWithValue("@codigo", codigo);
        using var lector = comando.ExecuteReader();
        return lector.Read() ? LeerOrden(lector) : null;
    }

    public string CrearOrden(OrdenCompra o)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand("""
            INSERT INTO dbo.OrdenCompra (Sku, NitProveedor, Cantidad)
            OUTPUT INSERTED.Codigo
            VALUES (@sku, @nit, @cantidad)
            """, conexion);
        comando.Parameters.AddWithValue("@sku", o.Sku);
        comando.Parameters.AddWithValue("@nit", o.NitProveedor);
        comando.Parameters.AddWithValue("@cantidad", o.Cantidad);
        return (string)comando.ExecuteScalar()!;
    }

    public void AprobarOrden(string codigo)
    {
        Ejecutar("UPDATE dbo.OrdenCompra SET Estado = N'Aprobada', FechaAprobacion = SYSDATETIME() WHERE Codigo = @codigo", ("@codigo", codigo));
    }

    public void MarcarOrdenPagada(string codigo)
    {
        Ejecutar("UPDATE dbo.OrdenCompra SET Pagada = 1 WHERE Codigo = @codigo", ("@codigo", codigo));
    }

    // ---------------- Ayudas ----------------

    // Ejecuta un INSERT, UPDATE o DELETE. Los valores null se guardan como NULL.
    private static void Ejecutar(string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using var conexion = BaseDatos.Conectar();
        using var comando = new SqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros)
            comando.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        comando.ExecuteNonQuery();
    }

    private static (string, object?)[] ParametrosProducto(Producto p) =>
    [
        ("@sku", p.Sku), ("@nombre", p.Nombre), ("@categoria", p.Categoria), ("@unidad", p.Unidad),
        ("@stockActual", p.StockActual), ("@stockMinimo", p.StockMinimo),
        ("@precio", p.PrecioCompra), ("@nit", string.IsNullOrEmpty(p.NitProveedor) ? null : p.NitProveedor),
    ];

    private static Producto LeerProducto(SqlDataReader r) => new()
    {
        Sku = r.GetString(0),
        Nombre = r.GetString(1),
        Categoria = r.GetString(2),
        Unidad = r.GetString(3),
        StockActual = r.GetDecimal(4),
        StockMinimo = r.GetDecimal(5),
        PrecioCompra = r.IsDBNull(6) ? null : r.GetDecimal(6),
        NitProveedor = r.IsDBNull(7) ? null : r.GetString(7),
        NombreProveedor = r.IsDBNull(8) ? null : r.GetString(8),
        DiasEntregaProveedor = r.IsDBNull(9) ? null : r.GetInt32(9),
    };

    private static Proveedor LeerProveedor(SqlDataReader r) => new()
    {
        Nit = r.GetString(0),
        Nombre = r.GetString(1),
        Telefono = r.IsDBNull(2) ? null : r.GetString(2),
        DiasEntrega = r.GetInt32(3),
        Cumplimiento = r.IsDBNull(4) ? null : r.GetDecimal(4),
    };

    private static OrdenCompra LeerOrden(SqlDataReader r) => new()
    {
        OrdenId = r.GetInt32(0),
        Codigo = r.GetString(1),
        Sku = r.GetString(2),
        NombreProducto = r.GetString(3),
        NitProveedor = r.GetString(4),
        NombreProveedor = r.GetString(5),
        Cantidad = r.GetDecimal(6),
        Estado = r.GetString(7),
        FechaCreacion = r.GetDateTime(8),
        FechaAprobacion = r.IsDBNull(9) ? null : r.GetDateTime(9),
        Pagada = r.GetBoolean(10),
        Valor = r.IsDBNull(11) ? null : r.GetDecimal(11) * r.GetDecimal(6),
    };
}
