using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FormatModals;
using FormatResult.Models;

namespace FormatResult.Controllers;

public class FileUploadController : Controller
{
    private readonly ILogger<FileUploadController> _logger;
    private readonly IWebHostEnvironment _environment;
    public FileUploadController(ILogger<FileUploadController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }
    [HttpGet]
    public IActionResult Index()
    {
            return View();
    }

    // POST: File/Upload
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)

    {

        if (file != null && file.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Redirect to Success Page
            return RedirectToAction("Success", new { fileName = file.FileName });
        }

        return View("Index");

    }

    // GET: File/Success
    public IActionResult Success(string fileName)
    {
        var model = new FileUploadViewModel
        {
            FileName = fileName
        };
        return View(model);
    }


    public IActionResult GsIntro()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
