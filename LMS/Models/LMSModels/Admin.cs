using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Admin
    {
        public int UId { get; set; }
        public string FName { get; set; } = null!;
        public string LName { get; set; } = null!;
        public DateOnly Dob { get; set; }
    }
}
