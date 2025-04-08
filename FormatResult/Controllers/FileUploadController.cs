using AspNetCoreGeneratedDocument;
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

    //TO DISPLAY First Toppers
    public IActionResult GetTopPerformersPercentWise()
    {

        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Simulating getting the top  students
        // Get distinct percentages and find top 3
        var topPercentages = schoolResult.Students
            .Select(s => s.Percentage)
            .Distinct()
            .OrderByDescending(p => p)
            .Take(3)
            .ToList();

        // Group  top performing students on the basis of percentage
        var result = schoolResult.Students
            .Where(s => topPercentages.Contains(s.Percentage))
            .GroupBy(s => s.Percentage)
            .OrderByDescending(g => g.Key)
            .SelectMany(g => g
                .OrderByDescending(s => s.Percentage)
                .ThenBy(s => s.Name)
                .Select(s => new FirstToppersViewModel
                {
                    Name = s.Name,
                    RollNumber = s.RollNumber,
                    Percentage = s.Percentage,
                    Student = s,
                    SubjectCode = "",   //s.Subjects.ToLookup<>"",
                    SubjectName = "Overall Result"
                }))
            .ToList();
        // return PartialView("_FirstToppersPartial", result);
        return PartialView("_TopPerformersPercentWise", result);

    }

    //TO DISPLAY PERCENTAGE OF ALL THE STUDENTS
    public IActionResult GetAllStudentsPercent()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Calculating percentage of all students
        var allToppers = schoolResult.Students.OrderByDescending(x => x.Percentage).ToList();

        //return PartialView("_PercentPartial", allToppers);
        return PartialView("_AllStudentsPercent", allToppers);
    }

    //TO DISPLAY FULL DETAILS OF SUBJECT WISE TOPPERS
    public IActionResult GetSubjectWiseTopper()
    {
        //Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        /*
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
                                    .Take(3))
                                .ToList();*/

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
            .SelectMany(group =>
            {
                // Get the top 3 distinct marks for the subject
                var topMarks = group
                    .OrderByDescending(s => s.Marks)
                    .Select(s => s.Marks)
                    .Distinct()
                    .Take(3)
                    .ToHashSet(); // Using HashSet for fast lookup

                // Return all students who have those marks
                return group.Where(s => topMarks.Contains(s.Marks));
            })
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

        // Display students who scored 100 in any subject
        var subjectWiseCenturions = schoolResult.Students
        .SelectMany(student => student.Subjects
            .Where(subject => subject.Value.Marks == 100)
            .Select(subject => new FullMarksViewModel
            {
                SubjectCode = subject.Key,
                SubjectName = subject.Value.SubjectName,
                RollNumber = student.RollNumber,
                Name = student.Name,
                Marks = subject.Value.Marks,
                Percentage = student.Percentage,
                Gender = student.Gender,
                OverallResult = student.OverallResult,
                Subjects = student.Subjects
            }))
        .ToList();

        return PartialView("_FullMarksPartial", subjectWiseCenturions);
    }

    // GET SUBJECT WISE FULL DETAIL OF STUDENTS LIKE SUBJECT TOPPER AND STUDENTS IN 90S AND 80S AND 70S
    public IActionResult GetSubjectFullDetails()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        // Get subject name and code (assuming first subject for demonstration)
        // Get subject name and code (using first student's first subject if available)
        string subjectName = "All Subjects";
        string subjectCode = "ALL";

        if (schoolResult.Students.Any() && schoolResult.Students[0].Subjects.Any())
        {
            var firstSubject = schoolResult.Students[0].Subjects.First();
            subjectName = firstSubject.Value.SubjectName;
            subjectCode = firstSubject.Key;
        }

        // Get Subject-wise Top 3 Students (with ties handled)
        var subjectWiseToppers = schoolResult.Students
                                .SelectMany(student => student.Subjects, (student, subject) => new
                                {
                                    SubjectCode = subject.Key,
                                    SubjectName = subject.Value.SubjectName,
                                    Student = new
                                    {
                                        student.RollNumber,
                                        student.Name,
                                        student.Gender,
                                        student.OverallResult,
                                        student.Percentage
                                    },
                                    Marks = subject.Value.Marks,
                                    Grade = subject.Value.Grade
                                })
                                .GroupBy(x => new { x.SubjectCode, x.SubjectName })
                                .Select(g => new
                                {
                                    SubjectCode = g.Key.SubjectCode,
                                    SubjectName = g.Key.SubjectName,
                                    TopStudents = g
                                        .OrderByDescending(x => x.Marks)
                                        .GroupBy(x => x.Marks)
                                        .Take(3)
                                        .SelectMany(group => group)
                                        .Select(s => new
                                        {
                                            s.Student.RollNumber,
                                            s.Student.Name,
                                            s.SubjectCode,
                                            s.SubjectName,
                                            s.Marks,
                                            s.Grade,
                                            s.Student.Percentage
                                        })
                                        .ToList()
                                })
                                .ToList();

        var viewModel = new List<SubjectFullDetailsViewModel>();

        foreach (var item in subjectWiseToppers)
        {
            viewModel.Add(new SubjectFullDetailsViewModel
            {
                SubjectName = item.SubjectName,
                SubjectCode = item.SubjectCode,
                FullMarks = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks == 100))
                .ToList(),
                Above95 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks >= 95))
                .ToList(),
                Above90 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks > 90 && x.Value.Marks < 95))
                .ToList(),
                Between80n90 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks > 80 && x.Value.Marks <= 90))
                .ToList(),
                Between70n80 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks > 70 && x.Value.Marks <= 80))
                .ToList(),
                Between60n70 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks > 60 && x.Value.Marks <= 70))
                .ToList(),
                Between50n60 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks > 50 && x.Value.Marks <= 60))
                .ToList(),
                Between33n50 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks >= 33 && x.Value.Marks <= 50))
                .ToList(),
                Pass = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks >= 33))
                .ToList(),
                Fail = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == subjectCode && x.Value.Marks < 33))
                .ToList(),
                Compartment = schoolResult.Students
                .Where(s => s.OverallResult.Equals("Compartment", StringComparison.OrdinalIgnoreCase))
                .ToList(),
                SubjectWiseToppers = item.TopStudents // Add the toppers data to the view model
            });
        }



        /*
        // Create and populate the ViewModel
        var viewModel = new SubjectFullDetailsViewModel
        {
            SubjectName = subjectName,
            SubjectCode = subjectCode,
            FullMarks = schoolResult.Students
                .Where(s => s.Percentage == 100)
                .ToList(),
            Above95 = schoolResult.Students
                .Where(s => s.Percentage >= 95)
                .ToList(),
            Above90 = schoolResult.Students
                .Where(s => s.Percentage > 90 && s.Percentage<95)
                .ToList(),
            Between80n90 = schoolResult.Students
                .Where(s => s.Percentage > 80 && s.Percentage <= 90)
                .ToList(),
            Between70n80 = schoolResult.Students
                .Where(s => s.Percentage > 70 && s.Percentage <= 80)
                .ToList(),
            Between60n70 = schoolResult.Students
                .Where(s => s.Percentage > 60 && s.Percentage <= 70)
                .ToList(),
            Between50n60 = schoolResult.Students
                .Where(s => s.Percentage > 50 && s.Percentage <= 60)
                .ToList(),
            Between33n50 = schoolResult.Students
                .Where(s => s.Percentage >= 33 && s.Percentage <= 50)
                .ToList(),
            Pass = schoolResult.Students
                .Where(s => s.OverallResult.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                .ToList(),
            Fail = schoolResult.Students
                .Where(s => s.OverallResult.Equals("Fail", StringComparison.OrdinalIgnoreCase))
                .ToList(),
            Compartment = schoolResult.Students
                .Where(s => s.OverallResult.Equals("Compartment", StringComparison.OrdinalIgnoreCase))
                .ToList(),
            SubjectWiseToppers = subjectWiseToppers // Add the toppers data to the view model
        };
        */
        return PartialView("_SubjectFullDetailsPartial", viewModel);
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


/*
 * 
 * 
 * public IActionResult GetSubjectFullSummary()
{
    // Deserialize the JSON string back to SchoolResult object
    var schoolResultJson = HttpContext.Session.GetString("SchoolResult");
    var schoolResult = schoolResultJson != null
        ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
        : new SchoolResult();

    // 1. Get Subject-wise Top 3 Students (with ties handled)
    var subjectWiseToppers = schoolResult.Students
        .SelectMany(student => student.Subjects, (student, subject) => new
        {
            SubjectCode = subject.Key,
            SubjectName = subject.Value.SubjectName,
            Student = student,
            Marks = subject.Value.Marks,
            Percentage = student.Percentage
        })
        .GroupBy(x => new { x.SubjectCode, x.SubjectName })
        .Select(g => new
        {
            SubjectCode = g.Key.SubjectCode,
            SubjectName = g.Key.SubjectName,
            TopStudents = g
                .OrderByDescending(x => x.Marks)
                .GroupBy(x => x.Marks)
                .Take(3) // Top 3 distinct mark ranges
                .SelectMany(group => group) // Flatten groups (handles ties)
                .ToList()
        })
        .ToList();

    // 2. Counts (same as before)
    int above95Count = schoolResult.Students.Count(s => s.Percentage > 95);
    int above90Count = schoolResult.Students.Count(s => s.Percentage > 90);
    int passCount = schoolResult.Students.Count(s => s.OverallResult.Equals("Pass", StringComparison.OrdinalIgnoreCase));
    int compartmentCount = schoolResult.Students.Count(s => s.OverallResult.Equals("Compartment", StringComparison.OrdinalIgnoreCase));
    int failCount = schoolResult.Students.Count(s => s.OverallResult.Equals("Fail", StringComparison.OrdinalIgnoreCase));

    // 3. Prepare ViewModel (optional: populate subject-wise ranges)
    var viewModel = new SubjectFullSummaryViewModel
    {
        // ... (same as before)
    };

    // Return data
    return PartialView("_FullSubjectSummaryPartial", new
    {
        SubjectWiseToppers = subjectWiseToppers,
        Above95Count = above95Count,
        Above90Count = above90Count,
        PassCount = passCount,
        CompartmentCount = compartmentCount,
        FailCount = failCount,
        Summary = viewModel
    });
}
 * 
 * */



