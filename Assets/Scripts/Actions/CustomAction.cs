using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class CustomAction : ScriptableObject
{
    public string ActionName;
    public abstract void Use();
    public abstract void Use(Item item);
    public abstract void UseByCamp(Camps camp);

    public CustomAction() { }
}
