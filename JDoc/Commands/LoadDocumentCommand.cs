using System;
using System.Threading.Tasks;

namespace JDoc.Commands
{
    public class LoadDocumentCommand : Command<Document>
    {
        public override Document HandleResult(Task<Result> task)
        {
            // not doing anything special here at the moment
            return ((DocumentResult)task.Result).Document;
        }

        public override CommandType Type
        {
            get { return CommandType.LoadDocument; }
        }

        public DocumentIdentifier Id { get; set; }

        public Guid? Revision { get; set; }
        
        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}