using System.Text.RegularExpressions;
using FormatModals;

namespace BusinessLogic
{
    public class Parser
    {
        private static readonly Dictionary<string, string> SubjectLookup = new()
        {
                {"001", "English (Core)"},    //Core Subjects
                {"002", "Hindi (Core)"},
                {"003", "Urdu (Core)"},
                {"041", "Mathematics"},
                {"042", "Physics"},
                {"043", "Chemistry"},
                {"044", "Biology"},
                {"048", "Physical Education"},
                {"049", "Painting"},
                {"054", "Business Studies"},
                {"055", "Accountancy"},
                {"064", "Home Science"},
                {"065", "Informatics Practices (IP)"},
                {"066", "Computer Science (CS)"},
                {"073", "Psychology"},
                {"083", "Computer Science (New)"},
                {"301", "English Elective"},
                {"027", "History"},

                {"028", "Political Science"}, //Elective subjects
                {"029", "Geography"},
                {"030", "Economics"},
                {"031", "Carnatic Music (Vocal)"},
                {"032", "Carnatic Music (Melodic)"},
                {"033", "Carnatic Music (Percussion)"},
                {"034", "Hindustani Music (Vocal)"},
                {"035", "Hindustani Music (Melodic)"},
                {"036", "Hindustani Music (Percussion)"},
                {"037", "Bharatnatyam (Dance)"},
                {"039", "Kathak (Dance)"},
                {"050", "Graphics"},
                {"052", "Sculpture"},
                {"056", "Entrepreneurship"},
                {"062", "Sociology"},
                {"067", "Biotechnology"},
                {"082", "Multimedia & Web Tech"},
                
                {"606", "Food Nutrition & Dietetics"}, //Skill-Based or Vocational Subjects
                {"608", "Fashion Studies"},
                {"618", "Marketing"},
                {"802", "Information Technology"},
                {"803", "Web Applications"},
                {"837", "Artificial Intelligence"},

                {"022", "Sanskrit (Core)"},    //Language Subjects
                {"104", "French"},
                {"105", "German"},
                {"106", "Spanish"},
                {"107", "Japanese"},
                {"108", "Russian"},
                {"109", "Persian"},
                {"110", "Nepali"}  
        };

        public static SchoolResult ParseFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            var school = new SchoolResult();
            Student currentStudent = null;

            //foreach (var line in lines)
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];

                if (line.Contains("DATE:-"))
                {
                    var match = Regex.Match(line, @"DATE:-\s+(\d{2}/\d{2}/\d{4})");
                    if (match.Success) school.Date = match.Groups[1].Value;
                }
                else if (line.Contains("C.B.S.E."))
                {
                    school.BoardName = "C.B.S.E.";
                }
                else if (line.Contains("REGION:"))
                {
                    var match = Regex.Match(line, @"REGION:\s+([A-Z ]+)");
                    if (match.Success) school.Region = match.Groups[1].Value.Trim();
                }
                else if (line.Contains("SCHOOL : -"))
                {
                    var match = Regex.Match(line, @"SCHOOL : -\s+(\d+)\s+(.+)");
                    if (match.Success)
                    {
                        school.SchoolCode = match.Groups[1].Value;
                        school.SchoolName = match.Groups[2].Value.Trim();
                    }
                }
                else if (Regex.IsMatch(line, "\\d{8}\\s+[MF]"))
                {
                    var match = Regex.Match(line, @"(\d{8})\s+([MF])\s+(.+?)\s+((\d{3}\s+)+)([A-Z0-9 ]+)\s+(PASS|FAIL)");
                    if (match.Success)
                    {
                        currentStudent = new Student
                        {
                            RollNumber = match.Groups[1].Value,
                            Gender = match.Groups[2].Value,
                            Name = match.Groups[3].Value.Trim(),
                            OverallResult = match.Groups[7].Value
                        };

                        school.Students.Add(currentStudent);

                        ParseSubjects(currentStudent, line, lines, i);
                    }
                }
            }
            return school;
        }

        private static void ParseSubjects(Student student, string subjectData, string[] lines, int lineIndex)
        {
            string nextLine = lines[lineIndex + 1];
            string pattern = @"\b\d{3}\b"; // Match exactly 3 digits (subject codes)

            // Find matches in the input string
            var matches = Regex.Matches(subjectData, pattern);

            int marksIndex = 66;
            foreach (Match match in matches)
            {
                if (SubjectLookup.ContainsKey(match.Value))
                {
                    student.Subjects[match.Value] = (SubjectLookup[match.Value], int.Parse(nextLine.Substring(marksIndex, 3).Trim()), nextLine.Substring(marksIndex + 4, 3).Trim());
                    marksIndex += 8;
                }

            }
        }
    }
}
