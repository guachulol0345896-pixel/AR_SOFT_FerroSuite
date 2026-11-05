using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Datos;

/// <summary>
/// Operaciones de la base de datos que usa el backend. La implementación real es
/// AlmacenSqlServer; los servicios no saben qué motor hay detrás.
/// </summary>
public interface IAlmacenDatos
{
    List<Producto> ListarProductos();
    Producto? BuscarProducto(string sku);
    void CrearProducto(Producto p);
    void ActualizarProducto(Producto p);
    void EliminarProducto(string sku);

    List<Proveedor> ListarProveedores();
    Proveedor? BuscarProveedor(string nit);
    void CrearProveedor(Proveedor p);

    List<Movimiento> ListarMovimientos(int ultimosDias);
    /// <summary>Guarda el movimiento y el nuevo stock en una sola transacción.</summary>
    void RegistrarMovimiento(Movimiento m, decimal nuevoStock);

    List<OrdenCompra> ListarOrdenes();
    OrdenCompra? BuscarOrden(string codigo);
    string CrearOrden(OrdenCompra o);
    void AprobarOrden(string codigo);
    void MarcarOrdenPagada(string codigo);
}
