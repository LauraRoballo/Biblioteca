using BibliotecaLiceista.Web.Models;
using BibliotecaLiceista.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Controllers
{
    // Controlador MVC: cumple el papel de un Servlet mapeado con @WebServlet("/libros/*").
    // Cada acción atiende una petición HTTP: [HttpGet] equivale a doGet() y [HttpPost] a doPost().
    [Authorize(Roles = RolesUsuario.Administrador)]
    [Route("libros")]
    public class LibrosController : Controller
    {
        private readonly LibroRepository _libroRepository;
        private readonly EditorialRepository _editorialRepository;
        private readonly CategoriaRepository _categoriaRepository;

        public LibrosController(
            LibroRepository libroRepository,
            EditorialRepository editorialRepository,
            CategoriaRepository categoriaRepository)
        {
            _libroRepository = libroRepository;
            _editorialRepository = editorialRepository;
            _categoriaRepository = categoriaRepository;
        }

        // GET /libros  o  GET /libros?busqueda=texto   → CONSULTAR
        [HttpGet("")]
        public async Task<IActionResult> Index(string? busqueda)
        {
            var listaLibros = string.IsNullOrWhiteSpace(busqueda)
                ? await _libroRepository.ConsultarTodosAsync()
                : await _libroRepository.ConsultarPorTituloAsync(busqueda.Trim());

            ViewData["Busqueda"] = busqueda;
            return View(listaLibros);                       // ≈ request.getRequestDispatcher("index.jsp").forward(...)
        }

        // GET /libros/crear   → muestra el formulario vacío
        [HttpGet("crear")]
        public async Task<IActionResult> Crear()
        {
            await CargarListasDesplegablesAsync();
            return View(new Libro());
        }

        // POST /libros/crear   → INSERTAR
        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Libro libro)  // ≈ request.getParameter(...) para cada campo
        {
            if (!ModelState.IsValid)
            {
                await CargarListasDesplegablesAsync();
                return View(libro);                         // vuelve al formulario con los errores
            }

            await _libroRepository.InsertarAsync(libro);
            TempData["Mensaje"] = $"El libro \"{libro.Titulo}\" se registró correctamente.";
            return RedirectToAction(nameof(Index));         // ≈ response.sendRedirect("/libros")
        }

        // GET /libros/editar/5   → muestra el formulario con los datos del libro
        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Editar(int id)
        {
            var libro = await _libroRepository.ConsultarPorIdAsync(id);
            if (libro is null)
            {
                return NotFound();
            }

            await CargarListasDesplegablesAsync();
            return View(libro);
        }

        // POST /libros/editar/5   → ACTUALIZAR
        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Libro libro)
        {
            if (id != libro.IDLibro)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarListasDesplegablesAsync();
                return View(libro);
            }

            await _libroRepository.ActualizarAsync(libro);
            TempData["Mensaje"] = $"El libro \"{libro.Titulo}\" se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST /libros/eliminar/5   → ELIMINAR
        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _libroRepository.EliminarAsync(id);
                TempData["Mensaje"] = "El libro se eliminó correctamente.";
            }
            catch (SqlException error) when (error.Number == 547)   // 547 = violación de llave foránea
            {
                TempData["Error"] = "No se puede eliminar el libro porque tiene ejemplares, autores o reservas registradas.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasDesplegablesAsync()
        {
            ViewBag.ListaEditoriales = await _editorialRepository.ConsultarTodosAsync();
            ViewBag.ListaCategorias = await _categoriaRepository.ConsultarTodosAsync();
            ViewBag.ListaEstados = await _libroRepository.ConsultarEstadosLibroAsync();
        }
    }
}
