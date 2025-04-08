namespace FormatResult.Models
{
    public class FullMarksViewModel
    {
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string RollNumber { get; set; }
        public string Name { get; set; }
        public double Marks { get; set; }
        public double Percentage {  get; set; }
        public string Gender { get; set; }
        public string OverallResult { get; set; }
        public Dictionary<string, (string SubjectName, int Marks, string Grade)> Subjects { get; set; } = new();

    }
}
