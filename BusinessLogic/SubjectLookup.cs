using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class SubjectMaster
    {
        public static readonly Dictionary<string, string> SubjectLookup = new()
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
    }
}
