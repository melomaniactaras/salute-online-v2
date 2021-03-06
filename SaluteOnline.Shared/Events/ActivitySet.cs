﻿using SaluteOnline.Shared.Common;

namespace SaluteOnline.Shared.Events
{
    public class ActivitySet
    {
        public string SubjectId { get; set; }
        public int UserId { get; set; }
        public ActivityType Type { get; set; }
        public ActivityImportance Importance { get; set; }
        public string Data { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
