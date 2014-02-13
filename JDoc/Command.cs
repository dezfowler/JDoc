using JDoc.Commands;
using System.Threading.Tasks;

namespace JDoc
{
    public abstract class Command
    {
        public abstract CommandType Type { get; }

        public abstract Task<Result> Accept(ICommandVisitor visitor);
    }
}