# FerroSuite.IA — Aplicativo de escritorio (backend)

Programa de Windows que muestra las pantallas de la carpeta `frontend` dentro de una ventana (WebView2) y las atiende con un backend en C#. Los datos se guardan en SQL Server LocalDB. No abre puertos ni usa `localhost`.

```text
Pantalla (frontend) ──mensaje──► Comunicacion/Puente ──► Servicios ──► Datos ──► SQL Server LocalDB
                    ◄─respuesta──                   ◄──────────────◄────────◄
```

| Carpeta | Qué contiene |
|---|---|
| `Comunicacion/` | `Puente.cs`: recibe las solicitudes del frontend, llama al servicio y responde con éxito o error. |
| `Servicios/` | Lógica de negocio y validaciones: inventario, proveedores, órdenes y el asistente de IA (Gemini). |
| `Modelos/` | Entidades (Producto, Proveedor, Movimiento, OrdenCompra) y datos que usa el asistente. |
| `Datos/` | Conexión con SQL Server y consultas con parámetros. |
| `BaseDatos/` | `crear_base_datos.sql` (tablas, claves y relaciones) y `datos_prueba.sql`. |

La justificación de la arquitectura está en [`../docs/arquitectura-integracion.md`](../docs/arquitectura-integracion.md) y las pruebas en [`../docs/pruebas-integracion.md`](../docs/pruebas-integracion.md).

## Requisitos

- Visual Studio con la carga de trabajo **Desarrollo de ASP.NET y web**, que instala SQL Server Express LocalDB, y **Desarrollo de escritorio de .NET**.
- .NET 10 (o .NET 8: cambia `net10.0-windows` por `net8.0-windows` en el `.csproj`).
- WebView2 Runtime, que ya viene en Windows 10 y 11.

## Ejecutar

1. Abre `FerroSuite.slnx` y elige `FerroSuite.IA` como proyecto de inicio.
2. Pulsa F5. Visual Studio descarga los paquetes NuGet (WebView2 y Microsoft.Data.SqlClient) la primera vez.
3. Se abre la ventana de FerroSuite. La primera vez crea la base `FerroSuite` en `(localdb)\MSSQLLocalDB` y carga los datos de prueba.

## Clave de Gemini

En la ventana del programa: **Opciones → Clave de Gemini…** y pega la clave de <https://aistudio.google.com/apikey>. Se guarda en `Documentos\FerroSuite\clave-gemini.txt`, fuera del repositorio. Sin clave, el asistente responde con reglas fijas.

## Para las pruebas

En el menú **Opciones** hay dos interruptores para la prueba de integración 4: **Simular error de comunicación** y **Simular caída de la base de datos**.
