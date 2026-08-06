using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Tree : Plant, IInteractable
{

    public string ActionName => "заготовить дрова";

    public int Priority => 1;
    public BoxCollider2D collider2D;

    protected override void Start()
    {
        base.Start();
    }
    private void OnBecameVisible()
    {
        collider2D.enabled = true;
    }
    private void OnBecameInvisible()
    {
        collider2D.enabled = false;
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
