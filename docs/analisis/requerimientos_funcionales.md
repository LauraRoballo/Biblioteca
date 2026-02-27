# Requerimientos funcionales ⚙️

## RF-001: Creación de cuentas de usuario
El sistema no debe permitir el registro público de usuarios.
Las cuentas de alumnos, docentes, bibliotecarios y administradores deben ser creadas exclusivamente por el rol administrador, asociadas a un correo institucional válido.

## RF-002: Activación de cuenta y definición de contraseña
Una vez creada la cuenta, el sistema debe enviar al correo institucional del usuario un enlace de activación, de un solo uso y con vigencia limitada, mediante el cual el usuario podrá definir su contraseña de acceso.

El enlace de activación debe quedar automáticamente deshabilitado una vez la contraseña haya sido creada por primera vez.

## RF-003: Inicio de sesión
El sistema debe permitir que los usuarios con cuenta activa (alumnos, docentes, bibliotecarios y administradores) inicien sesión utilizando su correo institucional y la contraseña definida durante la activación de la cuenta.

## RF-004: Gestión de libros
El sistema debe permitir al bibliotecario registrar, editar y eliminar la información de los libros disponibles en la biblioteca (título, autor, categoría, código, cantidad y estado).

El sistema debe permitir a los usuarios consultar la disponibilidad de los libros.

## RF-005: Gestión de préstamos
El sistema debe permitir al bibliotecario registrar los préstamos de libros a alumnos y docentes, incluyendo la fecha de entrega y la fecha de devolución.

## RF-006: Gestión de devoluciones
El sistema debe permitir al bibliotecario registrar la devolución de los libros prestados y actualizar su disponibilidad.

## RF-007: Gestión de reservas
El sistema debe permitir a los usuarios realizar reservas de libros que se encuentren actualmente prestados y recibir notificaciones cuando estos estén disponibles.

## RF-008: Gestión de usuarios
El sistema debe permitir al administrador gestionar los datos de los usuarios del sistema, incluyendo la modificación, eliminación y asignación de roles 

## RF-009: Gestión de estadísticas
El sistema debe permitir al administrador y al bibliotecario generar reportes y estadísticas sobre el funcionamiento de la biblioteca, como libros más prestados, usuarios con mayor número de préstamos y cantidad de reservas.

## RF-010: Notificaciones y alertas
El sistema debe enviar notificaciones automáticas a los usuarios cuando se acerque la fecha de vencimiento de un préstamo o cuando un libro reservado esté disponible.

## RF-011: Políticas por rol
El sistema debe permitir al administrador definir y gestionar políticas de préstamo asociadas a los distintos roles de usuario (alumnos y docentes), tales como límites de cantidad de libros y tiempos máximos de préstamo.

## RF-012: Difusión literaria
El sistema debe contar con una sección pública para la publicación de noticias del colegio, recomendaciones de lectura y autores destacados del mes.

