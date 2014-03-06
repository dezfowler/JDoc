using System;
using System.Collections.Generic;

namespace JDoc
{
    public class DocumentMeta
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid RevisionEtag { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string[] Tags { get; set; }

        public DocumentMeta Clone()
        {
            return new DocumentMeta
                       {
                           Id = Id,
                           Name = Name,
                           RevisionEtag = RevisionEtag,
                           Created = Created,
                           Modified = Modified,
                           Tags = Tags == null ? null : new List<string>(Tags).ToArray(),
                       };
        }
    }
}