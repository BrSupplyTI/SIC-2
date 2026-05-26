using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIC.Web.Areas.Comercial.Controllers;

[Area("Comercial")]
[Authorize]
public sealed class ComercialController : Controller
{
    public IActionResult AdmClientes()
    {
        ViewData["Title"] = "Adm. de Clientes";
        return View();
    }

    public IActionResult AdmPedidos()
    {
        ViewData["Title"] = "Adm. de Pedidos";
        return View();
    }
}
