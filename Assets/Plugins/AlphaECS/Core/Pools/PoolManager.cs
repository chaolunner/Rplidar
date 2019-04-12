using System.Collections.Generic;
using System.Linq;
using UniRx;
using System;

namespace AlphaECS
{
	public class PoolManager : IPoolManager, IDisposable
    {
        public const string DefaultPoolName = "default";
        
//        private readonly IDictionary<GroupAccessorToken, IEnumerable<IEntity>> _groupAccessors;
        private IDictionary<string, IPool> _pools;

        public IEventSystem EventSystem { get; private set; }
        public IEnumerable<IPool> Pools { get { return _pools.Values; } }
        public IIdentityGenerator IdentityGenerator { get; private set; }

        public PoolManager(IIdentityGenerator identityGenerator, IEventSystem eventSystem)
        {
            IdentityGenerator = identityGenerator;
            EventSystem = eventSystem;
//            _groupAccessors = new Dictionary<GroupAccessorToken, IEnumerable<IEntity>>();
            _pools = new Dictionary<string, IPool>();
            CreatePool(DefaultPoolName);
        }
        
        public IPool CreatePool(string name)
        {
            var pool = new Pool(name, IdentityGenerator, EventSystem);
            _pools.Add(name, pool);

            EventSystem.Publish(new PoolAddedEvent(pool));

            return pool;
        }

        public IPool GetPool(string name = null)
        { return _pools[name ?? DefaultPoolName]; }

        public void RemovePool(string name)
        {
            if(!_pools.ContainsKey(name)) { return; }

            var pool = _pools[name];
            _pools.Remove(name);

            EventSystem.Publish(new PoolRemovedEvent(pool));
        }

        public void Dispose()
        {
//            _groupAccessors.Values.ForEachRun(x =>
//            {
//                if (x is IDisposable)
//                {
//                    (x as IDisposable).Dispose();
//                }
//            });
		}
        
//        public IEnumerable<IEntity> GetEntitiesFor(IGroup group, string poolName = null)
//        {
//            if(group is EmptyGroup)
//            { return new IEntity[0]; }
//
//            if (poolName != null)
//            { return _pools[poolName].Entities.MatchingGroup(group); }
//
//            return Pools.GetAllEntities().MatchingGroup(group);
//			return Pools.GetAllEntities();
//        }

//        public GroupAccessor CreateGroupAccessor(IGroup group, string poolName = null)
//        {
//            var groupAccessorToken = new GroupAccessorToken(group.TargettedComponents.ToArray(), poolName);
//            if (!_groupAccessors.ContainsKey(groupAccessorToken))
//            {
//                var entityMatches = GetEntitiesFor(group, poolName);
//                _groupAccessors.Add(groupAccessorToken, entityMatches);
//            }
//
//            return new GroupAccessor(groupAccessorToken, _groupAccessors[groupAccessorToken]);
//        }
    }
}