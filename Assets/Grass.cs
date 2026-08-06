using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Plant, IInteractable
{
    public string ActionName => "добыть растения";

    public int Priority => 0;
    protected override void Start()
    {
        transform.position = ChangePosition();
        transform.localScale = ChangeScale();
        if (Random.Range(1, 100) < ChanceToRemove)
        {
            Destroy(gameObject);
        }
        transform.rotation = q;
    }
    new Vector3 ChangePosition()
    {
        float x = transform.position.x + Random.Range(-MaxOffset.x, MaxOffset.x);
        float y = transform.position.y + Random.Range(-MaxOffset.y, MaxOffset.y);
        return new Vector3(x, y, 0);
    }
    Vector3 ChangeScale()
    {
        float x = Random.Range(MinScale.x, MaxScale.x);
        return new Vector3(x, x, 1);
    }

    public IEnumerator ExecuteInteraction(Movement player)
    {
        // Идем к объекту
        yield return player.StartCoroutine(player.MoveToRoutine(transform.position));
        // Физическая остановка на 1.5 секунды перед объектом (добыча)
        yield return new WaitForSeconds(1.5f);
        // Логика выдачи лута

        if (Vector2.Distance(transform.position, player.gameObject.transform.position) < 0.5f && player.currentTask != null)
        {
            SpawnItemInCamp();
            // Inventory.instance.AddItem(item, count);
            Destroy(gameObject);
        }
    }
    public void SpawnItemInCamp()
    {
        for (int i = 0; i < items.Count; i++)
        {
            int counti = Random.Range(0, itemCounts[i] + 1);
            if (counti < 1) continue;
            Inventory.instance.AddItem(items[i], counti, null);
            NotifyManager.instance.SetMinNotify($"получено: {items[i].Name} x{counti}");
        }
    }
}
