using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class NoisePerlin : MonoBehaviour
{
    public Tilemap ground;
    public RuleTile[] grasses;

    public float temperatureScale = 0.1f;
    public float humidityScale = 0.1f;
    public float dirtScale = 0.1f;

    public float waterThreshold = 0.3f;

    public List<Vector3Int> waterPosition;

    public int width = 256;
    public int height = 256;

    public void GenerateWorld()
    {
        waterPosition = new List<Vector3Int>();
        StartCoroutine(GenerateWorldCoroutine());
    }

    IEnumerator GenerateWorldCoroutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float temperature = Mathf.PerlinNoise(x * temperatureScale, y * temperatureScale);
                float humidity = Mathf.PerlinNoise(x * humidityScale, y * humidityScale);
                float dirtNoise = Mathf.PerlinNoise(x * dirtScale, y * dirtScale);

                Color biomeColor = GetBiomeColor(temperature, humidity, dirtNoise, x, y);
                ground.SetTile(new Vector3Int(x, y, 0), grasses[1]); // Use a default tile
                ground.SetColor(new Vector3Int(x, y, 0), biomeColor);

                if (dirtNoise < waterThreshold)
                {
                    waterPosition.Add(new Vector3Int(x, y, 0));
                    ground.SetTile(new Vector3Int(x, y, 0), grasses[0]);
                }
            }
            yield return new WaitForSeconds(0.001f);
        }
        Debug.Log(waterPosition.Count);
    }

    Color GetBiomeColor(float temperature, float humidity, float dirtNoise, int x, int y)
    {
        Color biomeColor = Color.white; // Default color

        float desertTempThreshold = 0.7f;
        float forestHumidityThreshold = 0.6f;
        float snowTempThreshold = 0.3f;

        if (temperature > desertTempThreshold && humidity < forestHumidityThreshold) // Desert
        {
            float sandColorValue = Mathf.InverseLerp(desertTempThreshold, 1f, temperature);
            biomeColor = Color.Lerp(new Color(0.94f, 0.9f, 0.74f), new Color(0.89f, 0.82f, 0.67f), sandColorValue);
        }
        else if (temperature < snowTempThreshold) // Snow
        {
            float snowColorValue = Mathf.InverseLerp(0f, snowTempThreshold, temperature);
            biomeColor = Color.Lerp(Color.white, new Color(0.8f, 0.9f, 1f), snowColorValue);
        }
        else if (humidity > forestHumidityThreshold) // Forest
        {
            float forestColorValue = Mathf.InverseLerp(forestHumidityThreshold, 1f, humidity);
            biomeColor = Color.Lerp(new Color(0.2f, 0.6f, 0.2f), new Color(0.1f, 0.4f, 0.1f), forestColorValue);
        }
        else // Dirt/Grassland
        {
            float dirtValueRemapped = Mathf.InverseLerp(0.4f, 0.7f, dirtNoise); // Adjust these values to fine tune dirt vs. grass transition
            float smoothStepValue = Mathf.SmoothStep(0f, 1f, dirtValueRemapped);

            Color dirtColor = new Color(160 / 255f, 120 / 255f, 80 / 255f);
            Color grassColor = new Color(94 / 255f, 144 / 255f, 94 / 255f);

            biomeColor = Color.Lerp(dirtColor, grassColor, smoothStepValue);

        }
        return biomeColor;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GenerateWorld();
        }
    }
}
