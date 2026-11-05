# Relación entre requerimientos, pantallas y código

Este documento muestra cómo el prototipo del frontend (`frontend/`) se relaciona con
los requerimientos del proyecto (secciones 13 y 14 del documento) y con la
arquitectura en capas propuesta en la sección 8.

## Organización del código

| Carpeta | Capa de la arquitectura | Qué contiene |
|---|---|---|
| `frontend/*.html` | Presentación | Una pantalla por archivo: Inicio, Inventario, Proveedores, Órdenes de compra e Ingreso. |
| `frontend/js/components/` | Presentación | Componentes reutilizables: encabezado y menú, pie, tabla, formulario, tarjeta de resumen, etiqueta de estado, panel de alerta, aviso y panel del asistente. |
| `frontend/js/controllers/` | Presentación | Un controlador por pantalla. Conecta la pantalla con los servicios y los componentes. |
| `frontend/js/services/` | Presentación | Validación básica y solicitudes al backend por módulo (inventario, proveedores, órdenes, asistente). |
| `frontend/js/comunicacion/` | Comunicación | `api.js`: envía las solicitudes al backend y entrega las respuestas o los errores. |
| `frontend/js/models/` | Presentación | Listas fijas de los formularios (unidades, categorías y motivos). |
| `FerroSuite.IA/Servicios/` | Aplicación | Reglas del negocio y validaciones, y el asistente de IA (`CalculadoraReposicion`, `GeneradorRecordatorios`, `AsistenteInventario`, `ClienteGemini`). |
| `FerroSuite.IA/Modelos/` | Dominio | Entidades Producto, Proveedor, Movimiento y OrdenCompra. |
| `FerroSuite.IA/Datos/` y `BaseDatos/` | Infraestructura | Acceso a SQL Server y scripts de creación y datos de prueba. |
| `frontend/css/` | Presentación | `base.css` (variables y botones), `layout.css` (estructura de página), `components.css` (componentes) y `responsive.css`. |

## Requerimientos funcionales y su pantalla

| Requerimiento | Pantalla | Archivos principales | Estado en el prototipo |
|---|---|---|---|
| RF01 Iniciar sesión con rol | `login.html` | `controllers/login.js` | Solo la interfaz; no valida credenciales. |
| RF02 Registrar producto | `inventario.html` | `services/inventarioService.js`, `components/formulario.js` | Con categoría, unidad, precio de compra y proveedor. |
| RF03 Consultar catálogo y existencias | `inventario.html` | `components/tabla.js`, `controllers/inventario.js` | Búsqueda por SKU, nombre o categoría; filtros por categoría y por riesgo; días que alcanza el stock. |
| RF04 Modificar y eliminar producto | `inventario.html` | `services/inventarioService.js`, `controllers/inventario.js` | Editar (el stock solo cambia con movimientos) y eliminar con confirmación; no deja eliminar productos con órdenes pendientes. |
| RF05 Registrar movimiento de inventario | `inventario.html` | `services/inventarioService.js` | Entradas y salidas con motivo, fecha y usuario; decimales solo en metros y kilos. |
| RF06 Alertar riesgo de quiebre de stock | `inventario.html`, `index.html` | `components/panelAlerta.js`, `FerroSuite.IA` | Regla: cantidad menor o igual al mínimo, y el asistente avisa si se acaba antes de que llegue el pedido. |
| RF07 Registrar proveedor | `proveedores.html` | `services/proveedoresService.js` | Funciona con datos de prueba. |
| RF08 Generar orden de compra | `ordenes.html`, `inventario.html` | `services/ordenesService.js`, `controllers/asistente.js` | Crea la orden en estado pendiente, también desde una sugerencia del asistente. |
| RF09 Aprobar orden de compra | `ordenes.html` | `controllers/ordenes.js` | Aprobar la orden y marcarla como pagada; muestra la fecha en que vence el pago. |
| RF10 Consultar cumplimiento de proveedores | `proveedores.html` | `controllers/proveedores.js` | Columna de cumplimiento y proveedor con entrega más rápida. |
| RF12 Validar la información registrada | Todas las pantallas con formularios | `services/*.js`, `components/aviso.js` | Rechaza datos vacíos, cantidades negativas o con decimales en unidades enteras, motivos que no corresponden al tipo de movimiento, y SKU o NIT repetidos. |

RF11 (reportes) no se implementa en esta etapa.

## Servicio de inteligencia artificial (sección 17)

| Función | Endpoint | Pantalla | Qué hace |
|---|---|---|---|
| Sugerir reposición | `POST /api/ia/reposicion` | `inventario.html` | Qué pedir, cuánto, a quién y cuánto pagar a cada proveedor. |
| Recordatorios | `POST /api/ia/recordatorios` | `inventario.html` | Pagos por vencer, órdenes por aprobar y pedidos por hacer, redactados por Gemini. |
| Preguntas sobre el inventario | `POST /api/ia/preguntar` | `inventario.html` | Respuestas en lenguaje sencillo con los datos reales. |
| Estado del servicio | `GET /api/salud` | `inventario.html` | Muestra si el asistente está conectado y si usa Gemini. |

## Qué no se hace todavía

Todavía no hay autenticación real: el ingreso es de demostración. La organización completa
de capas está en `arquitectura-integracion.md`.
