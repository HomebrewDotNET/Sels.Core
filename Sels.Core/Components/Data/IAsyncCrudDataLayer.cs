using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Components.Data
{
    public interface IAsyncCrudDataLayer<TObject, TId>
    {
        Task Create(TObject objectToPersist);
        Task<TObject> Get(TId id);
        Task<IEnumerable<TObject>> GetAll();
        Task Update(TObject objectToPersist);
        Task Delete(TId id);

    }
}
