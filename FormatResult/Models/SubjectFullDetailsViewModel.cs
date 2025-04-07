using FormatModals;

namespace FormatResult.Models
{
    public class SubjectFullDetailsViewModel
    {
        public string SubjectName;
        public string SubjectCode;
        public List<Student> FullMarks;
        public List<Student> Above95;
        public List<Student> Above90;
        public List<Student> Between80n90;
        public List<Student> Between70n80;
        public List<Student> Between60n70;
        public List<Student> Between50n60;
        public List<Student> Between33n50;
        public List<Student> Pass;
        public List<Student> Fail;
        public List<Student> Compartment;
        public dynamic SubjectWiseToppers; // Add this property
    }
}



//public class SubjectFullSummaryViewModel
//{
//    public string SubjectName;
//    public string SubjectCode;
//    public List<Student> FullMarks;
//    public List<Student> Above90;
//    public List<Student> Between80n90;
//    public List<Student> Between70n80;
//    public List<Student> Between60n70;
//    public List<Student> Between50n60;
//    public List<Student> Pass;
//    public List<Student> Fail;
//    public List<Student> Compartment;
//}