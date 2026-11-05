# FerroSuite

**ERP modular en la nube para ferreterías pyme en Colombia.** Proyecto de la asignatura Arquitectura de Software, sexto semestre de Ingeniería de Software, Universidad Manuela Beltrán (Bogotá D.C., 2026).

**Integrantes:** Santiago Rios Duque, Juan Diego Rojas y Zhadyth Alejandra Puentes Velandia.

## ¿Qué es FerroSuite?

Muchas ferreterías pequeñas y medianas llevan su inventario, sus proveedores y sus cuentas en cuadernos u hojas de cálculo. Eso causa descuadres de inventario, pérdida de control sobre los proveedores y errores en la contabilidad. Los ERP que existen son muy genéricos para manejar unidades mixtas (metros, kilos, unidades, varillas) o muy costosos para un negocio pequeño.

FerroSuite propone un sistema modular: el negocio empieza con **Inventario** y **Proveedores** y puede sumar después Contabilidad, RRHH y CRM, sin rehacer lo que ya funciona.

## Lo que se ha trabajado

| Fase | Trabajo realizado |
|---|---|
| 1 | Definición del proyecto: contexto, justificación, problemática, necesidad y pregunta problema. Primera interfaz web con la pantalla de Inicio. |
| 2 | Diseño de la arquitectura de software: arquitectura en capas, modelo relacional, diagramas UML (casos de uso, clases y componentes), lenguajes, herramientas y metodología. Pantallas de Inventario y Proveedores con datos de prueba. |
| 3 | Definición de requerimientos funcionales y no funcionales, y de los marcos conceptual, contextual y de inteligencia artificial. Código del prototipo organizado en componentes, servicios y controladores, y nuevas pantallas de Órdenes de compra e Ingreso. |
| 4 | Módulo de Inventario completo (unidades mixtas, editar y eliminar, historial con usuario y motivo, días que alcanza el stock) y asistente de IA conectado a Gemini, con respuestas y recordatorios. |
| 5 | Integración completa: aplicativo de escritorio, backend en C# con validaciones, base de datos SQL Server con script y datos de prueba, comunicación entre frontend y backend, y pruebas de integración. |

Además se hizo un prototipo de escritorio en C# (Windows Forms), una presentación de avance y una infografía del proyecto. Esos materiales no están en este repositorio.

## Arquitectura

El sistema se organiza en cuatro capas, para que cada módulo nuevo se pueda agregar sin tocar lo que ya existe:

- **Presentación:** páginas Razor Pages (ASP.NET Core).
- **Aplicación:** servicios de cada módulo, con las reglas del negocio.
- **Dominio:** entidades y reglas del negocio.
- **Infraestructura:** repositorios y servicios externos, con SQL Server Express como base de datos.

| Herramienta | Uso |
|---|---|
| C# y ASP.NET Core con Razor Pages | Backend y páginas web |
| SQL Server Express | Base de datos |
| Visual Studio Community | Desarrollo y pruebas |
| PlantUML | Diagramas UML y modelo relacional |
| GitHub y GitHub Actions | Control de versiones y compilación automática |
| Hangfire | Tareas programadas, como recalcular el riesgo de agotarse un producto |

Los diagramas están en [`diagrams/`](diagrams/).

## Prototipo web

El prototipo está en [`frontend/`](frontend/) y usa HTML, CSS y JavaScript sin instalar nada. Tiene cinco pantallas:

- **Inicio:** presentación de los módulos y resumen del estado del prototipo.
- **Inventario:** catálogo con búsqueda y filtros, unidades mixtas (metros y kilos aceptan decimales), días que alcanza el stock según las ventas, alertas de productos por agotarse, entradas y salidas con motivo y usuario, agregar, editar y eliminar productos, y el **asistente de IA**.
- **Proveedores:** listado, resumen y formulario con validación de NIT repetido.
- **Órdenes de compra:** crear una orden y aprobarla.
- **Ingreso:** pantalla de demostración con selección de rol, sin autenticación real.

El código está separado por función: `css/` (base, layout, componentes y responsive) y `js/` (`components/`, `controllers/`, `services/` y `models/`). La relación entre requerimientos, pantallas y archivos está en [`docs/relacion-requerimientos-prototipo.md`](docs/relacion-requerimientos-prototipo.md).

Los datos se guardan en SQL Server a través del backend. El botón "Restaurar datos de prueba" vuelve al estado inicial. El ingreso todavía es de demostración, sin autenticación real.

## Aplicativo, backend y base de datos

FerroSuite corre como un **aplicativo de escritorio para Windows** (proyecto `FerroSuite.IA`). La ventana muestra las pantallas de `frontend/`, y estas se comunican por mensajes con el backend en C#, que valida, aplica las reglas del negocio y guarda en **SQL Server LocalDB**. No usa `localhost`.

- **Frontend:** `frontend/` (pantallas, componentes, controladores y servicios que piden datos al backend).
- **Comunicación:** `frontend/js/comunicacion/api.js` ↔ `FerroSuite.IA/Comunicacion/Puente.cs`.
- **Backend:** `FerroSuite.IA/Servicios/`, con validaciones y manejo de errores.
- **Base de datos:** `FerroSuite.IA/BaseDatos/crear_base_datos.sql` (tablas `Proveedor`, `Producto`, `MovimientoInventario` y `OrdenCompra`, con claves primarias y foráneas) y `datos_prueba.sql`.
- **Asistente de IA:** sugiere qué pedir, recuerda pagos a proveedores y órdenes por aprobar, y responde preguntas. Usa Gemini si hay clave y reglas fijas si no.

La explicación de la arquitectura está en [`docs/arquitectura-integracion.md`](docs/arquitectura-integracion.md) y las pruebas de integración en [`docs/pruebas-integracion.md`](docs/pruebas-integracion.md).

## Estructura del repositorio

```text
├── README.md
├── docs/                 Documentación (ERP.docx y relación entre requerimientos y prototipo)
├── diagrams/             Diagramas UML y de arquitectura
├── frontend/             Prototipo web: pantallas, css y js
├── FerroSuite.IA/        Aplicativo de escritorio: backend, comunicación, base de datos e IA
└── FerroSuite.web/       Proyecto base ASP.NET Core para el backend
```

## Cómo ejecutar

1. Abre `FerroSuite.slnx` en Visual Studio y elige `FerroSuite.IA` como proyecto de inicio.
2. Pulsa F5. La primera vez se crea la base de datos con los datos de prueba.
3. Para usar Gemini: en el programa, **Opciones → Clave de Gemini…**.

