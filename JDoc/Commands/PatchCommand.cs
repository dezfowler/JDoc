using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JDoc.Commands
{
    public class PatchCommand : Command
    {
        public override CommandType Type
        {
            get { return CommandType.PatchDocument; }
        }

        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public DocumentIdentifier Id { get; set; }

        public string Path { get; set; }

        public PatchCommandType PatchType { get; set; }

        public JToken Value { get; set; }

        public Task<Result> RunLocal(IProvider provider)
        {
            var doc = provider.LoadDocument(Id).Result;
            var token = doc.Content.SelectToken(Path);

            switch(PatchType)
            {
                case PatchCommandType.Add:
                    ((JContainer)token).Add(Value);
                    break;

                case PatchCommandType.Remove:
                    token.Remove();
                    break;

                case PatchCommandType.Replace:
                    token.Replace(Value);
                    break;

                default:
                    throw new InvalidOperationException("Invalid Type");
            }

            return provider.ExecuteCommand(new StoreDocumentCommand { Document = doc });
        }
    }
}