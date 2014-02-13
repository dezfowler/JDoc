using System.Collections.Generic;
using System.Threading.Tasks;

namespace JDoc.Commands
{
    public interface ISupportPatching
    {
        Task<Result> Patch(PatchCommand command);

        Task<IEnumerable<Result>> Patch(IEnumerable<PatchCommand> commands);
    }
}