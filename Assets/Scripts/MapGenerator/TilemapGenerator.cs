using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;

public struct TreeData
{
    public float2 scale;
    public float2 offset;
    public int MaterialType;
}

public struct TileGenerationJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<byte> mapData;
    [ReadOnly] public int startX;
    [ReadOnly] public int startY;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public int mapWidth;

    [ReadOnly] public float noiseScale;
    [ReadOnly] public float radiationScale;
    [ReadOnly] public float4 offsets; // heightX, heightY, tempX, tempY
    [ReadOnly] public float4 offsets2; // moistureX, moistureY, radiationX, radiationY

    [ReadOnly] public float waterThreshold;
    [ReadOnly] public float mountainThreshold;
    [ReadOnly] public int generateColors;

    // Biome colors
    [ReadOnly] public BiomeSettings settings;
    [ReadOnly] public float4 riversColor;
    [ReadOnly] public float4 seaColor;

    [ReadOnly] public float4 PystoshTileColor;
    [ReadOnly] public float4 SnowPystoshTileColor;
    [ReadOnly] public float4 RiverTileColor;

    // Output arrays
    public NativeArray<int> tileTypes;
    public NativeArray<float4> tileColors;

    public NativeArray<bool> spawnTree;
    public NativeArray<TreeData> treeData;

    public void Execute(int index)
    {
        int x = index % width;
        int y = index / width;

        int worldX = startX + x;
        int worldY = startY + y;

        // Проверяем границы карты
        if (worldX >= mapWidth || worldY * mapWidth + worldX >= mapData.Length)
        {
            tileTypes[index] = 10; // sea
            tileColors[index] = seaColor * RiverTileColor;
            spawnTree[index] = false;
            return;
        }

        int mapIndex = worldY * mapWidth + worldX;
        byte terrainType = mapData[mapIndex];

        switch (terrainType)
        {
            case 0: // Вода
                tileTypes[index] = 10; // sea
                tileColors[index] = seaColor;
                spawnTree[index] = false;
                treeData[index] = new TreeData { scale = 0, offset = 0, MaterialType = 0 };
                break;

            case 2: // Река
                tileTypes[index] = 9; // rivers
                tileColors[index] = riversColor;
                spawnTree[index] = false;
                treeData[index] = new TreeData { scale = 0, offset = 0, MaterialType = 0 };
                break;

            case 1: // Земля - генерируем биом
                GenerateBiomeForTile(index, worldX, worldY);
                break;

            default: // Запасной вариант
                tileTypes[index] = 10; // sea
                tileColors[index] = seaColor;
                spawnTree[index] = false;
                treeData[index] = new TreeData { scale = 0, offset = 0, MaterialType = 0 };
                break;
        }
    }
    private void GenerateBiomeForTile(int index, int worldX, int worldY)
    {
        // Generate noise values
        float heightValue = GetNoise(worldX, worldY, offsets.xy, noiseScale);
        float temperatureValue = GetNoise(worldX, worldY, offsets.zw, noiseScale);
        float moistureValue = GetNoise(worldX, worldY, offsets2.xy, noiseScale);
        float radiationValue = GetNoise(worldX, worldY, offsets2.zw, radiationScale);

        BiomeCalculator.CalculateBiome(heightValue, temperatureValue, moistureValue, radiationValue, worldX, worldY, settings, 
            out int type, out float4 color, out bool tree, out int MaterialType, out float dyy, out float tresh);
        tileTypes[index] = type;
        tileColors[index] = color;
        spawnTree[index] = tree;
        if (tree)
        {
            uint seed = (uint)(worldX * 1000 * worldY) + 1;
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);
            treeData[index] = new TreeData
            {
                scale = new float2(1f, random.NextFloat(1.4f, 2.2f)),
                offset = new float2(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f)),
                MaterialType = MaterialType
            };
        }
        else
        {
            treeData[index] = new TreeData { scale = 0, offset = 0 };
        }
    }

    private float GetNoise(int x, int y, float2 offset, float scale)
    {
        float val = noise.cnoise(new float2((x + offset.x) / scale, (y + offset.y) / scale));
        return (val + 1f) * 0.5f;
    }
}

public class TilemapGenerator : MonoBehaviour
{
    public static TilemapGenerator instance;

    public Material TreeMaterial, MountainMaterial, KamyshMaterial;
    [Header("Map Data")]
    [SerializeField] private string mapDataFileName = "mapData";
    [Header("Objects")]
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform tree;
    [SerializeField] private Transform structure;

    [Header("Tiles")]
    [SerializeField]
    private RuleTile forest, radforest, pystosh, radpystosh, boloto,
        radboloto, mountains, snow_mountains, grass_mountains, rivers, sea, pole, mushrooms, ivaTree,
        snowyforest, snowyrad_forest, snowypystosh, snowyrad_pystosh, snowyboloto, snowyrad_boloto, snowyrivers, snowypole, pystosh_tree, radpystosh_tree, pystosh_car;

    [Header("Chunk Settings")]
    [SerializeField] private GameObject tilemapChunkPrefab;
    [SerializeField] private int chunkSize = 256;
    [SerializeField] private int viewDistanceInChunks = 2;
    [SerializeField] private Transform targetToTrack;

    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Queue<GameObject> chunkPool = new();

    private int mapWidth, mapHeight;
    private MapData currentMapData;
    private NativeArray<byte> nativeMapData;

    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 20f, radiationScale = 40f;
    [SerializeField]
    private Vector2 heightOffset,
        temperatureOffset, moistureOffset, radiationOffset;
    [Range(0, 1)] public float waterThreshold = 0.4f;
    [Range(0, 1)]
    [SerializeField] private float mountainThreshold = 0.8f;

    [Header("Biome Colors")]
    [SerializeField] private Color forestColor = new Color(0.1f, 0.5f, 0.1f);
    [SerializeField] private Color forestColorSecond = new Color(0.1f, 0.5f, 0.1f);
    [SerializeField] private Color radForestColor = new Color(0.3f, 0.6f, 0.2f);
    [SerializeField] private Color pystoshColor = new Color(0.7f, 0.6f, 0.4f);
    [SerializeField] private Color radPystoshColor = new Color(0.6f, 0.5f, 0.3f);
    [SerializeField] private Color bolotoColor = new Color(0.3f, 0.4f, 0.2f);
    [SerializeField] private Color radBolotoColor = new Color(0.4f, 0.5f, 0.3f);
    [SerializeField] private Color mountainsColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color snowMountainsColor = Color.white;
    [SerializeField] private Color riversColor = new Color(0.2f, 0.4f, 0.8f);
    [SerializeField] private Color seaColor = new Color(0.1f, 0.3f, 0.7f);
    [SerializeField] private Color beachColor = new Color(0.1f, 0.3f, 0.7f);
    [SerializeField] private Color snowyColor = new Color(0.1f, 0.3f, 0.7f);

    [SerializeField] private Color PystoshTileColor = new Color(0.1f, 0.3f, 0.7f);
    [SerializeField] private Color SnowPystoshTileColor = new Color(0.1f, 0.3f, 0.7f);
    [SerializeField] private Color RiverTileColor = new Color(0.1f, 0.3f, 0.7f);

    private int generatecolors = 1;
    public RuleTile[] tileLookup;
    public Biome[] biomes;
    public bool isZoomedOut = false;

    private void Start()
    {
        instance = this;
        viewDistanceInChunks = PlayerPrefs.GetInt("chunksize", 2);
        generatecolors = PlayerPrefs.GetInt("generatecolors", 1);

        // Загружаем данные карты
        LoadMapData();

        // Инициализируем lookup таблицу тайлов
        tileLookup = new RuleTile[]
        {
            forest,        // 0
            radforest,     // 1
            pystosh,       // 2
            radpystosh,    // 3
            boloto,        // 4
            radboloto,     // 5
            mountains,     // 6
            snow_mountains,// 7
            grass_mountains,//8
            rivers,        // 9
            sea,           // 10
            pole,          // 11
            mushrooms,     // 12
            ivaTree,       // 13
            snowyforest,   // 14
            snowyrad_forest,// 15
            snowypystosh,   // 16
            snowyrad_pystosh,   // 17
            snowyboloto,   // 18
            snowyrad_boloto,   // 19
            snowyrivers,   // 20
            snowypole,     // 21
            pystosh_tree,  // 22
            radpystosh_tree, // 23
            pystosh_car, // 24
        };
        StartCoroutine(ChunkUpdateLoop());
    }

    private void LoadMapData()
    {
        currentMapData = MapDataConverter.LoadMapDataFromResources(mapDataFileName);
        if (currentMapData == null)
        {
            Debug.LogError("Failed to load map data!");
            return;
        }

        mapWidth = currentMapData.width;
        mapHeight = currentMapData.height;

        // Создаем NativeArray из обычного массива
        nativeMapData = new NativeArray<byte>(currentMapData.terrainData, Allocator.Persistent);

        Debug.Log($"Map data loaded: {mapWidth}x{mapHeight}, data size: {currentMapData.terrainData.Length} bytes");
    }

    private void OnDestroy()
    {
        if (nativeMapData.IsCreated)
            nativeMapData.Dispose();
    }

    IEnumerator ChunkUpdateLoop()
    {
        while (true)
        {
            UpdateChunks();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public Tilemap GetTilemapPosition(Vector3 worldpos)
    {
        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt(worldpos.x / chunkSize),
            Mathf.FloorToInt(worldpos.y / chunkSize)
        );
        if (activeChunks.TryGetValue(chunkCoord, out GameObject chunk))
        {
            return chunk.GetComponentInChildren<Tilemap>();
        }
        return null;
    }

    void UpdateChunks()
    {
        if (isZoomedOut) return;
        if (currentMapData == null) return;

        Vector2 position = targetToTrack.position;
        Vector2Int currentChunk = new(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.y / chunkSize)
        );

        HashSet<Vector2Int> requiredChunks = new();

        for (int dx = -viewDistanceInChunks; dx <= viewDistanceInChunks; dx++)
        {
            for (int dy = -viewDistanceInChunks; dy <= viewDistanceInChunks; dy++)
            {
                Vector2Int coord = new(currentChunk.x + dx, currentChunk.y + dy);
                requiredChunks.Add(coord);

                if (!activeChunks.ContainsKey(coord))
                {
                    if (IsChunkWithinMap(coord))
                        StartCoroutine(LoadChunk(coord));
                }
            }
        }

        List<Vector2Int> toRemove = new();
        foreach (var chunk in activeChunks)
        {
            if (!requiredChunks.Contains(chunk.Key))
            {
                chunk.Value.SetActive(false);
                chunkPool.Enqueue(chunk.Value);
                toRemove.Add(chunk.Key);
            }
        }

        foreach (var coord in toRemove)
        {
            activeChunks.Remove(coord);
        }
    }

    bool IsChunkWithinMap(Vector2Int coord)
    {
        int x = coord.x * chunkSize;
        int y = coord.y * chunkSize;
        return x >= 0 && y >= 0 && x < mapWidth && y < mapHeight;
    }

    IEnumerator LoadChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObj = chunkPool.Count > 0
            ? chunkPool.Dequeue()
            : Instantiate(tilemapChunkPrefab, transform);

        chunkObj.name = $"Chunk_{chunkCoord.x}_{chunkCoord.y}";
        chunkObj.SetActive(true);

        Tilemap tilemap = chunkObj.GetComponentInChildren<Tilemap>();
        tilemap.ClearAllTiles();

        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;

        int width = Mathf.Min(chunkSize, mapWidth - startX);
        int height = Mathf.Min(chunkSize, mapHeight - startY);

        var settings = GetBiomeSettings();

        // Создаем массивы для результатов
        var tileTypes = new NativeArray<int>(width * height, Allocator.TempJob);
        var tileColors = new NativeArray<float4>(width * height, Allocator.TempJob);
        var spawnTree = new NativeArray<bool>(width * height, Allocator.TempJob);
        var treeData = new NativeArray<TreeData>(width * height, Allocator.TempJob);

        // Настраиваем job
        var job = new TileGenerationJob
        {
            mapData = nativeMapData,
            startX = startX,
            startY = startY,
            width = width,
            height = height,
            mapWidth = mapWidth,
            noiseScale = noiseScale,
            radiationScale = radiationScale,
            offsets = new float4(heightOffset.x, heightOffset.y, temperatureOffset.x, temperatureOffset.y),
            offsets2 = new float4(moistureOffset.x, moistureOffset.y, radiationOffset.x, radiationOffset.y),
            waterThreshold = waterThreshold,
            mountainThreshold = mountainThreshold,
            generateColors = generatecolors,

            // Colors as float4
            settings = settings,
            riversColor = new float4(riversColor.r, riversColor.g, riversColor.b, riversColor.a),
            seaColor = new float4(seaColor.r, seaColor.g, seaColor.b, seaColor.a),
            PystoshTileColor = new float4(PystoshTileColor.r, PystoshTileColor.g, PystoshTileColor.b, PystoshTileColor.a),
            RiverTileColor = new float4(RiverTileColor.r, RiverTileColor.g, RiverTileColor.b, RiverTileColor.a),
            SnowPystoshTileColor = new float4(SnowPystoshTileColor.r, SnowPystoshTileColor.g, SnowPystoshTileColor.b, SnowPystoshTileColor.a),

            tileTypes = tileTypes,
            tileColors = tileColors,
            spawnTree = spawnTree,
            treeData = treeData
        };

        // Запускаем job
        JobHandle jobHandle = job.Schedule(width * height, 64);
        yield return new WaitUntil(() => jobHandle.IsCompleted);
        jobHandle.Complete();
        for (int z = 0; z < tileTypes.Length; z++)
        {
            if (tileTypes[z] == 0 && UnityEngine.Random.Range(0, 100) < 2)
            {
                tileTypes[z] = 12;
            }
        }
        for (int z = 0; z < tileTypes.Length; z++)
        {
            if (tileTypes[z] == 4 && UnityEngine.Random.Range(0, 100) < 2)
            {
                tileTypes[z] = 13;
            }
        }
        for (int z = 0; z < tileTypes.Length; z++)
        {
            if (tileTypes[z] == 2 && UnityEngine.Random.Range(0, 100) < 2)
            {
                tileTypes[z] = 22;
            }
        }
        for (int z = 0; z < tileTypes.Length; z++)
        {
            if (tileTypes[z] == 3 && UnityEngine.Random.Range(0, 100) < 2)
            {
                tileTypes[z] = 23;
            }
        }
        for (int z = 0; z < tileTypes.Length; z++)
        {
            if (tileTypes[z] == 2 && UnityEngine.Random.Range(0, 10000) < 2)
            {
                tileTypes[z] = 24;
            }
        }
        // Применяем результаты к тайлмапу
        ApplyTilesToTilemap(tilemap, startX, startY, width, height, tileTypes, tileColors);
        //  CreateEnemyObjects(tilemap, startX, startY, width, height, tileTypes);
        CreateStructureObjects(tilemap, startX, startY, width, height, tileTypes);
        CreateTreeObjects1(chunkObj, tilemap, startX, startY, width, height, spawnTree, treeData);
        // Очищаем временные массивы
        tileTypes.Dispose();
        tileColors.Dispose();
        spawnTree.Dispose();
        treeData.Dispose();

        tilemap.CompressBounds();

        activeChunks[chunkCoord] = chunkObj;

        if (Movement.instance != null)
            Movement.instance.tilemap = GetTilemapPosition(targetToTrack.transform.position);
    }
    // Внутри TilemapGenerator добавьте поле:
    [SerializeField] private Mesh quadMesh; // НАЗНАЧИТЬ В ИНСПЕКТОРЕ (можно создать дефолтный Quad)
    // Полностью обновленный метод CreateTreeObjects
    private void CreateTreeObjects1(GameObject chunkObj, Tilemap tilemap, int startX, int startY, int width, int height, NativeArray<bool> spawnTrees, NativeArray<TreeData> treeData)
    {
        List<Matrix4x4> treeMatrices = new List<Matrix4x4>();
        List<Matrix4x4> mountainMatrices = new List<Matrix4x4>();
        List<Matrix4x4> kamyshMatrices = new List<Matrix4x4>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (spawnTrees[index])
                {
                    Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(startX + x, startY + y, 0));

                    // Расчет позиции
                    Vector3 position = worldPos + new Vector3(0.5f, 0.5f, 0) +
                                       new Vector3(treeData[index].offset.x, treeData[index].offset.y, -1f);

                    // Для сортировки спрайтов по Z (если игра 2D Top Down) часто используют Y
                    position.z = position.y * -0.001f; // Раскомментируйте если деревья перекрывают друг друга неправильно

                    Vector3 scale = new Vector3(treeData[index].scale.x, treeData[index].scale.y, 1f);
                    if (treeData[index].MaterialType == 0) treeMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, scale));
                    else if (treeData[index].MaterialType == 2)
                    {
                        if (UnityEngine.Random.Range(0, 20) == 0) kamyshMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, scale));
                    }
                    else
                    {
                        if (UnityEngine.Random.Range(0, 2) == 0) mountainMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, scale));
                    }
                }
            }
        }

        // Добавляем или получаем компонент рендеринга на чанке
        TreeChunkRenderer renderer = chunkObj.GetComponent<TreeChunkRenderer>();
        if (renderer == null) renderer = chunkObj.AddComponent<TreeChunkRenderer>();


        // Передаем данные. TreeMaterial берем из полей TilemapGenerator
        renderer.Initialize(quadMesh, TreeMaterial, treeMatrices);
        renderer.InitializeMountains(quadMesh, MountainMaterial, mountainMatrices);
        renderer.InitializeKamyshs(quadMesh, KamyshMaterial, kamyshMatrices);
    }


    private void CreateEnemyObjects(Tilemap tilemap, int startX, int startY, int width, int height, NativeArray<int> tileTypes)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int tileType = tileTypes[index];

                if (UnityEngine.Random.Range(0, 1000) < 1)
                {
                    Biome biome = biomes[tileType];
                    if (biome.enemys.Count < 1) return;

                    Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(startX + x, startY + y, 0));

                    // Создаем отдельный GameObject для дерева
                    GameObject tree = Instantiate(enemy.gameObject, worldPos + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);

                    tree.transform.SetParent(tilemap.transform);
                    tree.GetComponent<EnemyEntity>().enemy = biome.enemys[UnityEngine.Random.Range(0, biome.enemys.Count)];
                    tree.GetComponent<SpriteRenderer>().sprite = tree.GetComponent<EnemyEntity>().enemy.sprite;
                    if (UnityEngine.Random.Range(0, 10) < 5)
                    {
                        tree.GetComponent<EnemyEntity>().isDynamic = true;
                        tree.GetComponent<EnemyEntity>().lastPosition = worldPos + new Vector3(0.5f, 0.5f, 0);
                    }
                }
            }
        }
    }

    private void CreateStructureObjects(Tilemap tilemap, int startX, int startY, int width, int height, NativeArray<int> tileTypes)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int tileType = tileTypes[index];

                if (UnityEngine.Random.Range(0, 1000) < 1)
                {
                    Biome biome = biomes[tileType];
                    if (biome.structures.Count < 1) return;

                    Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(startX + x, startY + y, 0));

                    // Создаем отдельный GameObject для дерева
                    GameObject tree = Instantiate(structure.gameObject, worldPos + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);

                    tree.transform.SetParent(tilemap.transform);
                    tree.GetComponent<DynamicEvent>().Event = biome.structures[UnityEngine.Random.Range(0, biome.structures.Count)];
                    tree.GetComponent<SpriteRenderer>().sprite = tree.GetComponent<DynamicEvent>().Event.sprite;
                }
            }
        }
    }
    private void ApplyTilesToTilemap(Tilemap tilemap, int startX, int startY, int width, int height,
                                   NativeArray<int> tileTypes, NativeArray<float4> tileColors)
    {
        // Создаем массив тайлов для установки
        TileBase[] tiles = new TileBase[width * height];
        Color[] colors = new Color[width * height];

        // Заполняем массивы
        for (int i = 0; i < width * height; i++)
        {
            int tileType = tileTypes[i];
            if (tileType >= 0 && tileType < tileLookup.Length)
            {
                tiles[i] = tileLookup[tileType];
            }
            else
            {
                tiles[i] = sea; // fallback
            }

            float4 color4 = tileColors[i];
            colors[i] = new Color(color4.x, color4.y, color4.z, color4.w);
        }

        // Устанавливаем тайлы
        tilemap.SetTilesBlock(new BoundsInt(startX, startY, 0, width, height, 1), tiles);

        // Устанавливаем цвета если нужно
        if (generatecolors == 1)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (isNeighbour(tilemap, new Vector3Int(startX + x, startY + y, 0))) tilemap.SetColor(new Vector3Int(startX + x, startY + y, 0), beachColor);
                    else 
                    {
                        int index = y * width + x;
                        tilemap.SetColor(new Vector3Int(startX + x, startY + y, 0), colors[index]);
                    }
                }
            }
        }
        tilemap.gameObject.isStatic = true;
        Tree[] trees = tilemap.GetComponentsInChildren<Tree>();
        for (int i = 0; i < trees.Length; i++)
        {
            trees[i].gameObject.isStatic = true;
        }
        StaticBatchingUtility.Combine(tilemap.gameObject);
    }
    // ==========================================
    // НОВАЯ ФУНКЦИЯ ГЕНЕРАЦИИ КАРТЫ (ДОБАВЛЕНА)
    // ==========================================

    bool isNeighbour(Tilemap floor, Vector3Int pos)
    {
        if ((floor.GetTile(new Vector3Int(pos.x + 1, pos.y)) == rivers ||
    floor.GetTile(new Vector3Int(pos.x - 1, pos.y)) == rivers ||
    floor.GetTile(new Vector3Int(pos.x, pos.y + 1)) == rivers ||
    floor.GetTile(new Vector3Int(pos.x, pos.y - 1)) == rivers) && floor.GetTile(new Vector3Int(pos.x, pos.y)) != rivers) return true;
        return false;
    }
    public TileBase GetTileAtPosition(Vector3 worldPosition)
    {
        int tileType = GetTileTypeIDAtPosition(worldPosition);
        if (biomes != null && tileType >= 0 && tileType < biomes.Length)
            return tileLookup[tileType];
        return null;
    }
    public Biome GetBiomeAtPosition(Vector3 worldPosition)
    {
        int tileType = GetTileTypeIDAtPosition(worldPosition);
        if (biomes != null && tileType >= 0 && tileType < biomes.Length)
            return biomes[tileType];
        return null;
    }
    private int GetTileTypeIDAtPosition(Vector3 worldPosition)
    {
        // Переводим мировые координаты в координаты сетки
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        // Проверяем границы карты
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return 10; // sea
        if (!nativeMapData.IsCreated)
            return 10; // sea
                       // Получаем базовый тип поверхности из NativeArray
        int mapIndex = y * mapWidth + x;
        byte terrainType = nativeMapData[mapIndex];
        if (terrainType == 0) return 10; // sea
        if (terrainType == 2) return 9; // rivers
                                        // Если это земля, высчитываем шум (точно так же, как в джобе)
        float heightValue = GetNoiseValue(x, y, heightOffset, noiseScale);
        float temperatureValue = GetNoiseValue(x, y, temperatureOffset, noiseScale);
        float moistureValue = GetNoiseValue(x, y, moistureOffset, noiseScale);
        float radiationValue = GetNoiseValue(x, y, radiationOffset, radiationScale);
        // Упаковываем настройки цветов и порогов
        BiomeSettings settings = GetBiomeSettings();
        // Вызываем нашу общую статическую функцию
        BiomeCalculator.CalculateBiome(
        heightValue, temperatureValue, moistureValue, radiationValue,
        x, y, settings,
        out int finalTileType, out float4 _, out bool _, out int _, out float dyy, out float tresh
        );
        return finalTileType;
    }
    private float GetNoiseValue(int x, int y, Vector2 offset, float scale)
    {
        float val = noise.cnoise(new float2((x + offset.x) / scale, (y + offset.y) / scale));
        return (val + 1f) * 0.5f;
    }
    // Вспомогательная функция для упаковки цветов в структуру (вызывается и здесь, и перед запуском Job)
    public BiomeSettings GetBiomeSettings()
    {
        return new BiomeSettings
        {
            mountainThreshold = this.mountainThreshold,
            forestColor = new float4(forestColor.r, forestColor.g, forestColor.b, forestColor.a),
            forestColorSecond = new float4(forestColorSecond.r, forestColorSecond.g, forestColorSecond.b, forestColorSecond.a),
            radForestColor = new float4(radForestColor.r, radForestColor.g, radForestColor.b, radForestColor.a),
            pystoshColor = new float4(pystoshColor.r, pystoshColor.g, pystoshColor.b, pystoshColor.a),
            radPystoshColor = new float4(radPystoshColor.r, radPystoshColor.g, radPystoshColor.b, radPystoshColor.a),
            bolotoColor = new float4(bolotoColor.r, bolotoColor.g, bolotoColor.b, bolotoColor.a),
            radBolotoColor = new float4(radBolotoColor.r, radBolotoColor.g, radBolotoColor.b, radBolotoColor.a),
            mountainsColor = new float4(mountainsColor.r, mountainsColor.g, mountainsColor.b, mountainsColor.a),
            snowMountainsColor = new float4(snowMountainsColor.r, snowMountainsColor.g, snowMountainsColor.b, snowMountainsColor.a),
            snowyColor = new float4(snowyColor.r, snowyColor.g, snowyColor.b, snowyColor.a)
        };
    }

    public List<Matrix4x4> GenerateGlobalForestMatrices(int step)
    {
        List<Matrix4x4> matrices = new List<Matrix4x4>();
        for (int y = 0; y < currentMapData.height; y += step)
        {
            for (int x = 0; x < currentMapData.width; x += step)
            {
                // 1. Повторяем логику расчетов из твоего Job
                float height = GetNoiseValue(x, y, heightOffset, noiseScale);
                float temperature = GetNoiseValue(x, y, temperatureOffset, noiseScale);
                float moisture = GetNoiseValue(x, y, moistureOffset, noiseScale);
                float radiation = GetNoiseValue(x, y, radiationOffset, radiationScale);
                // 2. Условие биома (в точности как в твоем Job для ForestColor)
                // Здесь я привел пример, подставь свои точные условия из Job
                bool isWater = currentMapData.terrainData[y * currentMapData.width + x] == 0 || currentMapData.terrainData[y * currentMapData.width + x] == 2;
                bool isMountain = height > mountainThreshold;
                bool isForest = (!isWater && !isMountain) && ((moisture > 0.6f && moisture < 0.8f) || moisture > 0.6f && moisture < 0.8f && radiation > 0.7f);
                if (isForest)
                {
                    // Добавляем случайный сдвиг внутри шага, чтобы убрать эффект сетки
                    float randomX = UnityEngine.Random.Range(-step * 0.4f, step * 0.4f);
                    float randomY = UnityEngine.Random.Range(-step * 0.4f, step * 0.4f);
                    Vector3 worldPos = new Vector3(x, y , 0);
                    // Создаем матрицу с увеличенным масштабом для глобальной карты
                    // Можно также добавить рандомный поворот по Z
                    float randomRotation = UnityEngine.Random.Range(0f, 360f);
                    Matrix4x4 matrix = Matrix4x4.TRS(
                    worldPos,
                    Quaternion.Euler(0, 0, 0),
                    new Vector3(1.4f, 2.2f, 0) * 7f // Размер "иконки" леса
                    );
                    matrices.Add(matrix);
                    if (matrices.Count >= 1023) break;
                }
            }
            if (matrices.Count >= 1023) break;
        }
        return matrices;
    }

    public List<Matrix4x4> GenerateGlobalMountainsMatrices(int step)
    {
        List<Matrix4x4> matrices = new List<Matrix4x4>();
        for (int y = 0; y < currentMapData.height; y += step)
        {
            for (int x = 0; x < currentMapData.width; x += step)
            {
                // 1. Повторяем логику расчетов из твоего Job
                float height = GetNoiseValue(x, y, heightOffset, noiseScale);
                float temperature = GetNoiseValue(x, y, temperatureOffset, noiseScale);
                float moisture = GetNoiseValue(x, y, moistureOffset, noiseScale);
                float radiation = GetNoiseValue(x, y, radiationOffset, radiationScale);
                // 2. Условие биома (в точности как в твоем Job для ForestColor)
                // Здесь я привел пример, подставь свои точные условия из Job
                bool isWater = currentMapData.terrainData[y * currentMapData.width + x] == 0 || currentMapData.terrainData[y * currentMapData.width + x] == 2;
                bool isForest = (!isWater) && (height > 0.7f);
                if (isForest)
                {
                    // Добавляем случайный сдвиг внутри шага, чтобы убрать эффект сетки
                    float randomX = UnityEngine.Random.Range(-step * 0.4f, step * 0.4f);
                    float randomY = UnityEngine.Random.Range(-step * 0.4f, step * 0.4f);
                    Vector3 worldPos = new Vector3(x, y, 0);
                    // Создаем матрицу с увеличенным масштабом для глобальной карты
                    // Можно также добавить рандомный поворот по Z
                    float randomRotation = UnityEngine.Random.Range(0f, 360f);
                    Matrix4x4 matrix = Matrix4x4.TRS(
                    worldPos,
                    Quaternion.Euler(0, 0, 0),
                    new Vector3(1.4f, 2.2f, 0) * 15f // Размер "иконки" леса
                    );
                    matrices.Add(matrix);
                    if (matrices.Count >= 1023) break;
                }
            }
            if (matrices.Count >= 1023) break;
        }
        return matrices;
    }
#if UNITY_EDITOR
    [ContextMenu("Generate Map Image")]
    public void GenerateAndSaveMapTexture()
    {
        // 1. Загружаем данные карты заново, чтобы быть уверенным, что массив существует
        currentMapData = MapDataConverter.LoadMapDataFromResources(mapDataFileName);
        if (currentMapData == null)
        {
            Debug.LogError("Не удалось загрузить данные карты!");
            return;
        }

        int w = currentMapData.width;
        int h = currentMapData.height;
        int totalPixels = w * h;

        // Создаем временный NativeArray для Job
        NativeArray<byte> tempNativeMapData = new NativeArray<byte>(currentMapData.terrainData, Allocator.TempJob);

        var tileTypes = new NativeArray<int>(totalPixels, Allocator.TempJob);
        var tileColors = new NativeArray<float4>(totalPixels, Allocator.TempJob);
        var spawnTree = new NativeArray<bool>(totalPixels, Allocator.TempJob);
        var treeData = new NativeArray<TreeData>(totalPixels, Allocator.TempJob);

        var job = new TileGenerationJob
        {
            mapData = tempNativeMapData, // Используем свежесозданный массив
            startX = 0,
            startY = 0,
            width = w,
            height = h,
            mapWidth = w,
            noiseScale = noiseScale,
            radiationScale = radiationScale,
            offsets = new float4(heightOffset.x, heightOffset.y, temperatureOffset.x, temperatureOffset.y),
            offsets2 = new float4(moistureOffset.x, moistureOffset.y, radiationOffset.x, radiationOffset.y),
            waterThreshold = waterThreshold,
            mountainThreshold = mountainThreshold,
            generateColors = 1,

            riversColor = new float4(riversColor.r, riversColor.g, riversColor.b, riversColor.a),
            seaColor = new float4(seaColor.r, seaColor.g, seaColor.b, seaColor.a),
            PystoshTileColor = new float4(PystoshTileColor.r, PystoshTileColor.g, PystoshTileColor.b, PystoshTileColor.a),
            RiverTileColor = new float4(RiverTileColor.r, RiverTileColor.g, RiverTileColor.b, RiverTileColor.a),
            SnowPystoshTileColor = new float4(SnowPystoshTileColor.r, SnowPystoshTileColor.g, SnowPystoshTileColor.b, SnowPystoshTileColor.a),

            tileTypes = tileTypes,
            tileColors = tileColors,
            spawnTree = spawnTree,
            treeData = treeData
        };

        JobHandle handle = job.Schedule(totalPixels, 64);
        handle.Complete();

        Texture2D mapTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color32[] colors = new Color32[totalPixels];

        for (int i = 0; i < totalPixels; i++)
        {
            float4 c = tileColors[i];
            // Убеждаемся, что значения зажаты между 0 и 1
            colors[i] = new Color(Mathf.Clamp01(c.x), Mathf.Clamp01(c.y), Mathf.Clamp01(c.z), 1f);
        }

        mapTexture.SetPixels32(colors);
        mapTexture.Apply();

        byte[] bytes = mapTexture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, "GeneratedMap.png");
        File.WriteAllBytes(path, bytes);

        // Очистка
        tempNativeMapData.Dispose();
        tileTypes.Dispose();
        tileColors.Dispose();
        spawnTree.Dispose();
        treeData.Dispose();

        AssetDatabase.Refresh();
        Debug.Log($"Карта успешно сохранена в: {path}");
    }
#endif
#if UNITY_EDITOR
    [ContextMenu("Generate Map Types")]
    public void GenerateAndSaveMapBytes()
    {
        // 1. Загружаем данные карты заново, чтобы быть уверенным, что массив существует
        currentMapData = MapDataConverter.LoadMapDataFromResources(mapDataFileName);
        if (currentMapData == null)
        {
            Debug.LogError("Не удалось загрузить данные карты!");
            return;
        }

        int w = currentMapData.width;
        int h = currentMapData.height;
        int totalPixels = w * h;

        // Создаем временный NativeArray для Job
        NativeArray<byte> tempNativeMapData = new NativeArray<byte>(currentMapData.terrainData, Allocator.TempJob);

        var tileTypes = new NativeArray<int>(totalPixels, Allocator.TempJob);
        var tileColors = new NativeArray<float4>(totalPixels, Allocator.TempJob);
        var spawnTree = new NativeArray<bool>(totalPixels, Allocator.TempJob);
        var treeData = new NativeArray<TreeData>(totalPixels, Allocator.TempJob);

        var job = new TileGenerationJob
        {
            mapData = tempNativeMapData, // Используем свежесозданный массив
            startX = 0,
            startY = 0,
            width = w,
            height = h,
            mapWidth = w,
            noiseScale = noiseScale,
            radiationScale = radiationScale,
            offsets = new float4(heightOffset.x, heightOffset.y, temperatureOffset.x, temperatureOffset.y),
            offsets2 = new float4(moistureOffset.x, moistureOffset.y, radiationOffset.x, radiationOffset.y),
            waterThreshold = waterThreshold,
            mountainThreshold = mountainThreshold,
            generateColors = 1,

            riversColor = new float4(riversColor.r, riversColor.g, riversColor.b, riversColor.a),
            seaColor = new float4(seaColor.r, seaColor.g, seaColor.b, seaColor.a),
            PystoshTileColor = new float4(PystoshTileColor.r, PystoshTileColor.g, PystoshTileColor.b, PystoshTileColor.a),
            RiverTileColor = new float4(RiverTileColor.r, RiverTileColor.g, RiverTileColor.b, RiverTileColor.a),
            SnowPystoshTileColor = new float4(SnowPystoshTileColor.r, SnowPystoshTileColor.g, SnowPystoshTileColor.b, SnowPystoshTileColor.a),

            tileTypes = tileTypes,
            tileColors = tileColors,
            spawnTree = spawnTree,
            treeData = treeData
        };

        JobHandle handle = job.Schedule(totalPixels, 64);
        handle.Complete();

        List<int> bytes = new List<int>();
        Color32[] colors = new Color32[totalPixels];

        for (int i = 0; i < totalPixels; i++)
        {
            int c = tileTypes[i];
            // Убеждаемся, что значения зажаты между 0 и 1
            bytes[i] = c;
        }
        MapTypesData data = new MapTypesData();
        data.items = bytes;
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(Path.Combine(Application.dataPath, "GeneratedMap.json"), json);

        // Очистка
        tempNativeMapData.Dispose();
        tileTypes.Dispose();
        tileColors.Dispose();
        spawnTree.Dispose();
        treeData.Dispose();

        AssetDatabase.Refresh();
    }
    public class MapTypesData
    {
        public List<int> items = new();
    }
#endif
}