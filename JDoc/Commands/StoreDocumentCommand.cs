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

        public Document Document { get; set; }

        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override Document HandleResult(Task<Result> task)
        {
            var documentResult = task.Result as DocumentResult;
            if (documentResult != null)
            {
                return documentResult.Document;
            }

            return HandleOtherResults(task.Result);
        }
    }
}