using System.Collections.Generic;

namespace AlphaECS
{
    public interface IPool
    {
        string Name { get; }

        IEnumerable<IEntity> Entities { get; }
        IIdentityGenerator IdentityGenerator { get; }

        IEntity CreateEntity(string id = null, IBlueprint blueprint = null);
        void RemoveEntity(IEntity entity);
    }
}
