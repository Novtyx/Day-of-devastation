using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCurner : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 32;
    public int mapHeight = 18;
    public int tileSize = 32;

    [Header("Tile Colors")]
    public Color grassColor = Color.green;
    public Color waterColor = Color.blue;
    public Color sandColor = Color.yellow;
    public Color mountainColor = Color.gray;

    [Header("References")]
    public Tilemap tilemap;
    public TileBase grassTile;
    public TileBase waterTile;
    public TileBase sandTile;
    public TileBase mountainTile;

    private byte[,] mapData;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        // 1. Генерируем базовую карту 32x18 байтов
        mapData = new byte[mapWidth, mapHeight];
        GenerateBaseMap();

        // 2. Создаем тайлы и применяем к тайлмапу
        CreateTiles();
    }

    void GenerateBaseMap()
    {
        // Простой пример генерации - можно заменить на свою логику
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Пример: градиент от воды к траве
                float noiseValue = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);

                if (noiseValue < 0.3f)
                    mapData[x, y] = 0; // Вода
                else if (noiseValue < 0.4f)
                    mapData[x, y] = 1; // Песок
                else if (noiseValue < 0.7f)
                    mapData[x, y] = 2; // Трава
                else
                    mapData[x, y] = 3; // Горы
            }
        }
    }

    void CreateTiles()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                TileBase tile = GetTileForData(mapData[x, y]);
                tilemap.SetTile(tilePosition, tile);
            }
        }

        // Обрабатываем угловые клетки
        ProcessCornerTiles();
    }

    TileBase GetTileForData(byte data)
    {
        switch (data)
        {
            case 0: return waterTile;
            case 1: return sandTile;
            case 2: return grassTile;
            case 3: return mountainTile;
            default: return grassTile;
        }
    }

    void ProcessCornerTiles()
    {
        // Обрабатываем четыре угла
        ProcessSingleCorner(0, 0, true, true); // Левый нижний
        ProcessSingleCorner(mapWidth - 1, 0, false, true); // Правый нижний
        ProcessSingleCorner(0, mapHeight - 1, true, false); // Левый верхний
        ProcessSingleCorner(mapWidth - 1, mapHeight - 1, false, false); // Правый верхний
    }

    void ProcessSingleCorner(int x, int y, bool isLeft, bool isBottom)
    {
        Vector3Int position = new Vector3Int(x, y, 0);

        // Создаем специальный тайл для угла с диагональным срезом
        // В реальной реализации здесь будет создание/установка специального тайла
        // с диагональным срезом через шейдер или спрайт

        // Временная реализация - просто ставим другой цвет
        tilemap.SetTileFlags(position, TileFlags.None);
        tilemap.SetColor(position, Color.black); // Маркер для углов
    }
}

// Класс для кастомных тайлов с поддержкой диагональных срезов
[CreateAssetMenu(fileName = "DiagonalTile", menuName = "2D/Tiles/Diagonal Tile")]
public class DiagonalTile : TileBase
{
    public Sprite sprite;
    public Color color = Color.white;
    public DiagonalType diagonalType;

    public enum DiagonalType
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.color = color;
        tileData.colliderType = Tile.ColliderType.Sprite;
    }
}