using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceItem", menuName = "Items/Resource")]
public class ResourceItem : Item
{
    public override void Use()
    {
        NotifyManager.instance.SetMinNotify("нельзя использовать напрямую");
    }
    public override void UseByCamp(Camps camp)
    {
        NotifyManager.instance.SetMinNotify("нельзя использовать напрямую");
    }
}
