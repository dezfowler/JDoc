using System.Collections.Generic;

using JDoc.Typed;

using Newtonsoft.Json.Linq;

namespace JDoc
{
    public class Document
    {
        public DocumentMeta Meta { get; set; }

        public JObject Content { get; set; }

        /// <summary>
        /// References to other documents which appear in this document
        /// </summary>
        /// <remarks>
        /// References are to be stored separately from documents and are intended 
        /// to allow quick lookups such as find documents referencing this document
        /// and including referenced documents when retrieving a "parent" document.
        /// </remarks>
        public IList<DocumentReferencePosition> References { get; set; }

        public Document Clone(bool cloneContent = true)
        {
            return new Document
            {
                Meta = Meta.Clone(),
                Content = cloneContent ? (JObject)Content.DeepClone() : null,
                References = new List<DocumentReferencePosition>(References),
            };
        }
    }
}
