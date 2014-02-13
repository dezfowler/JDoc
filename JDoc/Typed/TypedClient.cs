using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using JDoc.DocumentCollections;

namespace JDoc.Typed
{
    public class TypedClient
    {
        private readonly IProvider _provider;

        public TypedClient(IProvider provider)
        {
            _provider = provider;
        }

        //public TypedDocument<T> Get<T>(DocumentIdentifier id)
        //{

        //}


        public TypedDocumentCollection<T> CreateCollection<T>(string name, CollectionSettings settings)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (settings == null) throw new ArgumentNullException("settings");
            if (settings.Schema != null) throw new ArgumentException("Schema must be null");

            settings = JObject.FromObject(settings).ToObject<CollectionSettings>();
            settings.Schema = new JsonSchemaGenerator().Generate(typeof(T));

            _provider.CreateCollection(name, settings).Wait();

            return new TypedDocumentCollection<T>(_provider, name);
        }

        //public TypedDocumentCollection<T> OpenCollection(string name)
        //{
        //    return new TypedDocumentCollection<T>(_provider.OpenCollection(name));
        //} 
    }
}