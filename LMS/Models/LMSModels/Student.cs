using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Student
    {
        public Student()
        {
            Enrolleds = new HashSet<Enrolled>();
            Submissions = new HashSet<Submission>();
        }

        public int UId { get; set; }
        public string FName { get; set; } = null!;
        public string LName { get; set; } = null!;
        public DateOnly Dob { get; set; }
        public int DId { get; set; }

        public virtual Department DIdNavigation { get; set; } = null!;
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
