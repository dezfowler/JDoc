using System.Threading.Tasks;

namespace JDoc
{
    public abstract class Command<TResult> : Command
    {
        public abstract TResult HandleResult(Task<Result> task);

        protected TResult HandleOtherResults(Result result)
        {
            if (result == null) throw new UnexpectedResult(result);

            if (result.Success == false)
            {
                throw new ProviderException(result);
            }
            
            throw new UnexpectedResult(result);
        }
    }
}