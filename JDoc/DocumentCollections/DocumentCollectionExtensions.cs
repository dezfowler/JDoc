using JDoc.Typed;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JDoc.DocumentCollections
{
    public static class DocumentCollectionExtensions
    {
        public static Task<Document> CreateCollection(this IProvider provider, string name, CollectionSettings settings)
        {
            var typed = new TypedClient(provider);
            
            //typed.Put<CollectionData>

            var content = new Dictionary<string, object>
            {
                { "$type", "collection" },
                { "settings", settings },
            };

            var things = JObject.FromObject(content);

            var collectionDocument = new Document
            {
                Meta = new DocumentMeta
                {
                    Name = name,
                },

                Content = things,
            };

            return provider.StoreDocument(collectionDocument);
        }

        public static Task<DocumentCollection> OpenCollection(this IProvider provider, string name)
        {
            var collectionDocument = provider.LoadDocument("name").Result;
            var collectionMetaContent = collectionDocument.Content;
            //collectionMetaContent.ToObject<
            throw new NotImplementedException();
        }
    }

    public class DocumentCollection //: IContainDocuments
    {

    }
}
