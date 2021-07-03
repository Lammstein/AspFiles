using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AspFiles.Models;
using Microsoft.AspNetCore.Hosting;
using AspFiles.Models.ViewModel;
using Minio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AspFiles.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _enviroment;
        private readonly SystemFilesContext _context;
        public HomeController(IWebHostEnvironment env)
        {
            _enviroment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        
       

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Upload1(UploadModel upload)
        {
            using (var db = new SystemFilesContext())
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    var file = new File();
                    await upload.MyFile.CopyToAsync(ms);
                    file.Filedb = ms.ToArray();
                    db.Files.Add(file);
                    db.SaveChanges();
                }
            }
            TempData["message"] = "Archivo arriba";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Upload2(UploadModel upload)
        {
            var filename = System.IO.Path.Combine(_enviroment.ContentRootPath, 
                "wwwroot/archivos", upload.MyFile.FileName);
            var emptyFile = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            await upload.MyFile.CopyToAsync(emptyFile);
            emptyFile.Close();
            TempData["message"] = "Archivo arriba";
            //System.IO.File.OpenRead(filename);
            Debug.WriteLine("NOMBRE: " + filename);

            using (var db = new SystemFilesContext())
            {
                var lista = db.Rutas.ToList();
                var ruta = new Ruta();
                if (lista.Count == 0)
                {
                    ruta.Id = 1;
                }
                else
                {
                    var aux = lista.ElementAt(lista.Count -1);
                    ruta.Id = aux.Id + 1;
                }
                
                ruta.Rutadb = filename;
                db.Rutas.Add(ruta);
                db.SaveChanges();
                db.Dispose();
            }

            String cadena = filename; // filename, es la ruta del archivo a guardar
            System.IO.FileInfo fi = new System.IO.FileInfo(cadena);
            var nombreConExte = fi.Name;
            var nombreSinExt = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
            Debug.WriteLine("NOMBRE CON: " + nombreConExte);
            Debug.WriteLine("NOMBRE SIN: " + nombreSinExt);
            TempData["message"] = "Archivo arriba";
            //return View("Index");
            //return new PhysicalFileResult(@"" + filename, "image/jpg");
            return new PhysicalFileResult(@"" + filename, "application/pdf");
            //Process.Start(filename);
            //return RedirectToAction("Index");
        }

        public async Task<IActionResult> Upload3(UploadModel upload)
        {
            var filename = System.IO.Path.Combine(_enviroment.ContentRootPath,
                "uploads", upload.MyFile.FileName);
            using(var fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                await upload.MyFile.CopyToAsync(fs);
            }

            var minioclient = new MinioClient("https://mega.nz/fm/GvghiaDC", "vehi970905hoclrs06@gmail.com", "mega#$&+#$2018").WithSSL();
            byte[] bs = await System.IO.File.ReadAllBytesAsync(filename);
            var ms = new System.IO.MemoryStream(bs);

            await minioclient.PutObjectAsync("bucket",upload.MyFile.FileName,
                ms,ms.Length,"application/octet-stream",null,null);

            System.IO.File.Delete(filename);
            TempData["message"] = "Archivo arriba";
            return RedirectToAction("Index");
        }
    }
}
