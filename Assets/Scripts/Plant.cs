using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Plant : MonoBehaviour
{
    public Vector2 MaxOffset = new Vector2(1f, 1f);
    public Vector2 MinScale = new Vector2(1f, 2f);
    public Vector2 MaxScale = new Vector2(1f, 2.3f);
    public int ChanceToRemove = 100;
    public Quaternion q;
    public List<Item> items = new List<Item>();
    public List<int> itemCounts = new List<int>();

    protected virtual void Start()
    {
        transform.position = ChangePosition();
        transform.localScale = ChangeScale();
        if (Random.Range(1, 100) < ChanceToRemove)
        {
            Destroy(gameObject);
        }
        transform.rotation = q;
    }
    public Vector3 ChangePosition()
    {
        float x = transform.position.x + Random.Range(-MaxOffset.x, MaxOffset.x);
        float y = transform.position.y + Random.Range(-MaxOffset.y, MaxOffset.y);
        return new Vector3(x, y, 0);
    }
    public Vector3 ChangeScale()
    {
        float x = Random.Range(MinScale.x, MaxScale.x);
        float y = Random.Range(MinScale.y, MaxScale.y);
        return new Vector3(x, y, 1);
    }
}
