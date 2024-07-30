using System;
using System.Collections.Generic;

namespace MFractor.Data.Repositories
{
    public interface IEntityIndex<TEntity> where TEntity : Entity
    {
        void Clear();

        void OnInserted(TEntity entity);
        void OnUpdated(TEntity before, TEntity after);
        void OnDeleted(int entityPrimaryKey);
        void OnDeleted(IReadOnlyList<int> primaryKeys);
    }
}
