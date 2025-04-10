using FormatModals;

namespace FormatResult.Models
{
    public class FirstToppersViewModel
    {
        public List<double> Percentages;
        public List<FirstToppersViewModelDetails1> FirstToppersViewModelDetails;
    }

    public class FirstToppersViewModelDetails1
    {
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string RollNumber { get; set; }
        public string Name { get; set; }
        public double Percentage { get; set; }
        public Student Student { get; set; }
    }
}
