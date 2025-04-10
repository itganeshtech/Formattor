using AspNetCoreGeneratedDocument;
using BusinessLogic;
using ClosedXML.Excel;
using FormatModals;
using FormatResult.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
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

            var filePath = Path.Combine(uploadsFolder, uploadedFile.FileName + Guid.NewGuid());
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }
            // Parse the file and get schoolResult
            var schoolResult = Parser.ParseFile(filePath);

            // Serialize the schoolResult object to JSON and store it in TempData
            //TempData["SchoolResult"] = JsonConvert.SerializeObject(schoolResult);

            // System.IO.File.Delete(filePath);

            HttpContext.Session.SetString("SchoolResult", JsonConvert.SerializeObject(schoolResult));

            // Redirect to Success Page
            return RedirectToAction("Success", "FileUpload", new { fileName = HttpUtility.UrlEncode(uploadedFile.FileName) });
        }

        return View("Index");

    }

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

        OverallSummaryViewModel viewModel = GetOverallSummaryViewModelLocal(schoolResult);

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

        FirstToppersViewModel firstToppersViewModel = GetFirstToppersViewModelLocal(schoolResult);

        return PartialView("_TopPerformersPercentWise", firstToppersViewModel);
    }

    //TO DISPLAY PERCENTAGE OF ALL THE STUDENTS
    public IActionResult GetAllStudentsPercent()
    {
        // Deserialize the JSON string back to SchoolResult object
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");

        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult(); // Fallback if nothing is passed

        List <Student> allToppers = GetAllStudentsPercentLocal(schoolResult);

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

        
        List<SubjectWiseResultViewModel> subjectwiseToppers=GetSubjectWiseTopperLocal(schoolResult);

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

        FullMarksViewModel fullMarksViewModel = GetFullMarksViewModelLocal(schoolResult);

        // return PartialView("_FullMarksPartial", subjectWiseCenturions);
        return PartialView("_FullMarksPartial", fullMarksViewModel);
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
                .Where(x => x.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks == 100))
                .ToList(),
                Above95 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks >= 95))
                .ToList(),
                Between90to95 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks > 90 && x.Value.Marks < 95))
                .ToList(),
                Between80n90 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks > 80 && x.Value.Marks <= 90))
                .ToList(),
                Between70n80 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks > 70 && x.Value.Marks <= 80))
                .ToList(),
                Between60n70 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks > 60 && x.Value.Marks <= 70))
                .ToList(),
                Between50n60 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks > 50 && x.Value.Marks <= 60))
                .ToList(),
                Between33n50 = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks >= 33 && x.Value.Marks <= 50))
                .ToList(),
                Pass = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks >= 33))
                .ToList(),
                Fail = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks < 33)
                    && s.OverallResult.Equals("FAIL", StringComparison.OrdinalIgnoreCase))
                .ToList(),
                Compartment = schoolResult.Students
                .Where(s => s.Subjects.Any(x => x.Key == item.SubjectCode && x.Value.Marks < 33)
                    && s.OverallResult.Equals("COMP", StringComparison.OrdinalIgnoreCase))
                .ToList(),
                SubjectWiseToppers = item.TopStudents // Add the toppers data to the view model
            });
        }

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

    public IActionResult DownloadExcel()
    {
        var schoolResultJson = HttpContext.Session.GetString("SchoolResult");
        var schoolResult = schoolResultJson != null
            ? JsonConvert.DeserializeObject<SchoolResult>(schoolResultJson)
            : new SchoolResult();

        OverallSummaryViewModel viewModel = GetOverallSummaryViewModelLocal(schoolResult);
        FirstToppersViewModel firstToppersViewModel = GetFirstToppersViewModelLocal(schoolResult);
        List<Student> allToppers = GetAllStudentsPercentLocal(schoolResult);
        FullMarksViewModel fullMarksViewModel = GetFullMarksViewModelLocal(schoolResult);

       // SubjectFullDetailsViewModel subjectFullDetailsViewModel = GetSubjectFullDetailsViewModelLocal(schoolResult);
      //  SubjectWiseResultViewModel subjectWiseResultViewModel = GetSubjectWiseResultViewModelLocal();

        using (var workbook = new XLWorkbook())
        {
            // Sheet 1: Summary
            var summarySheet = workbook.Worksheets.Add("Summary");
            summarySheet.Cell(1, 1).Value = "Max Percentage";
            summarySheet.Cell(1, 2).Value = viewModel.MaxPercentage;

            summarySheet.Cell(2, 1).Value = "Count > 95%";
            summarySheet.Cell(2, 2).Value = viewModel.CountAbove95;

            summarySheet.Cell(3, 1).Value = "Count > 90%";
            summarySheet.Cell(3, 2).Value = viewModel.CountAbove90;

            summarySheet.Cell(4, 1).Value = "Count Pass";
            summarySheet.Cell(4, 2).Value = viewModel.CountPass;

            summarySheet.Cell(5, 1).Value = "Count Fail";
            summarySheet.Cell(5, 2).Value = viewModel.CountFail;

            summarySheet.Cell(6, 1).Value = "Count Compartment";
            summarySheet.Cell(6, 2).Value = viewModel.CountCompartment;

            // Sheet 2: Toppers
            var toppersSheet = workbook.Worksheets.Add("Toppers");
            toppersSheet.Cell(1, 1).Value = "Name";
            toppersSheet.Cell(1, 2).Value = "Percentage";
            toppersSheet.Cell(1, 3).Value = "Result";

            for (int i = 0; i < viewModel.Toppers.Count; i++)
            {
                var student = viewModel.Toppers[i];
                toppersSheet.Cell(i + 2, 1).Value = student.Name;
                toppersSheet.Cell(i + 2, 2).Value = student.Percentage;
                toppersSheet.Cell(i + 2, 3).Value = student.OverallResult;
            }

            // Sheet 3: Top Performers
            var performersSheet = workbook.Worksheets.Add("Top Performers");
            performersSheet.Cell(1, 1).Value = "Name";
            performersSheet.Cell(1, 2).Value = "Roll Number";
            performersSheet.Cell(1, 3).Value = "Percentage";
            performersSheet.Cell(1, 4).Value = "Subject Name";
            performersSheet.Cell(1, 5).Value = "Subject Code";

            for (int i = 0; i < firstToppersViewModel.FirstToppersViewModelDetails.Count; i++)
            {
                var student = firstToppersViewModel.FirstToppersViewModelDetails[i];
                performersSheet.Cell(i + 2, 1).Value = student.Name;
                performersSheet.Cell(i + 2, 2).Value = student.RollNumber;
                performersSheet.Cell(i + 2, 3).Value = student.Percentage;
                performersSheet.Cell(i + 2, 4).Value = student.SubjectName;
                performersSheet.Cell(i + 2, 5).Value = student.SubjectCode;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "SchoolResultSummary.xlsx");
            }
        }
    }


    #region private methods

    private OverallSummaryViewModel GetOverallSummaryViewModelLocal(SchoolResult schoolResult)
    {
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
            CountFail = schoolResult.Students.Count(s => s.OverallResult.Equals("FAIL", StringComparison.OrdinalIgnoreCase)),
            CountCompartment = schoolResult.Students.Count(s => s.OverallResult.Equals("COMP", StringComparison.OrdinalIgnoreCase)),
            MaxPercentage = maxPercentage
        };

        return viewModel;
    }

    private FirstToppersViewModel GetFirstToppersViewModelLocal(SchoolResult schoolResult)
    {
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
                .Select(s => new FirstToppersViewModelDetails1
                {
                    Name = s.Name,
                    RollNumber = s.RollNumber,
                    Percentage = s.Percentage,
                    Student = s,
                    SubjectCode = "",   //s.Subjects.ToLookup<>"",
                    SubjectName = "Overall Result"
                }))
            .ToList();

        FirstToppersViewModel firstToppersViewModel = new FirstToppersViewModel()
        {
            Percentages = topPercentages.ToList(),
            FirstToppersViewModelDetails = result
        };

        return firstToppersViewModel;
    }

    private FullMarksViewModel GetFullMarksViewModelLocal(SchoolResult schoolResult)
    {

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
        var groupViewModel = subjectWiseCenturions.GroupBy(x => x.RollNumber).ToList();

        return groupViewModel;
    }


    private List<Student> GetAllStudentsPercentLocal(SchoolResult schoolResult)
    {
        // Calculating percentage of all students
        var allToppers = schoolResult.Students.OrderByDescending(x => x.Percentage).ToList();
        return allToppers;
    }

    private SubjectWiseResultViewModel GetSubjectWiseTopperLocal(SchoolResult schoolResult)
    {
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
                return group.Where(s => topMarks.Contains(s.Marks))
                .OrderByDescending(s => s.Marks);
            })
            .ToList();

        return subjectwiseToppers;
    }

    //private SubjectFullDetailsViewModel GetSubjectFullDetailsViewModelLocal(SchoolResult schoolResult)
    //{
    //    return;
    //}




    #endregion
}