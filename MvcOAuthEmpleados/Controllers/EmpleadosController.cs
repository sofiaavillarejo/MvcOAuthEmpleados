using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MvcOAuthEmpleados.Filters;
using MvcOAuthEmpleados.Models;
using MvcOAuthEmpleados.Services;

namespace MvcOAuthEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private ServiceEmpleados service;

        public EmpleadosController(ServiceEmpleados service)
        {
            this.service = service;
        }
        [AuthorizeEmpleados]
        public async Task<IActionResult> Index()
        {
            List<Empleado> empleados = await this.service.GetEmpleadosAsync();
            return View(empleados);
        }

        public async Task<IActionResult> Details(int id)
        {
            Empleado empleado = await
                this.service.FindEmpleadoAsync(id);
            return View(empleado);
            //------------------------------------------------------------
            //NO TIENE LOGICA ALMACENAR EL TOKEN EN SESSION, ASI QUE LO MODIFICAMOS

            ////TENDREMOS EL TOKEN EN SESSION
            //string token = HttpContext.Session.GetString("TOKEN");
            //if (token == null)
            //{
            //    ViewData["MENSAJE"] = "Debe validarse en Login";
            //    return View();
            //}
            //else
            //{
            //}
        }

        //ESTARA PROTEGIDO EN LA API Y SE HARA ALLI EL METODO, AQUI SIMPLEMENTE LE DAREMOS EL TOKEN Y LO PINTARA DIRECTAMENTE

        [AuthorizeEmpleados]
        public async Task<IActionResult> Perfil()
        {
            Empleado emp = await this.service.GetPerfilAsync();
            return View(emp);
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Compis()
        {
            List<Empleado> empleados = await this.service.GetCompislAsync();
            return View(empleados);
        }
    }
}
