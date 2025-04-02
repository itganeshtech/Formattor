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

    //OVERALL QUICK SUMMARY
    public IActionResult GetOverallSummary()
    {
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");
        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult();

        var maxPercentage = schoolResult.Students.Max(s => s.Percentage);

        var viewModel = new OverallSummaryViewModel
        {
            SchoolResult = schoolResult,
            Toppers = schoolResult.Students
                .Where(s => s.Percentage == maxPercentage)
                .ToList(),
            CountAbove95 = schoolResult.Students.Count(s => s.Percentage > 95),
            CountAbove90 = schoolResult.Students.Count(s => s.Percentage > 90),
            CountPass = schoolResult.Students.Count(s => s.OverallResult.Equals("Pass", StringComparison.OrdinalIgnoreCase)),
            CountFail = schoolResult.Students.Count(s => s.OverallResult.Equals("Fail", StringComparison.OrdinalIgnoreCase)),
            CountCompartment = schoolResult.Students.Count(s => s.OverallResult.Equals("Compartment", StringComparison.OrdinalIgnoreCase)),
            MaxPercentage = maxPercentage
        };

        return PartialView("_OverallSummaryPartial", viewModel);
    }

    //TO DISPLAY FIRST THREE TOPPERS
    public IActionResult GetFirstThreeToppers()
    {
        
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top  students
        var toppers = schoolResult.Students
                                .SelectMany(student => student.Subjects, (student, subject) => new SubjectWiseResultViewModel
                                {
                                    Name = student.Name,
                                    RollNumber = student.RollNumber,
                                    SubjectCode = subject.Key,
                                    SubjectName = subject.Value.SubjectName,
                                    Marks = subject.Value.Marks
                                })
                                .GroupBy(s => s.SubjectCode)
                                .Select(group => group
                                    .OrderByDescending(s => s.Marks)
                                    .First())
                                .ToList();

        return PartialView("_ToppersPartial", toppers);
    }

    //TO DISPLAY PERCENTAGE OF ALL THE STUDENTS
    public IActionResult GetPercent()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var allToppers = schoolResult.Students.OrderByDescending(x => x.Percentage).ToList();

        return PartialView("_PercentPartial", allToppers);
    }

    //TO DISPLAY FULL DETAILS OF SUBJECT WISE TOPPERS
    public IActionResult GetSubjectWiseTopper()
    {
        //Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        var subjectwiseToppers = schoolResult.Students
                                .SelectMany(student => student.Subjects, (student, subject) => new SubjectWiseResultViewModel
                                {
                                    Name = student.Name,
                                    RollNumber = student.RollNumber,
                                    SubjectCode = subject.Key,
                                    SubjectName = subject.Value.SubjectName,
                                    Marks = subject.Value.Marks
                                })
                                .GroupBy(s => s.SubjectCode)
                                .Select(group => group
                                    .OrderByDescending(s => s.Marks)
                                    .First())
                                .ToList();

        return PartialView("_SubjectWiseTopperPartial", subjectwiseToppers);
    }

    // COUNT OF TOTAL 100 AND  SUBJECT WISE 100 MARKS WITH STUDENT DETAILS
    public IActionResult GetFullMarks()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var toppers = schoolResult.Students
                      .Where(s => s.Percentage > 95).ToList();
        return PartialView("_FullMarksPartial", toppers);
    }

    // GET SUBJECT WISE FULL DETAIL OF STUDENTS LIKE SUBJECT TOPPER AND STUDENTS IN 90S AND 80S AND 70S
    public IActionResult GetSubjectWiseDetails()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top 3 students
        var toppers = schoolResult.Students
                      .Where(s => s.Percentage > 95).ToList();
        return PartialView("_SubjectWiseDetailsPartial", toppers);
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
      
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
