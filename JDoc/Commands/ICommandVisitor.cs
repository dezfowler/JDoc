using System.Threading.Tasks;

namespace JDoc.Commands
{
    public interface ICommandVisitor
    {
        Task<Result> Visit(StoreDocumentCommand storeDocumentCommand);

        Task<Result> Visit(LoadDocumentCommand loadDocumentCommand);

        Task<Result> Visit(PatchCommand patchCommand);

        Task<Result> Visit(QueryCommand queryCommand);
    }
}