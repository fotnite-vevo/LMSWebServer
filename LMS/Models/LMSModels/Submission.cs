using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public int UId { get; set; }
        public int AId { get; set; }
        public DateTime Time { get; set; }
        public int Score { get; set; }
        public string Contents { get; set; } = null!;

        public virtual Assignment AIdNavigation { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
