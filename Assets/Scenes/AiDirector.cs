using System.IO;
using UnityEngine;

public class AiDirector : MonoBehaviour
{
    private SimpleNeuralNetwork net;

    // Настройки
    [Header("Settings")]
    public int epochs = 5000; // Сколько раз прогнать обучение
    public float learningRate = 0.1f;

    // Для визуализации в инспекторе
    [Header("Manual Test")]
    public float[] inputTest = new float[4];
    public float[] resultOutput;
    public float[] target;
    public SimpleNeuralNetwork net1;

    void Start()
    {
        // Создаем сеть:
        // 4 входа (наши числа)
        // 4 скрытых нейрона (можно менять, например, 5 или 3)
        // 2 выхода (класс 1 или класс 2)
        net = new SimpleNeuralNetwork(4, 4, 2);

        Debug.Log("Начинаем обучение...");
        TrainNetwork();
        Debug.Log("Обучение завершено. Проверяем...");
        TestNetwork();
        net.SaveToFile(Path.Combine(Application.persistentDataPath, "ai.json"));
        Debug.Log(Mathf.Exp(100));
        Debug.Log(Mathf.Exp(-100));
        Debug.Log(Mathf.Exp(9));
        Debug.Log(Mathf.Exp(-9));
    }

    void TrainNetwork()
    {
        for (int i = 0; i < epochs; i++)
        {
            // Генерируем случайный пример
            float[] inputs = new float[4];
            for (int j = 0; j < 4; j++) inputs[j] = UnityEngine.Random.Range(0f, 1f);

            // Определяем правильный ответ
            float sumFirst = inputs[0] + inputs[1];
            float sumLast = inputs[2] + inputs[3];

            float[] targets = new float[2];

            if (sumFirst > sumLast)
            {
                targets[0] = 1f; // [1, 0] - первые больше
                targets[1] = 0f;
            }
            else
            {
                targets[0] = 0f; // [0, 1] - последние больше
                targets[1] = 1f;
            }

            // Учим сеть на этом примере
            net.Train(inputs, targets, learningRate);
        }
    }

    void TestNetwork()
    {
        // Проведем 5 контрольных тестов
        for (int i = 0; i < 5; i++)
        {
            float[] inputs = new float[4];
            for (int j = 0; j < 4; j++) inputs[j] = UnityEngine.Random.Range(0f, 1f);

            float[] outputs = net.Predict(inputs);

            float sumFirst = inputs[0] + inputs[1];
            float sumLast = inputs[2] + inputs[3];
            string expected = sumFirst > sumLast ? "Первые >" : "Последние >";
            string predicted = outputs[0] > outputs[1] ? "Первые >" : "Последние >";

            string color = expected == predicted ? "<color=green>OK</color>" : "<color=red>FAIL</color>";

            Debug.Log($"Вход: [{inputs[0]:F2}, {inputs[1]:F2} | {inputs[2]:F2}, {inputs[3]:F2}] " +
                      $"Ожидали: {expected} | Сеть: {outputs[0]:F2}, {outputs[1]:F2} ({predicted}) -> {color}");
        }
    }

    // Кнопка для теста из Инспектора (Context Menu)
    [ContextMenu("Run Manual Test")]
    public void RunManualTest()
    {
        if (net == null) return;
        resultOutput = net.Predict(inputTest);
        Debug.Log($"Manual Test: Output [{resultOutput[0]}, {resultOutput[1]}]");
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
}