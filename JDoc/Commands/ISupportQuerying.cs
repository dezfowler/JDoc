using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JDoc.Commands
{
    public interface ISupportQuerying
    {
        Task<IEnumerable<Document>> Query(Expression<Document> query);
    }
}