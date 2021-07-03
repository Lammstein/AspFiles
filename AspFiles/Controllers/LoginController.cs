using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspFiles.Models;
using AspFiles.Models.SchoolViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AspFiles.Controllers
{
    public class LoginController : Controller
    {
        private readonly SystemFilesContext _context;

        public LoginController(SystemFilesContext context)
        {
            _context = context;
        }

        // GET: LoginController
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Home");
            }
            return View();
        }

     
        public async Task<IActionResult> Login([Bind("NombreUsuario,Password")] UsuarioVM usuarioVM)
        {
            if (ModelState.IsValid)
            {                
                var result = await _context.Usuario
                    .Where(x => x.Nombre == usuarioVM.NombreUsuario)
                    .SingleOrDefaultAsync();
                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Verifique su nombre de usuario");
                    
                }
                else
                {
                   if(usuarioVM.Password.Equals(result.Password))
                    {
                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()));
                        identity.AddClaim(new Claim(ClaimTypes.Name, result.Nombre));
                        identity.AddClaim(new Claim(ClaimTypes.Email, result.Correo));
                        identity.AddClaim(new Claim("Dato", "Valor"));
                        

                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                            new AuthenticationProperties { ExpiresUtc = DateTime.Now.AddHours(1), IsPersistent = true });

                        return RedirectToAction(nameof(Index), "Home");

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Contraseña incorrecta");
                    }
                }
            }
            return View("Index",usuarioVM);
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Login");
        }
        
    }
}
