using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class layer : MonoBehaviour
{
    public bool isstaik = false;
    public float offset = 0;
    private int sort = 0;
    private Renderer renderer;
    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }
    private void LateUpdate()
    {
        renderer.sortingOrder = (int)(sort - transform.position.y + offset);
        if (isstaik)
            Destroy(this);
    }
}
