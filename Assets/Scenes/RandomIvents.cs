using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ActionRecord
{
    public string actionId;
    public float timestamp;

    public ActionRecord(string id) { actionId = id; timestamp = Time.time; }
}
public class RandomIvents : MonoBehaviour
{
    public static RandomIvents instance;

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
    public float eventCheckInterval = 5f;
    public List<AiEvent> allEvents;
    public List<int> lastEvents = new List<int>();
    public List<ActionRecord> actions = new List<ActionRecord>();
    public float[] eventFatigue = new float[5] { 1f, 1f, 1f, 1f, 1f };
    public AnimationCurve tresholdCurve;
    public float levelDuration = 600f;
    public float timePassed = 0f;
    public float tensionAdaptationSpeed = 0.05f;

    void Start()
    {
        instance = this;
        // Инициализация с примерной архитектурой
        brain = new SimpleNeuralNetwork(INPUT_SIZE, 20, 16, OUTPUT_SIZE);

        InitializeTrainingData();
        TrainOnInitialData(5000);
    }

    // блок реального ии за время
    private void Update()
    {
        for (int i = 0; i < trainingInputs.Length; i++)
        {
             brain.Train(trainingInputs[i], trainingOutputs[i], 0.002f * Time.deltaTime);
        }
        
        float dt = Time.deltaTime;
        timePassed += dt;
        float normalizedTime = Mathf.Clamp01(timePassed / levelDuration);
        float targetTension = tresholdCurve.Evaluate(normalizedTime);
        currentTension = Mathf.MoveTowards(currentTension, targetTension, dt * tensionAdaptationSpeed);

        // 3. Восстанавливаем усталость событий
        for (int i = 0; i < 5; i++) eventFatigue[i] = Mathf.MoveTowards(eventFatigue[i], 1f, dt * 0.001f);

        timeSinceLastEvent += dt;
        threatPoints += dt;
        checkTimer += dt;
        if (checkTimer >= eventCheckInterval)
        {
            checkTimer = 0;
            if (ShouldAct())
            {
                MakeDecison();
            }
        }
    }

    public void MakeDecison()
    {
        float death = Mathf.Clamp01(Characters.instance.death / 100f);
        float energy = Mathf.Clamp01(Characters.instance.energy / 100f);
        float power = 10 / 100f;
        float rad = Mathf.Clamp01(Characters.instance.radiation / 100f);
        float tension = currentTension;
        float time = Mathf.Clamp01(timeSinceLastEvent / 600f);
        float hunger = Mathf.Clamp01(Characters.instance.food / 100f);
        lastInputs = new float[] { death, energy, power, rad, tension, time, hunger, currentAggression };
        inputs = lastInputs;
        float[] probs = brain.Predict(lastInputs);
        outputs = probs;
        int action = WeightedRandom(probs);
        AiEvent aiEvent = GetEvent(action, outputs[action]);
        if (aiEvent != null)
        {
            if (ValidateByHistory(aiEvent))
            {
                ExecuteIvent(aiEvent);
            }
        }
        timeSinceLastEvent = 0f;
    }

    private void ExecuteIvent(AiEvent aiEvent)
    {
        eventFatigue[aiEvent.category] *= 0.1f;
        lastEvents.Add(aiEvent.category);
        if (lastEvents.Count > 15) lastEvents.RemoveAt(0);

        switch (aiEvent.category)
        {
            case 0: aiEvent.Activate(); AdjustTension(-0.1f); TrainImmidiate(aiEvent.category, 0.1f); break;
            case 1: aiEvent.Activate(); AdjustTension(0.1f); TrainImmidiate(aiEvent.category, 0.1f); break;
            case 2:
                aiEvent.Activate();
                AdjustTension(0.4f);
                AttackManager.instance.StartAttack(Movement.instance.tiledata[1].enemys[0]);
                break;
            case 3:
                aiEvent.Activate();
                AdjustTension(-0.2f);
                EventManager.instance.StartEvent(Movement.instance.tiledata[1].events[0]);
                break;
            case 4: aiEvent.Activate(); TrainImmidiate(aiEvent.category, 0.05f); break;
        }
    }
    private AiEvent GetEvent(int action, float power)
    {
        var valid = allEvents.Where(e => e.category == action && power >= e.minPower && threatPoints >= e.threatCost).ToList();
        Debug.Log(valid.Count());
        Debug.Log(action);

        float totalWeight = 0;
        List<KeyValuePair<AiEvent, float>> weights = new List<KeyValuePair<AiEvent, float>>();

        foreach (var e in valid)
        {
            float w = e.CalculateWeight(actions);
            if (w > 0) weights.Add(new KeyValuePair<AiEvent, float>(e, w)); totalWeight += w;
        }
        if (totalWeight <= 0) return null;

        float rnd = Random.value * totalWeight;
        float cur = 0;
        foreach (var val in weights)
        {
            cur += val.Value;
            if (rnd <= cur) return val.Key;
        }

        return null;
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
        for (int i = 0; i < 5; i++) total += outp[i] * eventFatigue[i];
        float rnd = Random.value * total;
        float cur = 0;
        for (int i = 0; i < 5; i++)
        {
            cur += outp[i] * eventFatigue[i];
            if (rnd <= cur) return i;
        }
        return 0;
    }

    private bool ValidateByHistory(AiEvent card)
    {
        // Защита от спама: не более 2 одинаковых категорий подряд
        if (lastEvents.Count >= 2)
        {
            var lastTwo = lastEvents.Skip(lastEvents.Count - 2);
            if (lastTwo.All(h => h == card.category)) return false;
        }
        // Защита от перегрузки: если напряжение > 0.8, запрещаем вред
        if (currentTension > 0.8f && (card.category == 1 || card.category == 2)) return false;
        return true;
    }

    private void AdjustTension(float v)
    {
        currentTension = Mathf.Clamp01(currentTension + v);
    }

    private bool ShouldAct()
    {
        if (timeSinceLastEvent < 60f && currentTension < 0.8f) return false;
        float chance = Mathf.Clamp01((timeSinceLastEvent - 60f) / 300f);
        return Random.value < chance;
    }

    public void ReportBattleOutcome(bool playerWon, bool playerFled)
    {
        if (lastInputs == null) return;
        float[] idealOutput = brain.Predict(lastInputs);

        if (playerFled)
        {
            // Игрок сбежал -> Он не хочет/не может драться.
            // 1. Снижаем агрессивность
            currentAggression = Mathf.Clamp01(currentAggression - 0.15f);

            // 2. Учим ИИ: В этой ситуации (HP, Wealth и т.д.) БОЙ был плохим выбором.
            // Лучше бы дал Тишину (0) или Мелкую пакость (1).
            idealOutput[0] += 0.4f; // Chill
            idealOutput[1] += 0.1f; // Minor
            idealOutput[2] *= 0.4f; // Battle (Снижаем вероятность до 0)
            idealOutput[4] += 0.05f;

            Debug.Log("Игрок сбежал. Режиссер запомнил: меньше боев.");
            AdjustTension(0.3f);// Побег — это стресс[span_11](end_span)
        }
        else if (playerWon)
        {
            // Игрок победил -> Ему нравится/он может драться.
            // 1. Повышаем агрессивность
            currentAggression = Mathf.Clamp01(currentAggression + 0.1f);

            // 2. Учим ИИ: Бой был хорошим выбором!
            idealOutput[0] += 0.1f; // Battle (Поощряем)
            idealOutput[2] += 0.2f; // Battle (Поощряем)

            Debug.Log("Игрок победил. Режиссер запомнил: можно атаковать.");
            AdjustTension(-0.2f);// Победа расслабляет[span_12](end_span)
        }
        else // Player Died / Lost
        {
            // Если проиграл, но не сбежал -> было слишком сложно.
            // В следующий раз лучше дать помощь.
            idealOutput[0] += 0.5f; // Help/Loot
            idealOutput[3] += 0.2f; // Help/Loot
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

    public void AddActionHistory(string action)
    {
        actions.Add(new ActionRecord(action));

        if (actions.Count > 20) actions.RemoveAt(0);
    }
}
