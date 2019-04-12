using System.Collections;

namespace AlphaECS
{
	public interface ISystem
	{
		IEventSystem EventSystem { get; set; }
		IPoolManager PoolManager { get; set; }
		GroupFactory GroupFactory { get; set; }
		void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory);
	}
}