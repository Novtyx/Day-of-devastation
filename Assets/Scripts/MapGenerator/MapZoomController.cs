using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapZoomController : MonoBehaviour
{
    public static MapZoomController instance;
    [Header("Настройки камеры")]
    [SerializeField] private Camera cam;
    [Tooltip("Размер камеры, при котором локальная карта меняется на глобальную")]
    [SerializeField] private float transitionThreshold = 25f;
    [Header("Компоненты глобальной карты")]
    [Tooltip("SpriteRenderer с картинкой, которую сгенерировал твой метод GenerateAndSaveMapTexture")]
    [SerializeField] private SpriteRenderer globalMapRenderer;
    [SerializeField] private float fadeSpeed = 5f;
    [Header("Игрок")]
    [Tooltip("Объект с графикой персонажа (аниматор, спрайт)")]
    [SerializeField] private GameObject Player;
    [Tooltip("Иконка-маркер, которая будет видна на большой карте")]
    [SerializeField] private Sprite mapMarker;
    [Tooltip("Иконка-маркер, которая будет видна на локальной карте")]
    [SerializeField] private Sprite localMarker;
    private bool isGlobalMode = false;
    [SerializeField] private GameObject PlayerMarker;
    private void Start()
    {
        instance = this;
        if (cam == null) cam = Camera.main;
        // Прячем глобальную карту и маркер при старте
        SetGlobalMapAlpha(0f);
        if (mapMarker != null) Player.GetComponent<SpriteRenderer>().sprite = localMarker;
        StartCoroutine(Gen());
    }
    private void Update()
    {
        UpdateTransitionLogic();

        if (batches.Count > 0 && treeMesh != null && treeMaterial != null && isGlobalMode)
        {
            foreach (var batch in batches)
            {
                Graphics.DrawMeshInstanced(
                    treeMesh,
                    0,
                    treeMaterial,
                    batch.ToArray(), // Убедитесь, что передаете массив
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

        if (mountainbatches.Count > 0 && mountainMesh != null && mountainMaterial != null && isGlobalMode)
        {
            foreach (var batch in mountainbatches)
            {
                Graphics.DrawMeshInstanced(
                    mountainMesh,
                    0,
                    mountainMaterial,
                    batch.ToArray(), // Убедитесь, что передаете массив
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
    private void UpdateTransitionLogic()
    {
        // Проверяем, пересекла ли камера порог зума
        bool shouldBeGlobal = cam.orthographicSize > transitionThreshold;
        if (shouldBeGlobal != isGlobalMode)
        {
            isGlobalMode = shouldBeGlobal;
            ToggleMode(isGlobalMode);
        }
        // Плавное появление/растворение текстуры глобальной карты
        float targetAlpha = isGlobalMode ? 1f : 0f;
        Color c = globalMapRenderer.color;
        if (Mathf.Abs(c.a - targetAlpha) > 0.01f)
        {
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            globalMapRenderer.color = c;
        }
    }
    private void ToggleMode(bool global)
    {
        // 1. Переключаем визуал игрока
        if (Player != null && !isGlobalMode)
        {
            PlayerMarker.SetActive(false);
            Player.GetComponent<SpriteRenderer>().sprite = localMarker;
        }
        if (Player != null && isGlobalMode)
        { 
            Player.GetComponent<SpriteRenderer>().sprite = mapMarker;
            PlayerMarker.SetActive(true);
        }
        // 2. Ставим генератор чанков на паузу, чтобы не грузить процессор (Jobs) в фоне
        if (TilemapGenerator.instance != null)
        {
            TilemapGenerator.instance.isZoomedOut = global;
            // Скрываем или показываем уже сгенерированные чанки (дети генератора)
            foreach (Transform chunk in TilemapGenerator.instance.transform)
            {
                chunk.gameObject.SetActive(!global);
            }
        }
    }
    private void SetGlobalMapAlpha(float alpha)
    {
        if (globalMapRenderer != null)
        {
            Color c = globalMapRenderer.color;
            c.a = alpha;
            globalMapRenderer.color = c;
        }
    }


    public Mesh treeMesh;
    public Material treeMaterial;

    // Batches of matrices (DrawMeshInstanced can only draw 1023 at a time)
    private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();

    private Bounds renderBounds;
    public int step = 180;

    public Mesh mountainMesh;
    public Material mountainMaterial;

    // Batches of matrices (DrawMeshInstanced can only draw 1023 at a time)
    private List<List<Matrix4x4>> mountainbatches = new List<List<Matrix4x4>>();

    private Bounds mountainrenderBounds;
    public int mountainstep = 180;
    [ContextMenu("Initialize")]
    public void Initialize()
    {
        batches.Clear();
        List<Matrix4x4> allTrees = TilemapGenerator.instance.GenerateGlobalForestMatrices(step);
        allTrees.Sort((a, b) =>
        {
            float yA = a.m13;
            float yB = b.m13;
            return yB.CompareTo(yA);
        });
        Debug.Log(allTrees.Count);

        // Создаем границы, охватывающие всю вашу карту (или очень большие)
        // Например, центр в 0,0,0 и размер 100000x100000
        renderBounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 100000));

        for (int i = 0; i < allTrees.Count; i += 1023)
        {
            int count = Mathf.Min(1023, allTrees.Count - i);
            batches.Add(allTrees.GetRange(i, count));
        }
    }

    [ContextMenu("Initializemountain")]
    public void Initializemountain()
    {
        mountainbatches.Clear();
        List<Matrix4x4> allTrees = TilemapGenerator.instance.GenerateGlobalMountainsMatrices(mountainstep);
        allTrees.Sort((a, b) =>
        {
            float yA = a.m13;
            float yB = b.m13;
            return yB.CompareTo(yA);
        });
        Debug.Log(allTrees.Count);

        // Создаем границы, охватывающие всю вашу карту (или очень большие)
        // Например, центр в 0,0,0 и размер 100000x100000
        mountainrenderBounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 100000));

        for (int i = 0; i < allTrees.Count; i += 1023)
        {
            int count = Mathf.Min(1023, allTrees.Count - i);
            mountainbatches.Add(allTrees.GetRange(i, count));
        }
    }
    public IEnumerator Gen()
    {
        yield return new WaitForSeconds(3f);
        Initialize();
        Initializemountain();
    }
}
