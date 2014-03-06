using JDoc.Commands;
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
        public static Task<DocumentCollection> CreateCollection(this IProvider provider, string name, CollectionSettings settings)
        {
            var content = new CollectionMeta
            {
                Settings = settings,
            };

            var collectionDocument = new TypedDocument<CollectionMeta>
            {
                Meta = new DocumentMeta
                {
                    Name = name,
                },

                Content = content,
            };

            return provider
                .StoreObject(collectionDocument)
                .ContinueWith<DocumentCollection>(t => new DocumentCollection(provider, t.Result));
        }

        public static Task<DocumentCollection> OpenCollection(this IProvider provider, string name)
        {
            // TODO: 
            return provider
                .LoadObject<CollectionMeta>(name)
                .ContinueWith(t => new DocumentCollection(provider, t.Result));
        }
    }

    public class DocumentCollection : IProvider//, IContainDocuments
    {
        internal readonly IProvider provider;
        internal readonly TypedDocument<CollectionMeta> collectionRoot;

        internal DocumentCollection(IProvider provider, TypedDocument<CollectionMeta> collectionRoot)
        {
            this.provider = provider;
            this.collectionRoot = collectionRoot;
        }

        public virtual Task<Result> ExecuteCommand(Command command)
        {
            var visitor = new DocumentCollectionCommandVisitor(this);
            return command.Accept(visitor);
        }

        public Task<IEnumerable<Result>> ExecuteBatch(IEnumerable<Command> commands)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommandType> SupportedCommands
        {
            get { return new[] { CommandType.LoadDocument, CommandType.PatchDocument, CommandType.QueryDocument, CommandType.StoreDocument }; }
        }
                
        internal class DocumentCollectionCommandVisitor : ICommandVisitor
        {
            internal readonly DocumentCollection documentCollection;

            public DocumentCollectionCommandVisitor(DocumentCollection documentCollection)
            {
                this.documentCollection = documentCollection;
            }

            public virtual Task<Result> Visit(StoreDocumentCommand storeDocumentCommand)
            {
                var collectionReference = new DocumentReferencePosition
                {
                    Path = "$collection",
                    Reference = new DocumentReference
                    {
                        DocumentId = this.documentCollection.collectionRoot.Meta.Id,
                        Revision = this.documentCollection.collectionRoot.Meta.RevisionEtag,
                    }
                };

                storeDocumentCommand.Document.References.Add(collectionReference);

                return documentCollection.provider.ExecuteCommand(storeDocumentCommand);
            }

            public virtual Task<Result> Visit(LoadDocumentCommand loadDocumentCommand)
            {
                // TODO: Need to query by ID and document reference
                throw new NotImplementedException();
            }

            public Task<Result> Visit(PatchCommand patchCommand)
            {
                throw new NotImplementedException();
            }

            public Task<Result> Visit(QueryCommand queryCommand)
            {
                throw new NotImplementedException();
            }
        }
    }
}
