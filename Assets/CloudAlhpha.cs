using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudAlhpha : MonoBehaviour
{
    public float MinAlpha = 4f;
    public float MaxAlpha = 60f;
    public Camera Camera;
    public Material material;
    float lastSize;
    void Update()
    {
        if (Camera == null || material == null) return;
        if (Mathf.Abs(Camera.orthographicSize - lastSize) > 0.01f)
        {
            float normalized = Mathf.InverseLerp(MinAlpha, MaxAlpha, Camera.orthographicSize);
            material.SetFloat("_alpha", normalized);
        }
    }
}
