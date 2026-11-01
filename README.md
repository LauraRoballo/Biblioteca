# 📚 Sistema de Gestión Bibliotecaria - Liceo Femenino Mercedes Nariño

Plataforma integral diseñada para modernizar y optimizar la administración de recursos bibliográficos del Colegio Liceo Femenino Mercedes Nariño. El sistema automatiza el ciclo completo de préstamo, devoluciones y reservas, eliminando procesos manuales y mejorando la experiencia de la comunidad educativa. 

## Alcance General
El sistema está orientado a la gestión interna de la biblioteca, incluyendo el control de libros, usuarios, préstamos, reservas y estadísticas administrativas. No contempla procesos de adquisición de libros ni integración con sistemas externos.

## Tecnologías

* **Lenguaje de Programación:** C#
* **Backend:** ASP.NET Core (.NET 10)
* **Acceso a datos:** ADO.NET con `Microsoft.Data.SqlClient` (equivalente en .NET a JDBC)
* **Base de Datos:** SQL Server
* **Frontend:** Blazor (renderizado interactivo en servidor)
* **Seguridad:** autenticación por cookie, autorización por roles y contraseñas cifradas con `PasswordHasher`

## Arquitectura

```
BibliotecaLiceista.Web/
├── Components/     Capa de presentación (páginas y layouts por rol)
├── Models/         Clases que representan las tablas de la base de datos
├── Repositories/   Acceso a datos: operaciones CRUD con ADO.NET
└── Data/           Conexión a SQL Server
```

## Estado del Proyecto

El proyecto se encuentra en **fase de construcción**. Están implementados:

* Conexión a SQL Server con ADO.NET.
* CRUD de usuarios: registro (insertar), consulta, actualización y eliminación desde Gestión de Usuarios.
* Inicio de sesión que valida las credenciales y redirige a cada usuario a su panel según su rol (Administrador, Bibliotecario, Alumno/Docente).
* Repositorio de libros con las operaciones CRUD.

La documentación se organiza en las siguientes secciones:

* [Análisis de requerimientos](./docs/analisis/)
* [Ver diseño y diagramas](./docs/diseno/diseño.md)

## Funcionalidades Principales

- Gestión de préstamos y devoluciones con control de fechas.
- Perfiles diferenciados según rol (bibliotecarios, administrativos, docentes y alumnos).
- Generación de reportes y estadísticas sobre el uso de la biblioteca.
