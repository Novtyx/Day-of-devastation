using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AiMedicineEvent", menuName = "AiActions/AiMedicineEvent")]
public class AiMedicineEvent : AiEvent
{
    public Enemy enemy;
    public override void Activate()
    {
        base.Activate();
        EventManager.instance.StartEvent(Movement.instance.tiledata[1].events[0]);
        NotifyManager.instance.SetPermanentNotify(eventName);
    }
}
