using System;
using System.Collections.Generic;
using AlphaECS;
using UniRx;
using System.Collections;

namespace AlphaECS
{
	public interface IGroup
    {
		IEventSystem EventSystem { get; set; }
		IPool EntityPool { get; set; }
		string Name { get; set; }
		ReactiveCollection<IEntity> Entities { get; set; }

		HashSet<Type> Components { get; set; }
        List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> Predicates { get; }
    }
}