using System.Collections.Generic;

namespace AlphaECS
{
    public interface IPoolManager
    {
        IEnumerable<IPool> Pools { get; }
        IIdentityGenerator IdentityGenerator { get; }

//        IEnumerable<IEntity> GetEntitiesFor(IGroup group, string poolName = null);
//        GroupAccessor CreateGroupAccessor(IGroup group, string poolName = null);

        IPool CreatePool(string name);
        IPool GetPool(string name = null);
        void RemovePool(string name);
    }
}