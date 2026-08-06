using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ImageGen : MonoBehaviour
{
    [Header("Images")]
    public Texture2D sourceImage; // исходное изображение для обучения[span_2](end_span)
    public Texture2D targetImage;
    // базовое изображение для старта генерации[span_3](end_span)

   [Header("Tiles")]
    public TileBase groundTile; // тайл для значений < 0.25[span_4](end_span)
    public TileBase fonTile; // тайл для значений 0.25-0.75[span_5](end_span)
    public TileBase centerTile; // тайл для значений > 0.75[span_6](end_span)
    public Tilemap tilemap;

    [Header("Generation Settings")]
    [Tooltip("Сколько раз сеть будет перерисовывать картинку на основе своих же соседей")]
    public int iterations = 5;

    [Tooltip("Сила шума для изначальной картинки (чтобы генерация отличалась от оригинала)")]
    [Range(0f, 1f)]
    public float noiseLevel = 0.3f;

    private SimpleNeuralNetwork neuralNetwork;
    private List<float[]> trainingInputs = new List<float[]>();
    private List<float[]> trainingOutputs = new List<float[]>();

    private void Start()
    {
        PrepareTrainingData();
        InitializeAndTrainNetwork();
    }

    private void PrepareTrainingData()
    {
        Color32[] pixels = sourceImage.GetPixels32();
        int width = sourceImage.width;
        int height = sourceImage.height;

        float[] pixelValues = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixelValues[i] = ColorToValue(pixels[i]);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                float left = GetPixelValueSafe(pixelValues, x - 1, y, width, height);
                float right = GetPixelValueSafe(pixelValues, x + 1, y, width, height);
                float up = GetPixelValueSafe(pixelValues, x, y + 1, width, height);
                float down = GetPixelValueSafe(pixelValues, x, y - 1, width, height);

                trainingInputs.Add(new float[] { left, right, up, down });
                trainingOutputs.Add(new float[] { pixelValues[index] });
            }
        }
    }

    // Вынес конвертацию цвета в отдельный метод для чистоты кода
    private float ColorToValue(Color32 p)
    {
        if (p.r == 0 && p.g == 0 && p.b == 0) return 0f;          // черный
        if (p.r == 255 && p.g == 255 && p.b == 255) return 0.5f;  // белый
        if (p.r == 0 && p.g == 0 && p.b == 255) return 1f;        // синий
        return 0.5f; // по умолчанию (белый)
    }

    private float GetPixelValueSafe(float[] pixelValues, int x, int y, int width, int height)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return 0.5f; // Возвращаем нейтральное значение для границ[span_22](end_span)

       return pixelValues[y * width + x];
    }

    private void InitializeAndTrainNetwork()
    {
        neuralNetwork = new SimpleNeuralNetwork(4, 3, 2, 1);

        int epochs = 1000;
        float learningRate = 0.1f;

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int i = 0; i < trainingInputs.Count; i++)
            {
                neuralNetwork.Train(trainingInputs[i], trainingOutputs[i], learningRate);
            }
        }
        Debug.Log("Network trained!");
    }

    [ContextMenu("Generate")]
    public void GenerateTilemap()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target image is not assigned!");
            return;
        }

        Color32[] pixels = targetImage.GetPixels32();
        int width = targetImage.width;
        int height = targetImage.height;

        float[] currentValues = new float[pixels.Length];

        // 1. Инициализируем карту значений, добавляя случайный шум
        for (int i = 0; i < pixels.Length; i++)
        {
            float val = ColorToValue(pixels[i]);

            // С вероятностью noiseLevel заменяем оригинальный пиксель случайным значением
            if (UnityEngine.Random.value < noiseLevel)
            {
                float r = UnityEngine.Random.value;
                if (r < 0.33f) val = 0f;
                else if (r < 0.66f) val = 0.5f;
                else val = 1f;
            }
            currentValues[i] = val;
        }

        float[] nextValues = new float[currentValues.Length];

        // 2. Итеративная генерация (Клеточный автомат)
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    // Соседи берутся из ТЕКУЩЕГО сгенерированного состояния, а не из оригинала
                    float left = GetPixelValueSafe(currentValues, x - 1, y, width, height);
                    float right = GetPixelValueSafe(currentValues, x + 1, y, width, height);
                    float up = GetPixelValueSafe(currentValues, x, y + 1, width, height);
                    float down = GetPixelValueSafe(currentValues, x, y - 1, width, height);

                    float[] input = new float[] { left, right, up, down };
                    float[] prediction = neuralNetwork.Predict(input);

                    // Жестко привязываем предсказание к 0, 0.5 или 1
                    nextValues[index] = SnapValue(prediction[0]);
                }
            }
            // Обновляем текущие значения для следующей итерации
            Array.Copy(nextValues, currentValues, nextValues.Length);
        }

        // 3. Выбор тайлов на основе итоговых значений
        TileBase[] tiles = new TileBase[pixels.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            float val = currentValues[i];
            if (val > 0.75f)
                tiles[i] = centerTile;
            else if (val > 0.25f)
                tiles[i] = fonTile;
            else
                tiles[i] = groundTile;
        }

        // 4. Отрисовка
        tilemap.ClearAllTiles();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[index]);
            }
        }

        Debug.Log("Tilemap generation completed!");
    }

    // Вспомогательный метод для округления значений (чтобы генерация не размывалась)
    private float SnapValue(float val)
    {
        if (val > 0.75f) return 1f;
        if (val > 0.25f) return 0.5f;
        return 0f;
    }
}
