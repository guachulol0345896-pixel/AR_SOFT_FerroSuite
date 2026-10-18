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
- **Inventario:** catálogo con búsqueda, panel de productos en riesgo de agotarse, registro de entradas y salidas, y formulario para agregar productos.
- **Proveedores:** listado, resumen y formulario con validación de NIT repetido.
- **Órdenes de compra:** crear una orden y aprobarla.
- **Ingreso:** pantalla de demostración con selección de rol, sin autenticación real.

El código está separado por función: `css/` (base, layout, componentes y responsive) y `js/` (`components/`, `controllers/`, `services/` y `models/`). La relación entre requerimientos, pantallas y archivos está en [`docs/relacion-requerimientos-prototipo.md`](docs/relacion-requerimientos-prototipo.md).

Los datos son de prueba y viven en memoria: se pierden al recargar la página. Todavía no hay base de datos, backend ni autenticación real.

## Estructura del repositorio

```text
├── README.md
├── docs/                 Documentación (ERP.docx y relación entre requerimientos y prototipo)
├── diagrams/             Diagramas UML y de arquitectura
├── frontend/             Prototipo web: pantallas, css y js
└── FerroSuite.web/       Proyecto base ASP.NET Core para el backend
```

## Cómo ejecutar el prototipo

Abre `frontend/index.html` en el navegador. No necesita instalación.
