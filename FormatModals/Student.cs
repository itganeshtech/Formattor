using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatModals
{
    public class Student
    {
        public string RollNumber { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string OverallResult { get; set; }
        public Dictionary<string, (string SubjectName, int Marks, string Grade)> Subjects { get; set; } = new();
    }
}
