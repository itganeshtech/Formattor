using System.Text.RegularExpressions;
using FormatModals;

namespace BusinessLogic
{
    public class Parser
    {
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
                if (SubjectMaster.SubjectLookup.ContainsKey(match.Value))
                {
                    student.Subjects[match.Value] = (SubjectMaster.SubjectLookup[match.Value], int.Parse(nextLine.Substring(marksIndex, 3).Trim()), nextLine.Substring(marksIndex + 4, 3).Trim());
                    marksIndex += 8;
                }

            }
        }
    }
}
