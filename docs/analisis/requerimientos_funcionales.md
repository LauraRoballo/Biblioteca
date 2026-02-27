# Requerimientos funcionales ⚙️

## RF-001: Creación de cuentas de usuario
El sistema no debe permitir el registro público de usuarios.Las cuentas de alumnos, docentes, bibliotecarios y administradores deben ser creadas exclusivamente por el rol administrador, asociadas a un correo institucional válido.

## RF-002: Activación de cuenta y definición de contraseña: 
Una vez creada la cuenta, el sistema debe enviar al correo institucional del usuario un enlace de activación de un solo uso y con vigencia limitada, mediante el cual el usuario podrá definir su contraseña de acceso.

El enlace de activación debe quedar automáticamente deshabilitado una vez la contraseña haya sido creada por primera vez.

## RF-003 Inicio de sesión
El sistema debe permitir que los usuarios con cuenta activa (alumnos, docentes, bibliotecarios y administradores) inicien sesión utilizando su correo institucional y la contraseña definida durante la activación de la cuenta.

## RF-004: Gestión de libros
El sistema debe permitir al bibliotecario registrar, editar, eliminar y consultar la información de los libros disponibles en la biblioteca (título, autor, categoría, código, cantidad, estado).

## RF-005: Gestión de prestamos 
El sistema debe permitir registrar los préstamos de libros a alumnos y docentes, incluyendo la fecha de entrega y la fecha de devolución.

## RF-006: Gestión de devoluciones 
El sistema debe registrar la devolución de los libros prestados, actualizar su disponibilidad y generar sanciones en caso de retrasos.

## RF-007: Gestión de reservas
Permitir que los usuarios reserven libros que se encuentran actualmente prestados y recibir notificaciones cuando estén disponibles.

## RF-008: Gestión de usuarios 
Permitir al administrador gestionar los datos de los usuarios del sistema (crear, modificar, eliminar, asignar roles y restablecer contraseñas).

## RF-009: Gestión de estadísticas 
El sistema debe generar reportes y estadísticas sobre el funcionamiento de la biblioteca, como libros más prestados, usuarios con más sanciones o préstamos, y cantidad de reservas mensuales.

## RF-010: Notificaciones y alertas 
El sistema debe enviar avisos automáticos a los usuarios cuando se acerque la fecha de vencimiento de un préstamo o cuando un libro reservado esté disponible.

## RF-011: Politicas por rol 
El sistema debe permitir establecer diferentes límites de cantidad de libros y días de préstamo según el perfil (Ej: más tiempo para docentes que para alumnos).

## RF-012: Difusión literaria 
El sistema contará con una sección pública para publicar noticias del colegio, recomendaciones de lectura y autores destacados del mes.






