using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using JDoc.DocumentCollections;
using Newtonsoft.Json.Schema;
using System.Globalization;

namespace JDoc.Typed
{
    public static class TypedCollectionProviderExtensions
    {
        public static TypedDocumentCollection<T> CreateCollection<T>(this IProvider provider, string name, CollectionSettings settings)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (settings == null) throw new ArgumentNullException("settings");
            if (settings.Schema != null) throw new ArgumentException("Schema must be null");

            settings = JObject.FromObject(settings).ToObject<CollectionSettings>();
            settings.Schema = new JsonSchemaGenerator().Generate(typeof(T));
            
            var collectionDoc = provider.CreateCollection(name, settings).Result;

            return new TypedDocumentCollection<T>(collectionDoc);
        }

        public static TypedDocumentCollection<T> OpenCollection<T>(this IProvider provider, string name)
        {
            var collectionDoc = provider.OpenCollection(name).Result;

            return new TypedDocumentCollection<T>(collectionDoc);
        }
    }

    public class TypedDocumentCollection<T> : DocumentCollection
    {
        public TypedDocumentCollection(DocumentCollection collection) : base(collection.provider, collection.collectionRoot)
        {
        }

        public override Task<Result> ExecuteCommand(Command command)
        {
            var visitor = new TypedCollectionCommandVisitor(this);
            return command.Accept(visitor);
        }       
        
        internal class TypedCollectionCommandVisitor : DocumentCollection.DocumentCollectionCommandVisitor
        {
            public TypedCollectionCommandVisitor(TypedDocumentCollection<T> documentCollection) : base(documentCollection)
            {
            }

            public override Task<Result> Visit(Commands.LoadDocumentCommand loadDocumentCommand)
            {
                // TODO: Need to only get documents within collection - how to do this?
                // 1. Substring of document name e.g. {collection}/{document}
                // 2. Document reference to collection document
         
                if (!loadDocumentCommand.Id.Actual.HasValue)
                {
                    var collectionScopedIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.documentCollection.collectionRoot.Meta.Name, loadDocumentCommand.Id.Friendly);

                    loadDocumentCommand = new Commands.LoadDocumentCommand
                    {
                        Id = new DocumentIdentifier(collectionScopedIdentifier, loadDocumentCommand.Id.Revision)
                    };
                }
                
                return base.Visit(loadDocumentCommand)
                        .ContinueWith(t =>
                        {
                            var doc = loadDocumentCommand.HandleResult(t);

                            if (!doc.References.Any(r => r.Path == "$collection" 
                                && r.Reference.DocumentId == this.documentCollection.collectionRoot.Meta.Id 
                                && r.Reference.Revision == this.documentCollection.collectionRoot.Meta.RevisionEtag))
                                throw new ProviderException("Document not within specified collection");

                            return t.Result;
                        });
            }

            public override Task<Result> Visit(Commands.StoreDocumentCommand storeDocumentCommand)
            {
                storeDocumentCommand.Document.Content.Validate(this.documentCollection.collectionRoot.Content.Settings.Schema);
                return base.Visit(storeDocumentCommand);
            }
        }
    }
}