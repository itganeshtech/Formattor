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
            //TempData["SchoolResult"] = JsonConvert.SerializeObject(schoolResult);

            HttpContext.Session.SetString("SchoolResult", JsonConvert.SerializeObject(schoolResult));

            // Redirect to Success Page
            return RedirectToAction("Success", "FileUpload", new { fileName = HttpUtility.UrlEncode(uploadedFile.FileName) });
        }

        return View("Index");

    }

    // GET: File/Success
    public IActionResult Success(string fileName)
    {
        // Deserialize the JSON string back to SchoolResult object
        //var schoolResultJson = TempData["SchoolResult"]  as string;

        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

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

    public IActionResult GetFirstThreeToppers()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var toppers = schoolResult.Students.OrderByDescending(x => x.Percentage).Take(3)
            .ToList();

        return PartialView("_ToppersPartial", toppers);
    }

    //To calculate the percentage of the students
    public IActionResult GetPercent()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var allToppers = schoolResult.Students.OrderByDescending(x => x.Percentage).ToList();

        //checking dictionary values of subjects
        foreach (var student in schoolResult.Students)
        {
            foreach (var subject in student.Subjects)
            {
                Console.WriteLine($"Code: {subject.Key}, Name: {subject.Value.SubjectName}, Marks: {subject.Value.Marks}, Grade: {subject.Value.Grade}");
            }
        }

        return PartialView("_PercentPartial", allToppers);
    }

    //Test LINQ query
    public IActionResult TestLinq()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var toppers = schoolResult.Students
                      .Where(s => s.Percentage > 95).ToList();


        //Console.WriteLine("Test Completed");
        return PartialView("_TestLinqPartial", toppers);
    }

    //To calculate subject wise topper
    public IActionResult GetSubjectWiseTopper()
    {
        //Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        //Simulating getting the top 3 students
        // var toppers = schoolResult.Students.OrderByDescending(x => x.Percentage).Take(3)
        //.ToList();

        var subjectwiseToppers = schoolResult.Students
    .GroupBy(x => x.Subjects) // Group by Subject Code
    .Select(group => group
        .OrderByDescending(x => x.Percentage) // Get the topper in each subject
        .FirstOrDefault()) // Get the student with the highest percentage
    .Where(x => x != null) // Exclude any null results
    .ToList();

        return PartialView("_SubjectWiseTopperPartial", subjectwiseToppers);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
