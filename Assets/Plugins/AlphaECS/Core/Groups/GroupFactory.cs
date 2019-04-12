using System;
using System.Collections.Generic;
using AlphaECS;
using Zenject;
using UniRx;
using System.Linq;

namespace AlphaECS
{
//	public class GroupFactory : Factory<Type[], Group>
	public class GroupFactory
    {
		[Inject] protected DiContainer Container = null;
//		protected Dictionary<HashSet<Type>, Group> Groups = new Dictionary<HashSet<Type>, Group>();

		private HashSet<Type> types;
        private List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> predicates = new List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> ();

		public Group Create(Type[] _types)
		{
			this.types = new HashSet<Type>(_types);
			return this.Create ();
		}

		public Group Create(HashSet<Type> _types)
		{
			this.types = _types;
			return this.Create ();
		}

		public Group Create()
		{
//			var hashSet = new HashSet<Type> (types);
//			foreach (var key in Groups.Keys)
//			{
//				if (hashSet.SetEquals(key) && predicates.Count == 0)
//				{
//					this.types = null;
//					this.predicates.Clear();
//					return Groups [key];
//				}
//			}
				
			var group = new Group (types, predicates);
			Container.Inject (group);

			types = null;
			predicates.Clear();
//			Groups.Add (hashSet, group);
			return group;
		}

//		public GroupFactory AddTypes(Type[] _types)
//		{
//			this.types = _types;
//			return this;
//		}

        public GroupFactory WithPredicates(params Func<IEntity, IReadOnlyReactiveProperty<bool>>[] predicates)
		{
            foreach(var predicate in predicates)
            {
                this.predicates.Add(predicate);
            }
            return this;
		}
    }
}