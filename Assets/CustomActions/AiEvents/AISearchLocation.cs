using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AISearchLocation", menuName = "AiActions/AISearchLocation")]
public class AISearchLocation : AiEvent
{
    public Event SearchedEvent;
    public override void Activate()
    {
        base.Activate();
        EventManager.instance.StartEvent(SearchedEvent);
        NotifyManager.instance.SetPermanentNotify(eventName);
    }
}
