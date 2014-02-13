using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JDoc.Commands
{
    public class QueryCommand : Command<IEnumerable<Document>>
    {
        public override IEnumerable<Document> HandleResult(Task<Result> task)
        {
            return ((MultiDocumentResult)task.Result).Documents;
        }

        public override CommandType Type
        {
            get { return CommandType.QueryDocument; }
        }

        public string CollectionName { get; set; }

        public Expression<JObject> Query { get; set; }

        public override Task<Result> Accept(ICommandVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}