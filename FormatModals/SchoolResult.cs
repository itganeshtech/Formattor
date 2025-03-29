using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormatModals;

namespace FormatModals
{
    public class SchoolResult
    {
        public string Date { get; set; }
        public string BoardName { get; set; }
        public string Region { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }
        public List<Student> Students { get; set; } = new();
    }
}
