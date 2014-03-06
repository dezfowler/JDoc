using System;
using System.Threading.Tasks;

namespace JDoc.Commands
{
    public class LoadDocumentCommand : Command<Document>
    {
        public override Document HandleResult(Task<Result> task)
        {
            var documentResult = task.Result as DocumentResult;
            if (documentResult != null)
            {
                return documentResult.Document;
            }

            return HandleOtherResults(task.Result);
        }

        public override CommandType Type
        {
            get { return CommandType.LoadDocument; }
        }

        public DocumentIdentifier Id { get; set; }

        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}