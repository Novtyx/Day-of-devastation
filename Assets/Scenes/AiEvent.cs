using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AiEvent : ScriptableObject
{
    public string eventName;
    public int category;
    public float threatCost;
    [Range(0, 1)] public float minPower;
    public float baseWeight;

    public AiEvent nextEvent;
    public float nextEventDelay = 10f;

    public virtual float CalculateWeight(List<ActionRecord> actions)
    {
        return baseWeight;
    }


    public virtual void Activate()
    {
    }
}
