using System;

namespace JDoc.Client
{
    public class RevisionConflictResult : SingleResult
    {
        public RevisionConflictResult() : base(false)
        {
        }

        public Guid ExpectedRevision { get; set; }

        public Guid ActualRevision { get; set; }
    }
}