# Pruebas de integración

Las pruebas recorren el flujo completo: pantalla → comunicación → backend → SQL Server → respuesta → pantalla.

**Cómo ejecutarlas:** abrir `FerroSuite.slnx` en Visual Studio y pulsar F5 con `FerroSuite.IA` como proyecto de inicio. Para comprobar lo guardado en la base, usar **Ver → Explorador de objetos de SQL Server** → `(localdb)\MSSQLLocalDB` → `Bases de datos` → `FerroSuite` → `Tablas`, clic derecho → **Ver datos**.

Llenar las columnas **Resultado obtenido** y **Estado** (Aprobada / Fallida) al ejecutar cada prueba, y guardar una captura como evidencia.

| # | Escenario | Pasos | Resultado esperado | Resultado obtenido | Estado |
|---|---|---|---|---|---|
| 1 | **Registrar información desde la interfaz y comprobar que queda almacenada** | En Inventario → *Agregar producto*: SKU `FER-0999`, nombre `Llave de paso 1/2`, categoría Plomería, unidad Unidad, stock 3, mínimo 5, precio 15000, proveedor Cementos Andinos SAS → *Agregar producto*. Luego revisar la tabla `Producto` en el Explorador de objetos de SQL Server. | Mensaje "Producto guardado en la base de datos.", el producto aparece en el catálogo y en la tabla `Producto` hay una fila con `Sku = FER-0999`. | | |
| 2 | **Consultar información y verificar que se visualiza correctamente** | Abrir Inventario y Órdenes de compra. Comparar con las tablas `Producto`, `MovimientoInventario` y `OrdenCompra`. Registrar una salida de 2,5 metros de *Cable eléctrico THHN 12* y volver a mirar. | El catálogo muestra los mismos productos y cantidades que la base. El historial muestra el movimiento nuevo con fecha, motivo y usuario. El stock del cable baja de 340 a 337,5. | | |
| 3 | **Intentar enviar información inválida y comprobar que el sistema la controla** | a) *Agregar producto* con los campos vacíos. b) SKU `FER-0999` repetido, stock −2, unidad Varilla con mínimo 1,5. c) Movimiento de *Entrada* con motivo *Venta* y cantidad 2,5 de Varilla. d) Proveedor con NIT `abc`. | a) El frontend no envía y pide los campos obligatorios. b) El backend responde: stock negativo, cantidades enteras para varilla y SKU repetido. c) Responde que la venta es una salida y que la varilla va en enteros. d) Responde que el NIT tiene un formato inválido. En ningún caso se guarda nada en la base. | | |
| 4 | **Simular un error de comunicación o conexión y verificar que se informa al usuario** | a) Menú **Opciones → Simular error de comunicación** (activar) y registrar un movimiento. Luego desactivarlo. b) Menú **Opciones → Simular caída de la base de datos** (activar) y pulsar *Recargar pantalla*, o registrar un movimiento. Luego desactivarlo. | a) A los 10 segundos aparece "No hay comunicación con el backend. Intenta de nuevo en unos segundos." y el programa sigue abierto. b) Aparece "Revisa que SQL Server LocalDB esté instalado y vuelve a intentar." y no se guarda nada. Al desactivar la opción, todo vuelve a funcionar. | | |

## Prueba previa del equipo de desarrollo

Antes de la entrega, el flujo de las cuatro pruebas se verificó con el mismo frontend y el mismo código del backend (`Puente`, servicios y validaciones), usando un almacenamiento en memoria en lugar de SQL Server:

- Prueba 1: el registro respondió "Producto guardado en la base de datos." y el producto apareció en el catálogo.
- Prueba 2: el catálogo, el historial y las órdenes se mostraron con los datos recibidos del backend.
- Prueba 3: los mensajes de validación se mostraron tal como se describen arriba.
- Prueba 4: el error de comunicación y el de base de datos se informaron sin cerrar la pantalla.

La ejecución sobre SQL Server LocalDB se hace en Visual Studio con la tabla de arriba.
