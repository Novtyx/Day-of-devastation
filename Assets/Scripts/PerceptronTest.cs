using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PerceptronTest : MonoBehaviour
{
    private SimpleNeuralNetwork net;

    // Настройки
    [Header("Settings")]
    public int epochs = 5000; // Сколько раз прогнать обучение
    public float learningRate = 0.05f;

    // Для визуализации в инспекторе
    [Header("Manual Test")]
    public float[] inputTest = new float[1];
    public float[] resultOutput;
    public float[] target;
    public SimpleNeuralNetwork net1;
    public bool train = true;
    public bool isTimming = false;
    public TMP_Text text;
    public GameObject obj;

    void Start()
    {
        // Создаем сеть:
        // 4 входа (наши числа)
        // 4 скрытых нейрона (можно менять, например, 5 или 3)
        // 2 выхода (класс 1 или класс 2)
        net = new SimpleNeuralNetwork(4, 8, 4, 1);

        Debug.Log("Начинаем обучение...");
        //TrainNetwork();
        Debug.Log("Обучение завершено. Проверяем...");
        //TestNetwork();
        StartCoroutine(TrainAi());
    }

    void TrainNetwork()
    {
        for (int i = 0; i < epochs; i++)
        {
            // Генерируем случайный пример
            float[] inputs = new float[1];
            inputs[0] = UnityEngine.Random.Range(0f, 0.5f);

            float[] targets = new float[1];
            targets[0] = inputs[0] * 2;

            

            // Учим сеть на этом примере
            net.Train(inputs, targets, learningRate);
        }
    }

    void TestNetwork()
    {
        // Проведем 5 контрольных тестов
        for (int i = 0; i < 5; i++)
        {
            // Генерируем случайный пример
            float[] inputs = new float[1];
            inputs[0] = UnityEngine.Random.Range(0f, 1f);

            float[] targets = new float[1];
            targets[0] = inputs[0] * 2;

            float[] outputs = net.Predict(inputs);
        }
    }

    // Кнопка для теста из Инспектора (Context Menu)
    [ContextMenu("Run Manual Test")]
    public void RunManualTest()
    {
        if (net == null) return;
        resultOutput = net.Predict(inputTest);
        Debug.Log($"Manual Test: Output [{resultOutput[0]}]");
    }
    public void initnet1()
    {
        net1 = new SimpleNeuralNetwork(1, 2, 1);
    }
    public void net1Train()
    {
        net1.Train(inputTest, target, learningRate);
    }
    public void predict()
    {
        resultOutput = net1.Predict(inputTest);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) isTimming = !isTimming;
    }
    public IEnumerator TrainAi()
    {
        while (train)
        {
            yield return new WaitForSeconds(0.01f);
            if (!isTimming)
            {
                // Генерируем случайный пример
                float[] inputs = new float[4];
                inputs[0] = Input.mousePosition.x / 10000f;
                inputs[1] = Input.mousePosition.y / 10000f;
                yield return new WaitForSeconds(0.01f);
                inputs[2] = Input.mousePosition.x / 10000f;
                inputs[3] = Input.mousePosition.y / 10000f;
                float[] targets = new float[1];
                targets[0] = 0f;


                text.text = $"{0}";
                // Учим сеть на этом примере
                net.Train(inputs, targets, learningRate);
            }
            if (isTimming)
            {
                // Генерируем случайный пример
                float[] inputs = new float[4];
                inputs[0] = Input.mousePosition.x / 10000f;
                inputs[1] = Input.mousePosition.y / 10000f;
                yield return new WaitForSeconds(0.01f);
                inputs[2] = Input.mousePosition.x / 10000f;
                inputs[3] = Input.mousePosition.y / 10000f;
                float[] targets = new float[1];
                targets[0] = 1f;


                text.text = $"{targets[0]}";
                // Учим сеть на этом примере
                net.Train(inputs, targets, learningRate);
            }
        }
        while (!train)
        {
            float[] inputs = new float[4];
            inputs[0] = Input.mousePosition.x / 10000f;
            inputs[1] = Input.mousePosition.y / 10000f;
            yield return new WaitForSeconds(0.01f);
            inputs[2] = Input.mousePosition.x / 10000f;
            inputs[3] = Input.mousePosition.y / 10000f;
            float[] targets = new float[1];
            targets = net.Predict(inputs);
            text.text = $"{targets[0]}";
        }
    }
}