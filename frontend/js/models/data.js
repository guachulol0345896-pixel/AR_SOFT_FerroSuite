// FerroSuite — Listas fijas que usan los formularios.
// Los datos (productos, proveedores, movimientos y órdenes) ya no viven aquí:
// están en la base de datos SQL Server y se piden al backend.

const UNIDADES = ["Unidad", "Metro", "Kilogramo", "Varilla", "Bulto", "Galón"];
const UNIDADES_CON_DECIMALES = ["Metro", "Kilogramo"]; // 2,5 metros sí; media varilla no
const CATEGORIAS = ["Construcción", "Plomería", "Eléctricos", "Tornillería", "Pinturas", "Herramientas"];
const MOTIVOS = ["Compra a proveedor", "Venta", "Devolución", "Ajuste por conteo", "Daño o pérdida"];
