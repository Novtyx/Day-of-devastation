using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class visiblechild : MonoBehaviour
{
    public GameObject child;
    private void OnBecameVisible()
    {
        if (!child.GetComponent<TopDownGenerator>().isActive) child.SetActive(true);
    }
    private void OnBecameInvisible()
    {
        if (!child.GetComponent<TopDownGenerator>().isActive) child.SetActive(false);
    }
}
