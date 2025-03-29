using BusinessLogic;
using FormatModals;
using FormatResult.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web;

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
        // string file = uploadedFile.FileName.Replace(" ", "");
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
            // Parse the file and get schoolResult
            var schoolResult = Parser.ParseFile(filePath);

            // Serialize the schoolResult object to JSON and store it in TempData
            TempData["SchoolResult"] = JsonConvert.SerializeObject(schoolResult);


            // Redirect to Success Page
            return RedirectToAction("Success", "FileUpload", new { fileName = HttpUtility.UrlEncode(uploadedFile.FileName) });

        }

        return View("Index");

    }

    // GET: File/Success
    public IActionResult Success(string fileName)
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = TempData["SchoolResult"] as string;
        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        var model = new FileUploadViewModel
        {
            FileName = fileName,
            SchoolResult = schoolResult
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
