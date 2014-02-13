using JDoc.Typed;
using JDoc.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JDoc
{
    public static class IProviderExtensions
    {
        public static Task<Document> LoadDocument(this IProvider provider, DocumentIdentifier id)
        {
            var command = new LoadDocumentCommand
                              {
                                  Id = id,
                              };

            return provider
                .ExecuteCommand(command)
                .ContinueWith<Document>(command.HandleResult);
        }

        public static Task<Document> StoreDocument(this IProvider provider, Document document)
        {
            var command = new StoreDocumentCommand
                              {
                                  Document = document,
                              };

            return provider
                .ExecuteCommand(command)
                .ContinueWith<Document>(command.HandleResult);
        }

        public static Task<Result> Patch(this IProvider provider, PatchCommand command)
        {
            // TODO: Revisit this...

            var remotePatch = provider as ISupportPatching;

            if (remotePatch != null) return remotePatch.Patch(command);

            return command.RunLocal(provider);
        }
    }
}