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

        private double _percentage = double.MinValue;
        public double Percentage
        {
            get
            {
                if (_percentage == double.MinValue)
                {
                    if (Subjects != null && Subjects.Count > 0)
                    {
                        /*
                        int sum = 0;
                        foreach (var item in Subjects)
                        {
                            sum += item.Value.Marks;
                        }
                        */

                        int sum = Subjects.Sum(x => x.Value.Marks);
                        _percentage = sum / Subjects.Count;
                    }
                }

                return _percentage;
            }
        }

        public StudyStreams Stream
        {
            get
            {
                StudyStreams result = StudyStreams.None;

                if (Subjects.Count > 0)
                {
                    // check for scie nce subject codes

                    // 
                }

                return result;
            }
        }
    }

    public enum StudyStreams
    {
        None,
        Science,
        Arts,
        Commerce
    }
}
