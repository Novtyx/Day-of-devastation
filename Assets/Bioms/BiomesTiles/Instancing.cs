using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Instancing : MonoBehaviour
{
    private void Awake()
    {
        MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(materialProperty);
    }
}
