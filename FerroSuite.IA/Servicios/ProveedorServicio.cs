using System.Text.RegularExpressions;
using FerroSuite.IA.Datos;
using FerroSuite.IA.Modelos;

namespace FerroSuite.IA.Servicios;

/// <summary>Lógica de negocio del módulo de Proveedores.</summary>
public class ProveedorServicio(IAlmacenDatos almacen)
{
    public List<Proveedor> Listar() => almacen.ListarProveedores();

    public void Crear(Proveedor d)
    {
        d.Nit = (d.Nit ?? "").Trim();
        d.Nombre = (d.Nombre ?? "").Trim();
        d.Telefono = string.IsNullOrWhiteSpace(d.Telefono) ? null : d.Telefono.Trim();

        var errores = new List<string>();
        if (d.Nombre.Length == 0) errores.Add("El nombre del proveedor es obligatorio.");
        if (d.Nit.Length == 0) errores.Add("El NIT es obligatorio.");
        else if (!Regex.IsMatch(d.Nit, @"^\d{6,12}(-\d)?$")) errores.Add("El NIT debe tener solo números y, si aplica, el dígito de verificación (ej.: 900123456-7).");
        else if (almacen.BuscarProveedor(d.Nit) is not null) errores.Add("Ya existe un proveedor con ese NIT.");
        if (d.DiasEntrega < 0 || d.DiasEntrega > 90) errores.Add("Los días de entrega deben estar entre 0 y 90.");
        if (errores.Count > 0) throw new ErrorValidacion(errores);

        almacen.CrearProveedor(d);
    }
}
