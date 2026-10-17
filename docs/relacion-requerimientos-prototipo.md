# Relación entre requerimientos, pantallas y código

Este documento muestra cómo el prototipo del frontend (`frontend/`) se relaciona con
los requerimientos del documento maestro (`FerroSuite_Documento_Maestro.docx`,
secciones 13 y 14) y con la arquitectura en capas propuesta en la sección 8.

## Organización del código

| Carpeta | Capa de la arquitectura | Qué contiene |
|---|---|---|
| `frontend/*.html` | Presentación | Una pantalla por archivo: Inicio, Inventario, Proveedores, Órdenes de compra e Ingreso. |
| `frontend/js/components/` | Presentación | Componentes reutilizables: encabezado y menú, pie, tabla, formulario, tarjeta de resumen, etiqueta de estado, panel de alerta y aviso. |
| `frontend/js/controllers/` | Presentación | Un controlador por pantalla. Conecta la pantalla con los servicios y los componentes. |
| `frontend/js/services/` | Aplicación | Reglas del negocio: riesgo de quiebre de stock, validaciones, movimientos, órdenes de compra. |
| `frontend/js/models/` | Dominio / datos | Datos de prueba que simulan las tablas Producto, Proveedor, MovimientoInventario y OrdenCompra. |
| `frontend/css/` | Presentación | `base.css` (variables y botones), `layout.css` (estructura de página), `components.css` (componentes) y `responsive.css`. |

## Requerimientos funcionales y su pantalla

| Requerimiento | Pantalla | Archivos principales | Estado en el prototipo |
|---|---|---|---|
| RF01 Iniciar sesión con rol | `login.html` | `controllers/login.js` | Solo la interfaz; no valida credenciales. |
| RF02 Registrar producto | `inventario.html` | `services/inventarioService.js`, `components/formulario.js` | Funciona con datos de prueba. |
| RF03 Consultar catálogo y existencias | `inventario.html` | `components/tabla.js`, `controllers/inventario.js` | Tabla con búsqueda por SKU o nombre. |
| RF05 Registrar movimiento de inventario | `inventario.html` | `services/inventarioService.js` | Entradas y salidas que actualizan el stock. |
| RF06 Alertar riesgo de quiebre de stock | `inventario.html`, `index.html` | `components/panelAlerta.js` | Regla: cantidad menor o igual al mínimo. |
| RF07 Registrar proveedor | `proveedores.html` | `services/proveedoresService.js` | Funciona con datos de prueba. |
| RF08 Generar orden de compra | `ordenes.html` | `services/ordenesService.js` | Crea la orden en estado pendiente. |
| RF09 Aprobar orden de compra | `ordenes.html` | `controllers/ordenes.js` | Botón que cambia el estado a aprobada. |
| RF10 Consultar cumplimiento de proveedores | `proveedores.html` | `controllers/proveedores.js` | Columna de cumplimiento y proveedor con entrega más rápida. |
| RF12 Validar la información registrada | Todas las pantallas con formularios | `services/*.js`, `components/aviso.js` | Rechaza datos vacíos, cantidades negativas y SKU o NIT repetidos. |

RF04 (modificar y eliminar productos) y RF11 (reportes) no se implementan en esta etapa.

## Qué no se hace todavía

El prototipo no tiene base de datos, backend, autenticación real ni conexión con APIs.
Los datos viven en memoria y se pierden al recargar la página. Esas partes se
construirán en las siguientes entregas sobre el proyecto `FerroSuite.web`.
