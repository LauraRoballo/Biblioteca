# Reglas de Negocio

RN-01 Un usuario puede tener un número máximo de libros prestados según su rol.

RN-02 El tiempo máximo de préstamo varía según el perfil del usuario (docente o alumno).

RN-03 Un libro no puede ser prestado si su estado es "no disponible".

RN-04 Un usuario con sanciones activas no puede realizar nuevas reservas.

RN-05 Las sanciones se generan automáticamente cuando se supera la fecha de devolución.

RN-06 Solo el administrador puede crear, modificar o eliminar usuarios del sistema.

RN-07 Cada libro puede tener un número máximo de reservas activas definido por el sistema.

RN-08 Cuando un libro prestado es devuelto, el sistema debe notificar automáticamente al primer usuario en la lista de reservas.

RN-09 Las reservas deben gestionarse en orden de solicitud (cola de espera).
