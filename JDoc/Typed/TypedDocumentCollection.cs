using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JDoc.Typed
{
    public class TypedDocumentCollection<T>
    {
        private readonly IProvider _provider;

        private readonly string _collectionName;

        public TypedDocumentCollection(IProvider provider, string collectionName)
        {
            _provider = provider;
            _collectionName = collectionName;
        }

        public Task<TypedDocument<T>> Load(DocumentIdentifier id)
        {
            return _provider.LoadDocument(id).ContinueWith<TypedDocument<T>>(Deserialize);
        }

        private static TypedDocument<T> Deserialize(Task<Document> arg)
        {
            return new TypedDocument<T>
                       {
                           Meta = arg.Result.Meta,
                           Content = arg.Result.Content.ToObject<T>(),
                       };
        }

        public Task<Document> Store(TypedDocument<T> document)
        {
            var settings = new JsonSerializerSettings();

            var documentReferenceConverter = new DocumentReferenceConverter();
            settings.Converters.Add(documentReferenceConverter);
            
            var jsonSerializer = JsonSerializer.CreateDefault(settings);

            var jdoc = new Document
                           {
                               Meta = document.Meta,
                               Content = JObject.FromObject(document.Content, jsonSerializer),
                           };

            jdoc.References = new List<DocumentReferencePosition>(documentReferenceConverter.References);

            return _provider.StoreDocument(jdoc);
        }
    }
}