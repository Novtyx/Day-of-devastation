using UnityEngine;
using System;
using System.IO;

// Класс одного нейрона
[System.Serializable]
public class Neuron
{
    public float[] weights;    // Веса
    public float bias;         // Смещение
    public float output;       // Выходное значение (после активации)
    public float delta;        // Значение ошибки (градиент) для этого нейрона

    public Neuron(int inputCount)
    {
        weights = new float[inputCount];
        // Инициализация Xavier/Glorot (лучше, чем просто Random.Range(-1, 1))
        // Помогает сети быстрее учиться
        float initRange = Mathf.Sqrt(2.0f / inputCount);

        bias = UnityEngine.Random.Range(-initRange, initRange);
        for (int i = 0; i < inputCount; i++)
        {
            weights[i] = UnityEngine.Random.Range(-initRange, initRange);
        }
    }

    public float FeedForward(float[] inputs)
    {
        float sum = bias;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += inputs[i] * weights[i];
        }
        output = Sigmoid(sum);
        return output;
    }

    private float Sigmoid(float x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }

    // Производная сигмоиды, принимающая уже вычисленный output (y)
    // f'(x) = f(x) * (1 - f(x))
    public static float SigmoidDerivativeFromOutput(float output)
    {
        return output * (1.0f - output);
    }
}

// Класс слоя
[System.Serializable]
public class Layer
{
    public Neuron[] neurons;
    public float[] inputs; // Запоминаем входы для Backpropagation

    public Layer(int neuronCount, int inputCount)
    {
        neurons = new Neuron[neuronCount];
        for (int i = 0; i < neuronCount; i++)
        {
            neurons[i] = new Neuron(inputCount);
        }
    }

    public float[] FeedForward(float[] inputValues)
    {
        // Сохраняем копию входов для фазы обучения
        inputs = new float[inputValues.Length];
        Array.Copy(inputValues, inputs, inputValues.Length);

        float[] outputs = new float[neurons.Length];
        for (int i = 0; i < neurons.Length; i++)
        {
            outputs[i] = neurons[i].FeedForward(inputValues);
        }
        return outputs;
    }
}

// Главный класс нейросети
[System.Serializable]
public class SimpleNeuralNetwork
{
    public Layer[] layers;

    // Размеры сети: например [4, 5, 2] -> 4 входа, 5 скрытых, 2 выхода
    public SimpleNeuralNetwork(params int[] layerSizes)
    {
        if (layerSizes.Length < 2)
        {
            Debug.LogError("Нейросеть должна иметь минимум 2 слоя (входной размер и выходной размер)!");
            return;
        }

        layers = new Layer[layerSizes.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            int inputsCount = layerSizes[i];
            int neuronsCount = layerSizes[i + 1];
            layers[i] = new Layer(neuronsCount, inputsCount);
            Debug.Log($"creaed layer neurons: {neuronsCount} x {inputsCount}");
        }
    }

    public float[] Predict(float[] inputs)
    {
        float[] currentOutputs = inputs;
        for (int i = 0; i < layers.Length; i++)
        {
            currentOutputs = layers[i].FeedForward(currentOutputs);
        }
        return currentOutputs;
    }

    // Полноценное обучение методом обратного распространения (Backpropagation)
    public void Train(float[] inputs, float[] targets, float learningRate = 0.1f)
    {
        // 1. Прямой проход
        float[] outputs = Predict(inputs);

        // 2. Вычисление градиентов (Delta)

        // А) Для ВЫХОДНОГО слоя
        Layer outputLayer = layers[layers.Length - 1];
        for (int i = 0; i < outputLayer.neurons.Length; i++)
        {
            Neuron neuron = outputLayer.neurons[i];

            float error = targets[i] - neuron.output; // Ошибка = Цель - Факт
            // Delta = Ошибка * Производная(Выход)
            neuron.delta = error * Neuron.SigmoidDerivativeFromOutput(neuron.output);
        }

        // Б) Для СКРЫТЫХ слоев (идем от конца к началу)
        for (int i = layers.Length - 2; i >= 0; i--)
        {
            Layer currentLayer = layers[i];
            Layer nextLayer = layers[i + 1];

            for (int j = 0; j < currentLayer.neurons.Length; j++)
            {
                Neuron neuron = currentLayer.neurons[j];

                // Суммируем ошибки от нейронов следующего слоя, взвешенные их весами
                float errorSum = 0;
                for (int k = 0; k < nextLayer.neurons.Length; k++)
                {
                    Neuron nextNeuron = nextLayer.neurons[k];
                    // nextNeuron.weights[j] - это вес связи от ТЕКУЩЕГО нейрона (j) к СЛЕДУЮЩЕМУ (k)
                    errorSum += nextNeuron.delta * nextNeuron.weights[j];
                }

                neuron.delta = errorSum * Neuron.SigmoidDerivativeFromOutput(neuron.output);
            }
        }

        // 3. Обновление весов (Gradient Descent)
        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                // Обновляем bias
                neuron.bias += neuron.delta * learningRate;

                // Обновляем веса
                for (int w = 0; w < neuron.weights.Length; w++)
                {
                    // Вес += Дельта * ВходноеЗначение * СкоростьОбучения
                    neuron.weights[w] += neuron.delta * layer.inputs[w] * learningRate;
                }
            }
        }
    }

    public void Forget(float decayRate, float targetOutput = 0.2f)
    {
        float targetBias = Mathf.Log(targetOutput / (1f - targetOutput));

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                for (int w = 0; w < neuron.weights.Length; w++)
                {
                    // Вес += Дельта * ВходноеЗначение * СкоростьОбучения
                    neuron.weights[w] = Mathf.Lerp(neuron.weights[w], 0f, decayRate);
                }
                neuron.bias = Mathf.Lerp(neuron.bias, targetBias, decayRate);
            }
        }
    }

    // --- Методы сохранения (без изменений, кроме проверки директории) ---
    public void SaveToFile(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        // Создаем папку, если её нет (требуется для StreamingAssets в редакторе)
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved to {path}");
    }

    public static SimpleNeuralNetwork LoadFromFile(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SimpleNeuralNetwork>(json);
        }
        Debug.LogError("File not found");
        return null;
    }
}