using System;

namespace JDoc
{
    public class DocumentReference
    {
        public Guid DocumentId { get; set; }

        public Guid? Revision { get; set; }
    }
}