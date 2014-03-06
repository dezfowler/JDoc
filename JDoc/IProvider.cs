using System.Collections.Generic;
using System.Threading.Tasks;

namespace JDoc
{
    public interface IProvider
    {
        Task<Result> ExecuteCommand(Command command);

        Task<IEnumerable<Result>> ExecuteBatch(IEnumerable<Command> commands);

        IEnumerable<CommandType> SupportedCommands { get; }
    }

    // TODO
    public interface IContainDocuments
    {
        void Add(Document document);

        void Update(Document document);

        void Delete(Document document);

        Document Load(DocumentIdentifier id);
    }

    public interface IContainTypedDocuments<TContent>
    {

    }
}