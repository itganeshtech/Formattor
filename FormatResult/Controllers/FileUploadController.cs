using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FormatModals;
using FormatResult.Models;
using FormatResult.BusinessLogic;

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
    public async Task<IActionResult> Upload(IFormFile uploadedFile)

    {

        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uploadedFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }

           var schoolResult = Parser.ParseFile(filePath);

            // Redirect to Success Page
            return RedirectToAction("Success", new { fileName = uploadedFile.FileName });
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
