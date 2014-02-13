using System.Threading.Tasks;

namespace JDoc
{
    public abstract class Command<TResult> : Command
    {
        public abstract TResult HandleResult(Task<Result> task);
    }
}