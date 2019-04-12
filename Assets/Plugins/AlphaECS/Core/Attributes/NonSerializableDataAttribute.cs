using System;

namespace AlphaECS
{
    public class NonSerializableDataAttribute : Attribute
    {
        public NonSerializableDataAttribute() { }
    }

	public class SerializableDataAttribute : Attribute
	{
		public SerializableDataAttribute() { }
	}
}
