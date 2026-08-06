using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string ActionName { get; }
    int Priority { get; }
    IEnumerator ExecuteInteraction(Movement player);
}
