# Organización de la arquitectura e integración

Este documento explica cómo está organizado el código de FerroSuite y cómo se relaciona con la arquitectura en capas del documento maestro (sección 8): Presentación, Aplicación, Dominio e Infraestructura.

## Tipo de aplicación

FerroSuite corre como un **aplicativo de escritorio para Windows**. La ventana se hizo con Windows Forms (.NET 10) y muestra las pantallas con WebView2, el navegador integrado de Windows. No abre puertos ni usa `localhost`: el frontend y el backend viven en el mismo programa y se comunican por mensajes.

Se eligió así porque la ferretería trabaja en un solo computador de mostrador y el equipo ya usa C# y Visual Studio. Además, las pantallas HTML del prototipo se pueden reutilizar sin rehacerlas. La organización en capas es la misma del documento maestro, así que más adelante se puede pasar a una aplicación web con ASP.NET Core cambiando solo la capa de comunicación (los mensajes por una API REST).

## Capas y carpetas

```text
FerroSuite-erp/
├── frontend/                     FRONTEND (capa de Presentación)
│   ├── *.html                    Pantallas: Inicio, Inventario, Proveedores, Órdenes, Ingreso
│   ├── css/                      Estilos
│   └── js/
│       ├── components/           Componentes visuales reutilizables (tabla, formulario, avisos…)
│       ├── controllers/          Un controlador por pantalla: muestra datos y envía solicitudes
│       ├── services/             Validación básica y llamadas al backend por módulo
│       ├── comunicacion/api.js   COMUNICACIÓN (lado del frontend)
│       └── models/data.js        Listas fijas de los formularios (unidades, categorías, motivos)
│
└── FerroSuite.IA/                BACKEND (aplicativo C#)
    ├── VentanaPrincipal.cs       Ventana de Windows que muestra el frontend (WebView2)
    ├── Comunicacion/Puente.cs    COMUNICACIÓN (lado del backend): recibe solicitudes y responde
    ├── Servicios/                Capa de Aplicación: lógica de negocio y validaciones
    │   ├── InventarioServicio.cs, ProveedorServicio.cs, OrdenServicio.cs
    │   └── CalculadoraReposicion.cs, GeneradorRecordatorios.cs, AsistenteInventario.cs, ClienteGemini.cs (IA)
    ├── Modelos/                  Capa de Dominio: entidades (Producto, Proveedor, Movimiento, OrdenCompra)
    ├── Datos/                    Capa de Infraestructura: acceso a SQL Server
    │   ├── IAlmacenDatos.cs      Operaciones de datos que usan los servicios
    │   ├── AlmacenSqlServer.cs   Consultas SQL con parámetros
    │   └── BaseDatos.cs          Conexión y creación de la base
    └── BaseDatos/                BASE DE DATOS
        ├── crear_base_datos.sql  Script de creación (tablas, claves, relaciones, restricciones)
        └── datos_prueba.sql      Datos de prueba
```

| Responsabilidad | Dónde está | Capa del documento maestro |
|---|---|---|
| Frontend | `frontend/` | Presentación |
| Comunicación | `frontend/js/comunicacion/api.js` ↔ `FerroSuite.IA/Comunicacion/Puente.cs` | Entre Presentación y Aplicación |
| Backend | `FerroSuite.IA/Servicios/` | Aplicación |
| Entidades | `FerroSuite.IA/Modelos/` | Dominio |
| Base de datos | `FerroSuite.IA/Datos/` y `FerroSuite.IA/BaseDatos/` (SQL Server LocalDB) | Infraestructura |

Cada capa solo habla con la siguiente. Las pantallas no tocan la base de datos, y los servicios no saben cómo se dibuja la pantalla ni qué motor de base de datos hay detrás, porque usan `IAlmacenDatos`. Así se cumple el requerimiento de mantenibilidad (RNF06): se puede cambiar una capa sin rehacer las otras.

## Comunicación entre frontend y backend

Cumple el papel de una API REST, pero por el canal de mensajes de WebView2:

| Paso | Qué pasa | Código |
|---|---|---|
| 1 | El usuario llena un formulario y pulsa el botón. | `components/formulario.js` |
| 2 | El frontend valida lo básico y envía `{ id, accion: "productos.crear", datos }`. | `services/inventarioService.js` → `comunicacion/api.js` |
| 3 | El backend recibe el mensaje, lo dirige al servicio y valida todo. | `Comunicacion/Puente.cs` → `Servicios/InventarioServicio.cs` |
| 4 | El backend guarda o consulta en SQL Server. | `Datos/AlmacenSqlServer.cs` |
| 5 | El backend responde `{ id, ok, mensaje, datos, errores }`. | `Comunicacion/Puente.cs` |
| 6 | El frontend muestra el mensaje de confirmación o los errores y actualiza la tabla. | `components/aviso.js`, `controllers/inventario.js` |

Acciones disponibles (equivalen a las rutas de una API):

| Módulo | Acciones |
|---|---|
| Inventario | `productos.listar`, `productos.crear`, `productos.actualizar`, `productos.eliminar`, `movimientos.listar`, `movimientos.registrar`, `resumen`, `datos.restablecer` |
| Proveedores | `proveedores.listar`, `proveedores.crear` |
| Órdenes de compra | `ordenes.listar`, `ordenes.crear`, `ordenes.aprobar`, `ordenes.pagar` |
| Asistente de IA | `ia.estado`, `ia.reposicion`, `ia.recordatorios`, `ia.preguntar` |

Manejo de errores:

- Datos inválidos: el backend responde `ok: false` con la lista de errores (`ErrorValidacion`).
- Falla de SQL Server: se responde con un mensaje claro en vez de cerrar el programa (`SqlException`).
- Sin respuesta del backend en 10 segundos: el frontend avisa "No hay comunicación con el backend".

## Base de datos

- **Motor:** SQL Server Express LocalDB, coherente con el documento maestro (SQL Server Express). Se instala con Visual Studio, en la carga de trabajo "Desarrollo de ASP.NET y web".
- **Creación:** al abrir el programa se ejecuta `crear_base_datos.sql`, que crea la base `FerroSuite` y las tablas si no existen. Luego se ejecuta `datos_prueba.sql` si la base está vacía.

| Tabla | Clave primaria | Relaciones | Tipos y restricciones principales |
|---|---|---|---|
| `Proveedor` | `Nit` (VARCHAR) | — | `DiasEntrega` INT ≥ 0; `Cumplimiento` DECIMAL(5,2) entre 0 y 100 |
| `Producto` | `Sku` (VARCHAR) | `NitProveedor` → `Proveedor.Nit` | `Unidad` solo valores permitidos; `StockActual`/`StockMinimo` DECIMAL(12,2) ≥ 0; `PrecioCompra` DECIMAL(12,2) |
| `MovimientoInventario` | `MovimientoId` (INT IDENTITY) | `Sku` → `Producto.Sku` | `Tipo` Entrada/Salida; `Cantidad` > 0; `Fecha` DATETIME2 |
| `OrdenCompra` | `OrdenId` (INT IDENTITY) | `Sku` → `Producto`; `NitProveedor` → `Proveedor` | `Codigo` calculado (OC-001); `Estado`; `Pagada` BIT |

Registrar un movimiento guarda el movimiento y actualiza el stock **en una sola transacción**: si una de las dos operaciones falla, no queda ninguna a medias.
