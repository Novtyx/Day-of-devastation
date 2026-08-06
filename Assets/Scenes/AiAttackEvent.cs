using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackEvent", menuName = "AiActions/AttackEvent")]
public class AiAttackEvent : AiEvent
{
    public Enemy enemy;
    public override void Activate()
    {
        base.Activate();
        AttackManager.instance.StartAttack(enemy);
        NotifyManager.instance.SetPermanentNotify(eventName);
    }
}
