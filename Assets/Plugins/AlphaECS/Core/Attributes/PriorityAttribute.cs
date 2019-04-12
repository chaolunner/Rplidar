using System;

namespace AlphaECS
{
	[AttributeUsage(AttributeTargets.Class)]
	public class PriorityAttribute : Attribute
	{
		public int Priority { get; set; }

		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}
	}
}