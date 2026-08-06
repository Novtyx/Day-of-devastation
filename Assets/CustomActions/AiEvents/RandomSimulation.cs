using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomSimulation : MonoBehaviour
{
    public static RandomSimulation instance;

    private SimpleNeuralNetwork brain;
    private float[][] trainingInputs;
    private float[][] trainingOutputs;
    private int trainingIndex = 0;
    public float[] inputs;
    public float[] outputs;
    public float death1 = 0.1f;
    public float energy1 = 0.1f;
    public float power1 = 0.1f;
    public float rad1 = 0.1f;
    public float tension1 = 0.1f;
    public float time1 = 0.1f;
    public float hunger1 = 0.1f;
    public float currentAggression1 = 0.1f;
    public bool train = true;
    // 0:истощение, 1:бодрость, 2:Сила, 3:радиация,
    // 4:напряжение, 5:времяБезСобытий, 6:сытость, 7:агрессивностьИгрока
    public const int INPUT_SIZE = 8;
    // 0:ничего, 1:пакость, 2:увечье, 3:помощь, 4:погода
    public const int OUTPUT_SIZE = 5;
    [Range(0, 1)] public float currentTension = 0f;
    public float timeSinceLastEvent = 0f;
    [Range(0, 1)] public float currentAggression = 0.5f;
    public float threatPoints;
    private float[] lastInputs;
    private float checkTimer;
    public float eventCheckInterval = 0.001f;
    public List<AiEvent> allEvents;

    void Start()
    {
        instance = this;
        // Инициализация с примерной архитектурой
        brain = new SimpleNeuralNetwork(INPUT_SIZE, 20, 16, OUTPUT_SIZE);

        InitializeTrainingData();
        TrainOnInitialData(5000);
    }
    // блок теста ии

    public void predict()
    {
        inputs = new float[] { death1, energy1, power1, rad1, tension1, time1, hunger1, currentAggression1 };
        outputs = brain.Predict(inputs);
        InstanceEvent();
    }
    public void battle(bool playerWon)
    {
        float[] idealOutput = new float[OUTPUT_SIZE];
        if (playerWon)
        {
            // Игрок победил -> Ему нравится/он может драться.
            // 1. Повышаем агрессивность
            currentAggression1 = Mathf.Clamp01(currentAggression1 + 0.1f);

            // 2. Учим ИИ: Бой был хорошим выбором!
            idealOutput[2] = 0.8f; // Battle (Поощряем)
            idealOutput[3] = 0.2f; // Loot (Награда)

            Debug.Log("Игрок победил. Режиссер запомнил: можно атаковать.");
            AdjustTension(-0.3f);// Победа расслабляет[span_12](end_span)
        }
        else // Player Died / Lost
        {
            // Если проиграл, но не сбежал -> было слишком сложно.
            // В следующий раз лучше дать помощь.
            idealOutput[3] = 0.9f; // Help/Loot
        }

        // ОБУЧАЕМ НЕЙРОСЕТЬ
        brain.Train(inputs, idealOutput, 0.2f); // Коэффициент обучения выше (0.2), так как это важный опыт
        //SaveBrain();
    }
    public void fled()
    {
        float[] idealOutput = new float[OUTPUT_SIZE];

        // Игрок сбежал -> Он не хочет/не может драться.
        // 1. Снижаем агрессивность
        currentAggression1 = Mathf.Clamp01(currentAggression1 - 0.15f);

        // 2. Учим ИИ: В этой ситуации (HP, Wealth и т.д.) БОЙ был плохим выбором.
        // Лучше бы дал Тишину (0) или Мелкую пакость (1).
        idealOutput[0] = 0.4f; // Chill
        idealOutput[1] = 0.4f; // Minor
        idealOutput[2] = 0.0f; // Battle (Снижаем вероятность до 0)
        idealOutput[3] = 0.1f;
        idealOutput[4] = 0.1f;

        Debug.Log("Игрок сбежал. Режиссер запомнил: меньше боев.");
        AdjustTension(0.2f);// Побег — это стресс[span_11](end_span)

        // ОБУЧАЕМ НЕЙРОСЕТЬ
        brain.Train(inputs, idealOutput, 0.2f); // Коэффициент обучения выше (0.2), так как это важный опыт
        //SaveBrain();
    }

    public void InstanceEvent()
    {
        float[] outp = brain.Predict(inputs);
        float total = 0;
        foreach (float prob in outp)
        {
            total += prob;
        }
        float randomPoint = Random.value * total;
        float cumulative = 0;
        int and = 0;
        for (int i = 0; i < outp.Length; i++)
        {
            cumulative += outp[i];
            if (randomPoint <= cumulative)
            {
                and = i;
                break;
            }
        }
        switch (and)
        {
            case 0: NotifyManager.instance.SetMinNotify("ничего"); AdjustTension(-0.1f); TrainImmidiate(and, 0.1f); break;
            case 1: NotifyManager.instance.SetMinNotify("мелкий вред"); AdjustTension(0.1f); TrainImmidiate(and, 0.1f); break;
            case 2:
                NotifyManager.instance.SetMinNotify("тяжкий вред");
                AdjustTension(0.4f);
                AttackManager.instance.StartAttack(Movement.instance.tiledata[1].enemys[0]);
                break;
            case 3:
                NotifyManager.instance.SetMinNotify("помощь");
                AdjustTension(-0.2f);
                EventManager.instance.StartEvent(Movement.instance.tiledata[1].events[0]);
                break;
            case 4: NotifyManager.instance.SetMinNotify("погода"); TrainImmidiate(and, 0.1f); break;
        }
    }

    private void Update()
    {
        /** for (int i = 0; i < trainingInputs.Length; i++)
         {
             brain.Train(trainingInputs[i], trainingOutputs[i], 0.002f * Time.deltaTime);
         }
        **/
    }

    private void AdjustTension(float v)
    {
        currentTension = Mathf.Clamp01(currentTension + v);
    }

    public void MakeDecison()
    {
        float death = Random.value;
        float energy = Random.value;
        float power = Random.value;
        float rad = Random.value;
        float tension = Random.value;
        float time = Random.value;
        float hunger = Random.value;
        lastInputs = new float[] { death, energy, power, rad, tension, time, hunger, Random.value };
        float[] probs = brain.Predict(lastInputs);
        int action = WeightedRandom(probs);
        ExecuteIvent(action);
        timeSinceLastEvent = 0f;
    }

    private void ExecuteIvent(int action)
    {
        switch (action)
        {
            case 0: AdjustTension(-0.1f); TrainImmidiate(action, 0.1f); break;
            case 1: AdjustTension(0.1f); TrainImmidiate(action, 0.1f); break;
            case 2:
                if (Random.value < 0.5) ReportBattleOutcome(false, true);
                else
                {
                    if (Random.value < 0.5) ReportBattleOutcome(true, false);
                    else ReportBattleOutcome(false, false);
                }
                AdjustTension(0.4f);
                break;
            case 3:
                AdjustTension(-0.2f);
                ReportLootOutcome(true);
                break;
            case 4: TrainImmidiate(action, 0.05f); break;
        }
    }
    private AiEvent GetEvent(int action, float power)
    {
        var valid = allEvents.Where(e => e.category == action && power >= e.minPower && threatPoints >= e.threatCost).ToList();
        return valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : null;
    }

    private void TrainImmidiate(int action, float v)
    {
        float[] target = new float[OUTPUT_SIZE];
        for (int i = 0; i < OUTPUT_SIZE; i++)
        {
            target[i] = 0.1f;
        }
        target[action] = 0.6f;
        brain.Train(lastInputs, target, v);
    }

    private int WeightedRandom(float[] outp)
    {
        float total = 0;
        foreach (float prob in outp)
        {
            total += prob;
        }
        float randomPoint = Random.value * total;
        float cumulative = 0;
        for (int i = 0; i < outp.Length; i++)
        {
            cumulative += outp[i];
            if (randomPoint <= cumulative)
            {
                return i;
            }
        }
        return 0;
    }

    public void ReportBattleOutcome(bool playerWon, bool playerFled)
    {
        float[] idealOutput = new float[OUTPUT_SIZE];

        if (playerFled)
        {
            // Игрок сбежал -> Он не хочет/не может драться.
            // 1. Снижаем агрессивность
            currentAggression = Mathf.Clamp01(currentAggression - 0.15f);

            // 2. Учим ИИ: В этой ситуации (HP, Wealth и т.д.) БОЙ был плохим выбором.
            // Лучше бы дал Тишину (0) или Мелкую пакость (1).
            idealOutput[0] = 0.4f; // Chill
            idealOutput[1] = 0.4f; // Minor
            idealOutput[2] = 0.0f; // Battle (Снижаем вероятность до 0)
            idealOutput[3] = 0.8f;
            idealOutput[4] = 0.1f;

            Debug.Log("Игрок сбежал. Режиссер запомнил: меньше боев.");
            AdjustTension(0.2f);// Побег — это стресс[span_11](end_span)
        }
        else if (playerWon)
        {
            // Игрок победил -> Ему нравится/он может драться.
            // 1. Повышаем агрессивность
            currentAggression = Mathf.Clamp01(currentAggression + 0.1f);

            // 2. Учим ИИ: Бой был хорошим выбором!
            idealOutput[2] = 0.8f; // Battle (Поощряем)
            idealOutput[3] = 0.2f; // Loot (Награда)

            Debug.Log("Игрок победил. Режиссер запомнил: можно атаковать.");
            AdjustTension(-0.3f);// Победа расслабляет[span_12](end_span)
        }
        else // Player Died / Lost
        {
            // Если проиграл, но не сбежал -> было слишком сложно.
            // В следующий раз лучше дать помощь.
            idealOutput[3] = 0.9f; // Help/Loot
        }

        // ОБУЧАЕМ НЕЙРОСЕТЬ
        brain.Train(lastInputs, idealOutput, 0.2f); // Коэффициент обучения выше (0.2), так как это важный опыт
        //SaveBrain();
    }

    // Вызывай этот метод, когда игрок залутал локацию или проигнорировал её
    public void ReportLootOutcome(bool tookLoot)
    {
        float[] ideal = new float[OUTPUT_SIZE];
        for (int i = 0; i < OUTPUT_SIZE; i++)
        {
            ideal[i] = 0.5f;
        }
        if (tookLoot)
        {
            // Игроку нравятся находки
            ideal[3] = 0f; // Loot
            ideal[0] = 0.2f;
        }
        else
        {
            // Игрок проигнорировал -> может у него перегруз? Дадим тишину
            ideal[0] = 0.8f; // Chill
            ideal[3] = 0.1f; // Меньше лута
        }
        for (int i = 0; i < OUTPUT_SIZE; i++)
        {
            brain.Train(lastInputs, ideal, 0.1f);
        }
        //SaveBrain();
    }
    void InitializeTrainingData()
    {
        // 0:истощение, 1:бодрость, 2:Сила, 3:радиация,
        // 4:напряжение, 5:времяБезСобытий, 6:сытость, 7:агрессивностьИгрока

        trainingInputs = new float[][] {

            // игрок любит сражаться
            new float[] {0.1f, 0.1f, 1f, 0f, 0.1f, 1f, 0.1f, 1f},
            new float[] {0.3f, 0.6f, 0.7f, 0.2f, 0.2f, 1f, 0.4f, 1f},
            new float[] {0.5f, 0.1f, 1f, 0.1f, 0.4f, 0.9f, 0.5f, 0.7f},
            new float[] {0.1f, 0.8f, 1f, 0.3f, 0.1f, 0.5f, 0.1f, 0.5f},

            // игрок хочет помощи
            new float[] {0.9f, 0.9f, 0.8f, 0.4f, 0.8f, 0.1f, 0.4f, 0.2f},
            new float[] {0.8f, 0.5f, 0.7f, 0.5f, 0.3f, 0.2f, 0.7f, 0.4f},
            new float[] {0.5f, 0.1f, 0.4f, 0.2f, 0.1f, 0.7f, 0.6f, 0.3f},
            new float[] {0.3f, 0.8f, 0.2f, 0.3f, 0.5f, 0.5f, 0.9f, 0.1f},

            // игрок скучает
            new float[] {0.2f, 0.3f, 0.8f, 0.2f, 0.1f, 1f, 0.4f, 0.1f},
            new float[] {0.1f, 0.5f, 0.7f, 0.1f, 0.2f, 0.9f, 0.2f, 0.3f},
            new float[] {0.5f, 0.1f, 0.9f, 0.1f, 0.1f, 0.9f, 0.1f, 0.7f},
            new float[] {0.3f, 0.6f, 0.5f, 0f, 0.1f, 1f, 0.8f, 0.9f},

            new float[] {0.1f, 0.1f, 0.3f, 0.2f, 0.1f, 0.5f, 0.2f, 0.1f},
            new float[] {0.1f, 0.1f, 0.2f, 0.1f, 0.2f, 0.9f, 0.2f, 0.3f},
            new float[] {0.1f, 0.1f, 0.9f, 0.1f, 0.1f, 0.5f, 0.1f, 0.1f},
            new float[] {0.1f, 0.1f, 0.5f, 0f, 0.1f, 1f, 0.1f, 0.1f},
        };

        // 0:ничего, 1:пакость, 2:увечье, 3:помощь, 4:погода
        trainingOutputs = new float[][] {

            // игрок любит сражаться
            new float[] {0.1f, 0.1f, 0.8f, 0.1f, 0.1f},
            new float[] {0.3f, 0.6f, 0.6f, 0.3f, 0.1f},
            new float[] {0.3f, 0.7f, 0.5f, 0.4f, 0.2f},
            new float[] {0.3f, 0.6f, 0.4f, 0.2f, 0.2f},

            // игрок хочет помощи
            new float[] {0.2f, 0.2f, 0.2f, 0.9f, 0.1f},
            new float[] {0.1f, 0.3f, 0.2f, 0.6f, 0.2f},
            new float[] {0.3f, 0.3f, 0.2f, 0.3f, 0.1f},
            new float[] {0.3f, 0.2f, 0.2f, 0.4f, 0.2f},

            // игрок скучает
            new float[] {0.1f, 0.5f, 0.2f, 0.5f, 0.3f},
            new float[] {0.1f, 0.35f, 0.25f, 0.4f, 0.2f},
            new float[] {0.1f, 0.3f, 0.3f, 0.5f, 0.2f},
            new float[] {0.1f, 0.6f, 0.5f, 0.1f, 0.3f},

            new float[] {0.1f, 0.5f, 0.2f, 0.5f, 0.3f},
            new float[] {0.1f, 0.35f, 0.25f, 0.4f, 0.2f},
            new float[] {0.1f, 0.3f, 0.3f, 0.5f, 0.2f},
            new float[] {0.1f, 0.6f, 0.5f, 0.1f, 0.3f},
        };
    }

    void TrainOnInitialData(int epochs)
    {
        // Несколько эпох обучения
        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int i = 0; i < trainingInputs.Length; i++)
            {
                brain.Train(trainingInputs[i], trainingOutputs[i], 0.1f);
            }
        }

        Debug.Log("Начальное обучение завершено!");
    }

    void SpawnHelp()
    {

    }
    void ChangeWeather()
    {
    }
}
