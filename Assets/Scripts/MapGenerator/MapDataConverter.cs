using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public byte[] terrainData; // 0 = вода, 1 = земля, 2 = река
    public CompressionType compressionType; // Добавляем тип сжатия
}

public enum CompressionType
{
    None = 0,
    FourBit = 1,
    RLE = 2,
    Hybrid = 3
}

public class MapDataConverter : MonoBehaviour
{
    [Header("Conversion Settings")]
    public Texture2D sourceTexture;
    public string outputFileName = "mapData";

    [Header("Compression Settings")]
    public bool useHybridCompression = true;

    [ContextMenu("Convert Texture to Map Data")]
    public void ConvertTexture()
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is not assigned!");
            return;
        }

        MapData mapData = useHybridCompression ?
            ConvertTextureToMapDataHybrid(sourceTexture) :
            ConvertTextureToMapData(sourceTexture);

        SaveMapDataToFile(mapData, outputFileName);

        Debug.Log($"Conversion complete! Map size: {mapData.width}x{mapData.height}");
        Debug.Log($"Data size: {mapData.terrainData.Length} bytes");
        Debug.Log($"Compression: {mapData.compressionType}");
    }

    [ContextMenu("Convert Texture to Map Data compressed")]
    public void ConvertTextureCompressed()
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is not assigned!");
            return;
        }

        MapData mapData = ConvertTextureToMapDataCompressed(sourceTexture);
        SaveMapDataToFile(mapData, outputFileName);

        Debug.Log($"Conversion complete! Map size: {mapData.width}x{mapData.height}");
        Debug.Log($"Data size: {mapData.terrainData.Length} bytes");
    }

    // Стандартный метод без сжатия
    public static MapData ConvertTextureToMapData(Texture2D texture)
    {
        MapData data = new MapData();
        data.width = texture.width;
        data.height = texture.height;
        data.compressionType = CompressionType.None;
        data.terrainData = new byte[data.width * data.height];

        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            data.terrainData[i] = ColorToByte(pixels[i]);
        }

        return data;
    }

    // 4-битное сжатие
    public static MapData ConvertTextureToMapDataCompressed(Texture2D texture)
    {
        MapData data = new MapData();
        data.width = texture.width;
        data.height = texture.height;
        data.compressionType = CompressionType.FourBit;

        // Каждый байт хранит 2 пикселя (4 бита на пиксель)
        int compressedSize = (data.width * data.height + 1) / 2;
        data.terrainData = new byte[compressedSize];

        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i += 2)
        {
            byte firstPixel = ColorToByte(pixels[i]);
            byte secondPixel = (i + 1 < pixels.Length) ? ColorToByte(pixels[i + 1]) : (byte)0;

            // Упаковываем 2 пикселя в один байт
            data.terrainData[i / 2] = (byte)((firstPixel << 4) | secondPixel);
        }

        return data;
    }

    // Гибридное сжатие (4-битное + RLE)
    public static MapData ConvertTextureToMapDataHybrid(Texture2D texture)
    {
        MapData data = new MapData();
        data.width = texture.width;
        data.height = texture.height;
        data.compressionType = CompressionType.Hybrid;

        Color32[] pixels = texture.GetPixels32();
        List<byte> compressedData = new List<byte>();

        // Сначала создаем 4-битные данные
        byte[] nibbleData = new byte[(pixels.Length + 1) / 2];
        for (int i = 0; i < pixels.Length; i += 2)
        {
            byte firstPixel = ColorToByte(pixels[i]);
            byte secondPixel = (i + 1 < pixels.Length) ? ColorToByte(pixels[i + 1]) : (byte)0;
            nibbleData[i / 2] = (byte)((firstPixel << 4) | secondPixel);
        }

        // Затем применяем RLE к 4-битным данным
        byte currentByte = nibbleData[0];
        byte runLength = 1;

        for (int i = 1; i < nibbleData.Length; i++)
        {
            if (nibbleData[i] == currentByte && runLength < 255)
            {
                runLength++;
            }
            else
            {
                compressedData.Add(runLength);
                compressedData.Add(currentByte);

                currentByte = nibbleData[i];
                runLength = 1;
            }
        }

        // Записываем последний прогон
        compressedData.Add(runLength);
        compressedData.Add(currentByte);

        data.terrainData = compressedData.ToArray();
        return data;
    }

    private static byte ColorToByte(Color32 color)
    {
        if (IsColorRiver(color)) return 2;
        if (IsColorLand(color)) return 1;
        return 0;
    }

    private static bool IsColorWater(Color32 color)
    {
        // Синий цвет - вода
        return color.r < 100 && color.g < 100 && color.b > 150;
    }

    private static bool IsColorRiver(Color32 color)
    {
        // Голубой/синий - река
        return color.r < 100 && color.g > 100 && color.g < 200 && color.b > 150;
    }

    private static bool IsColorLand(Color32 color)
    {
        // Белый/серый - земля для генерации
        return color.r > 200 && color.g > 200 && color.b > 200;
    }

    private void SaveMapDataToFile(MapData mapData, string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Resources", fileName + ".bytes");

        using (FileStream fs = new FileStream(path, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(mapData.width);
            writer.Write(mapData.height);
            writer.Write((int)mapData.compressionType); // Сохраняем тип сжатия
            writer.Write(mapData.terrainData.Length);
            writer.Write(mapData.terrainData);
        }

        Debug.Log($"Map data saved to: {path}");
    }

    // ОСНОВНОЙ МЕТОД ЗАГРУЗКИ С АВТОМАТИЧЕСКОЙ ДЕКОМПРЕССИЕЙ
    public static MapData LoadMapDataFromResources(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset == null)
        {
            Debug.LogError($"Map data file not found: {fileName}");
            return null;
        }

        using (MemoryStream ms = new MemoryStream(textAsset.bytes))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            MapData data = new MapData();
            data.width = reader.ReadInt32();
            data.height = reader.ReadInt32();
            data.compressionType = (CompressionType)reader.ReadInt32(); // Читаем тип сжатия
            int dataLength = reader.ReadInt32();
            data.terrainData = reader.ReadBytes(dataLength);

            // Автоматически декомпрессируем данные при загрузке
            data = DecompressMapData(data);

            return data;
        }
    }

    // МЕТОД ДЕКОМПРЕССИИ ДАННЫХ
    private static MapData DecompressMapData(MapData compressedData)
    {
        if (compressedData.compressionType == CompressionType.None)
        {
            return compressedData; // Данные уже декомпрессированы
        }

        MapData decompressedData = new MapData();
        decompressedData.width = compressedData.width;
        decompressedData.height = compressedData.height;
        decompressedData.compressionType = CompressionType.None; // Помечаем как декомпрессированные

        int totalPixels = compressedData.width * compressedData.height;

        switch (compressedData.compressionType)
        {
            case CompressionType.FourBit:
                decompressedData.terrainData = DecompressFourBitData(compressedData.terrainData, totalPixels);
                break;

            case CompressionType.Hybrid:
                decompressedData.terrainData = DecompressHybridData(compressedData.terrainData, totalPixels);
                break;

            case CompressionType.RLE:
                decompressedData.terrainData = DecompressRLEData(compressedData.terrainData, totalPixels);
                break;

            default:
                Debug.LogWarning($"Unknown compression type: {compressedData.compressionType}");
                decompressedData.terrainData = compressedData.terrainData;
                break;
        }

        Debug.Log($"Decompressed data from {compressedData.terrainData.Length} bytes to {decompressedData.terrainData.Length} bytes");
        return decompressedData;
    }

    // Декомпрессия 4-битных данных
    private static byte[] DecompressFourBitData(byte[] compressedData, int totalPixels)
    {
        byte[] decompressed = new byte[totalPixels];

        for (int i = 0; i < totalPixels; i++)
        {
            int compressedIndex = i / 2;
            if (compressedIndex < compressedData.Length)
            {
                byte compressedByte = compressedData[compressedIndex];

                if (i % 2 == 0)
                    decompressed[i] = (byte)((compressedByte >> 4) & 0x0F); // Первые 4 бита
                else
                    decompressed[i] = (byte)(compressedByte & 0x0F); // Вторые 4 бита
            }
        }

        return decompressed;
    }

    // Декомпрессия гибридных данных
    private static byte[] DecompressHybridData(byte[] hybridData, int totalPixels)
    {
        List<byte> nibbleData = new List<byte>();

        // Распаковываем RLE
        for (int i = 0; i < hybridData.Length; i += 2)
        {
            byte runLength = hybridData[i];
            byte dataByte = hybridData[i + 1];

            for (int j = 0; j < runLength; j++)
            {
                nibbleData.Add(dataByte);
            }
        }

        // Конвертируем 4-битные данные обратно в байты
        byte[] decompressed = new byte[totalPixels];
        for (int i = 0; i < totalPixels; i++)
        {
            int nibbleIndex = i / 2;
            if (nibbleIndex < nibbleData.Count)
            {
                byte compressedByte = nibbleData[nibbleIndex];

                if (i % 2 == 0)
                    decompressed[i] = (byte)((compressedByte >> 4) & 0x0F);
                else
                    decompressed[i] = (byte)(compressedByte & 0x0F);
            }
        }

        return decompressed;
    }

    // Декомпрессия RLE данных (на случай если понадобится)
    private static byte[] DecompressRLEData(byte[] rleData, int totalPixels)
    {
        byte[] decompressed = new byte[totalPixels];
        int decompressedIndex = 0;

        for (int i = 0; i < rleData.Length; i += 2)
        {
            byte runLength = rleData[i];
            byte terrainType = rleData[i + 1];

            for (int j = 0; j < runLength; j++)
            {
                if (decompressedIndex < totalPixels)
                {
                    decompressed[decompressedIndex++] = terrainType;
                }
            }
        }

        return decompressed;
    }

    // Вспомогательный метод для получения типа terrain (работает с декомпрессированными данными)
    public static byte GetTerrainType(MapData mapData, int x, int y)
    {
        if (mapData.compressionType != CompressionType.None)
        {
            Debug.LogWarning("GetTerrainType called on compressed data. Data should be decompressed first.");
        }

        int index = y * mapData.width + x;
        if (index >= 0 && index < mapData.terrainData.Length)
        {
            return mapData.terrainData[index];
        }
        return 0; // Вода по умолчанию
    }
}