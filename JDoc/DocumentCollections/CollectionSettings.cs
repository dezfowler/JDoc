using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JDoc.DocumentCollections
{
    public class CollectionSettings
    {
        public Revisions MaximumRevisions { get; set; }

        public JsonSchema Schema { get; set; }

        public ReferenceOption ReferencePolicy { get; set; }
    }
}
