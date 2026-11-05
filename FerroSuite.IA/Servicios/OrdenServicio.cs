using FerroSuite.IA.Datos;
using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Servicios;

/// <summary>Lógica de negocio de las órdenes de compra (RF08 y RF09) y su pago.</summary>
public class OrdenServicio(IAlmacenDatos almacen)
{
    public List<OrdenCompra> Listar()
    {
        var ordenes = almacen.ListarOrdenes();
        foreach (var o in ordenes)
        {
            if (o.Estado == "Aprobada" && !o.Pagada && o.FechaAprobacion is DateTime aprobada)
                o.VencePago = aprobada.AddDays(GeneradorRecordatorios.PlazoPagoDias);
        }
        return ordenes;
    }

    public string Crear(OrdenCompra d)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(d.Sku) || almacen.BuscarProducto(d.Sku) is null) errores.Add("Elige un producto que exista.");
        if (string.IsNullOrWhiteSpace(d.NitProveedor) || almacen.BuscarProveedor(d.NitProveedor) is null) errores.Add("Elige un proveedor que exista.");
        if (d.Cantidad <= 0) errores.Add("La cantidad debe ser mayor que 0.");
        if (errores.Count > 0) throw new ErrorValidacion(errores);
        return almacen.CrearOrden(d);
    }

    public void Aprobar(string codigo)
    {
        var orden = almacen.BuscarOrden(codigo) ?? throw new ErrorValidacion([$"La orden {codigo} no existe."]);
        if (orden.Estado == "Aprobada") throw new ErrorValidacion([$"La orden {codigo} ya estaba aprobada."]);
        almacen.AprobarOrden(codigo);
    }

    public void MarcarPagada(string codigo)
    {
        var orden = almacen.BuscarOrden(codigo) ?? throw new ErrorValidacion([$"La orden {codigo} no existe."]);
        if (orden.Estado != "Aprobada") throw new ErrorValidacion(["Primero hay que aprobar la orden."]);
        if (orden.Pagada) throw new ErrorValidacion([$"La orden {codigo} ya estaba pagada."]);
        almacen.MarcarOrdenPagada(codigo);
    }
}
