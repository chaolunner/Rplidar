using System;

namespace AlphaECS.Unity.Editor
{
    public interface IEditorInput
    {
        bool HandlesType(Type type);
        object CreateUI(string label, object value);
    }
}