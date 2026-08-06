using System.Collections.Generic;
using UnityEngine;

public class TreeChunkRenderer : MonoBehaviour
{
    private Mesh treeMesh;
    private Material treeMaterial;

    // Batches of matrices (DrawMeshInstanced can only draw 1023 at a time)
    private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
    private Bounds renderBounds;

    public void Initialize(Mesh mesh, Material mat, List<Matrix4x4> allTrees)
    {
        treeMesh = mesh;
        treeMaterial = mat;
        batches.Clear();
        allTrees.Sort((a, b) =>
        {
            float yA = a.m13;
            float yB = b.m13;
            return yB.CompareTo(yA);
        });

        // —оздаем границы, охватывающие всю вашу карту (или очень большие)
        // Ќапример, центр в 0,0,0 и размер 100000x100000
        renderBounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 100000));

        for (int i = 0; i < allTrees.Count; i += 1023)
        {
            int count = Mathf.Min(1023, allTrees.Count - i);
            batches.Add(allTrees.GetRange(i, count));
        }
    }

    private Mesh mountainMesh;
    private Material mountainMaterial;
    private List<List<Matrix4x4>> mountainbatches = new List<List<Matrix4x4>>();

    private Bounds mountainsrenderBounds;
    public void InitializeMountains(Mesh mesh, Material mat, List<Matrix4x4> allTrees)
    {
        mountainMesh = mesh;
        mountainMaterial = mat;
        mountainbatches.Clear();
        allTrees.Sort((a, b) =>
        {
            float yA = a.m13;
            float yB = b.m13;
            return yB.CompareTo(yA);
        });

        // —оздаем границы, охватывающие всю вашу карту (или очень большие)
        // Ќапример, центр в 0,0,0 и размер 100000x100000
        mountainsrenderBounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 100000));

        for (int i = 0; i < allTrees.Count; i += 1023)
        {
            int count = Mathf.Min(1023, allTrees.Count - i);
            mountainbatches.Add(allTrees.GetRange(i, count));
        }
    }

    private Mesh kamyshMesh;
    private Material kamyshMaterial;
    private List<List<Matrix4x4>> kamyshbatches = new List<List<Matrix4x4>>();

    private Bounds kamyshrenderBounds;
    public void InitializeKamyshs(Mesh mesh, Material mat, List<Matrix4x4> allTrees)
    {
        kamyshMesh = mesh;
        kamyshMaterial = mat;
        kamyshbatches.Clear();
        allTrees.Sort((a, b) =>
        {
            float yA = a.m13;
            float yB = b.m13;
            return yB.CompareTo(yA);
        });

        // —оздаем границы, охватывающие всю вашу карту (или очень большие)
        // Ќапример, центр в 0,0,0 и размер 100000x100000
        kamyshrenderBounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 100000));

        for (int i = 0; i < allTrees.Count; i += 1023)
        {
            int count = Mathf.Min(1023, allTrees.Count - i);
            kamyshbatches.Add(allTrees.GetRange(i, count));
        }
    }

    private void Update()
    {
        if (batches.Count > 0 && treeMesh != null && treeMaterial != null)
        {
            foreach (var batch in batches)
            {
                Graphics.DrawMeshInstanced(
                    treeMesh,
                    0,
                    treeMaterial,
                    batch.ToArray(), // ”бедитесь, что передаете массив
                    batch.Count,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false,
                    2,      // Layer
                    null,   // Camera (null = все камеры)
                    UnityEngine.Rendering.LightProbeUsage.Off,
                    null    // LightProbeProxyVolume
                );
            }
        }
        if (mountainbatches.Count > 0 && mountainMesh != null && mountainMaterial != null)
        {
            foreach (var batch in mountainbatches)
            {
                Graphics.DrawMeshInstanced(
                    mountainMesh,
                    0,
                    mountainMaterial,
                    batch.ToArray(), // ”бедитесь, что передаете массив
                    batch.Count,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false,
                    2,      // Layer
                    null,   // Camera (null = все камеры)
                    UnityEngine.Rendering.LightProbeUsage.Off,
                    null    // LightProbeProxyVolume
                );
            }
        }
        if (kamyshbatches.Count > 0 && kamyshMesh != null && kamyshMaterial != null)
        {
            foreach (var batch in kamyshbatches)
            {
                Graphics.DrawMeshInstanced(
                    kamyshMesh,
                    0,
                    kamyshMaterial,
                    batch.ToArray(), // ”бедитесь, что передаете массив
                    batch.Count,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false,
                    2,      // Layer
                    null,   // Camera (null = все камеры)
                    UnityEngine.Rendering.LightProbeUsage.Off,
                    null    // LightProbeProxyVolume
                );
            }
        }
    }


}

