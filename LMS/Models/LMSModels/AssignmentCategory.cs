﻿using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public int AcId { get; set; }
        public string Name { get; set; } = null!;
        public sbyte Weight { get; set; }
        public int ClassId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
