using FormatModals;

namespace FormatResult.Models
{
    public class OverallSummaryViewModel
    {
        public SchoolResult SchoolResult { get; set; } = new();
        public List<Student> Toppers { get; set; } = new();
        public int CountAbove95 { get; set; }
        public int CountAbove90 { get; set; }
        public int CountPass { get; set; }
        public int CountFail { get; set; }
        public int CountCompartment { get; set; }
        public double MaxPercentage { get; set; }


        //List<Student> students;
        //int countAbove95 = 0;
        //int countAbove90 = 0;   
        //int countPass = 0;
        //int countFail = 0;
        //int countCompartment = 0;
        //private List<Student> toppers;
        //private double maxPercentage;

        //public OverallSummaryViewModel(List<Student> toppers, int countAbove95, int countAbove90, int countPass, int countFail, int countCompartment, double maxPercentage)
        //{
        //    this.toppers = toppers;
        //    this.countAbove95 = countAbove95;
        //    this.countAbove90 = countAbove90;
        //    this.countPass = countPass;
        //    this.countFail = countFail;
        //    this.countCompartment = countCompartment;
        //    this.maxPercentage = maxPercentage;
        //}
    }

}
