using System;
using System.Collections;
using System.Collections.Generic;
using AlphaECS;
using AlphaECS.Unity;
using UnityEngine;

public class BlueprintBase : ScriptableObject, IBlueprint
{
    public virtual void Apply(IEntity entity) { }
}
