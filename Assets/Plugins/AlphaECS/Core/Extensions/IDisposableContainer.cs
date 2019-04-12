using UniRx;
using System;

namespace AlphaECS
{
	public interface IDisposableContainer
	{
		CompositeDisposable Disposer { get; }
	}

	public static partial class DisposableExtensions
	{
		public static IDisposable AddTo(this IDisposable disposable, IDisposableContainer container)
		{
			if (container.Disposer.IsDisposed)
			{
				throw new Exception(string.Format("IDisposable on {0} object is already disposed", container.GetType().Name));
			}
			container.Disposer.Add(disposable);
			return disposable;
		}
	}
}
