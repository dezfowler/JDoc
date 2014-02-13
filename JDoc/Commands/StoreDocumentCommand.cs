using System;
using System.Threading.Tasks;

namespace JDoc.Commands
{
    public class StoreDocumentCommand : Command<Document>
    {
        public override CommandType Type
        {
            get { return CommandType.StoreDocument; }
        }

        public string CollectionName { get; set; }

        public Document Document { get; set; }

        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override Document HandleResult(Task<Result> task)
        {
            return ((DocumentResult)task.Result).Document;
        }
    }
}