using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using JDoc.DocumentCollections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JDoc.Typed
{
    public static class TypedProviderExtensions
    {
        public static Task<TypedDocument<T>> LoadObject<T>(this IProvider provider, DocumentIdentifier id)
        {
            Func<Task<Document>, TypedDocument<T>> continuation = task => Deserialize<T>(task.Result);
            return provider.LoadDocument(id).ContinueWith<TypedDocument<T>>(continuation);
        }

        public static Task<TypedDocument<T>> StoreObject<T>(this IProvider provider, TypedDocument<T> typedDoc)
        {
            var doc = Serialize<T>(typedDoc);
            Func<Task<Document>, TypedDocument<T>> continuation = task => Deserialize<T>(task.Result);
            return provider.StoreDocument(doc).ContinueWith<TypedDocument<T>>(continuation);
        }

        internal static Document Serialize<T>(TypedDocument<T> typedDoc)
        {
            // TODO: This serializer stuff for document references should be in standard typed doc serializer
            var settings = new JsonSerializerSettings();

            var documentReferenceConverter = new DocumentReferenceConverter();
            settings.Converters.Add(documentReferenceConverter);

            var jsonSerializer = JsonSerializer.CreateDefault(settings);

            var jdoc = new Document
            {
                Meta = typedDoc.Meta,
                Content = JObject.FromObject(typedDoc.Content, jsonSerializer),

            };

            jdoc.References = new List<DocumentReferencePosition>(documentReferenceConverter.References);

            return jdoc;
        }

        internal static TypedDocument<T> Deserialize<T>(Document document)
        {
            return new TypedDocument<T>
            {
                Meta = document.Meta,
                Content = document.Content.ToObject<T>(),
            };
        }
    }
}