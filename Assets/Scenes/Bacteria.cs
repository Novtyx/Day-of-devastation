using UnityEngine;

public class Bacteria : MonoBehaviour
{
    public bool isCarnivore; // True - хищник, False - травоядное
    public SimpleNeuralNetwork ai;

    [Header("Stats")]
    public float hp = 50f;
    public float maxHp = 100f;
    public float speed = 3f;
    public float hpLossRate = 5f; // Сколько HP теряет в секунду
    private int epoch;

    [Header("Tags")]
    public string foodTag;  // Для травоядных = "Grass", для хищников = "Herbivore"
    public string enemyTag; // Для травоядных = "Carnivore", для хищников = "" (никто)

    private float sensorRadius = 10f;
    private float MAxsensorRadius = 100f;
    private float NormalsensorRadius = 10f;

    private SpriteRenderer sr;
    private Color healthyColor;
    private Vector3 originalScale;

    void Start()
    {
        // Если сеть не передали по наследству, создаем случайную (нулевое поколение)
        // 7 входов, 10 скрытых, 2 выхода
        if (ai == null || ai.layers == null || ai.layers.Length == 0) ai = new SimpleNeuralNetwork(8, 10, 2);

        sr = GetComponent<SpriteRenderer>();
        healthyColor = sr.color;
        originalScale = Vector3.one;
    }

    void Update()
    {
        hp -= hpLossRate * Time.deltaTime;
        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Ищем ближайшие объекты
        Transform nearestFood = FindNearest(foodTag);
        Transform nearestEnemy = string.IsNullOrEmpty(enemyTag) ? null : FindNearest(enemyTag);

        // 2. Формируем входы для ИИ
        float[] inputs = new float[8];

        if (nearestFood != null)
        {
            Vector2 dir = (nearestFood.position - transform.position).normalized;
            inputs[0] = dir.x;
            inputs[1] = dir.y;
            inputs[2] = Vector2.Distance(transform.position, nearestFood.position) / sensorRadius;
        }
        else
        {
            inputs[0] = inputs[1] = inputs[2] = 0; // Не видим еды
        }

        if (nearestEnemy != null)
        {
            Vector2 dir = (nearestEnemy.position - transform.position).normalized;
            inputs[3] = dir.x;
            inputs[4] = dir.y;
            inputs[5] = Vector2.Distance(transform.position, nearestEnemy.position) / sensorRadius;
        }
        else
        {
            inputs[3] = inputs[4] = inputs[5] = 0f; // Не видим врагов
        }

        inputs[6] = hp / maxHp; // Чувство голода

        inputs[7] = Random.Range(-1f, 1f);

        // 3. Получаем решение от нейросети
        float[] outputs = ai.Predict(inputs);

        // Переводим выход сигмоиды (0..1) в нормальное направление (-1..1)
        Vector2 moveDir = new Vector2((outputs[0] * 2f) - 1f, (outputs[1] * 2f) - 1f).normalized;
        if (Mathf.Abs(transform.position.x) > EcosystemManager.instance.mapSize*2 || Mathf.Abs(transform.position.y) > EcosystemManager.instance.mapSize * 2) sensorRadius = MAxsensorRadius;
        else sensorRadius = NormalsensorRadius;
        // 4. Двигаемся
        transform.Translate(moveDir * speed * Time.deltaTime);
        

        // 5. Размножение
        if (hp >= maxHp) Reproduce();

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float hpPercent = hp / maxHp;

        // 1. Цвет: бактерия становится прозрачнее (бледнеет) при голоде
        sr.color = new Color(healthyColor.r, healthyColor.g, healthyColor.b, 0.2f + 0.8f * hpPercent);

        // 2. Размер: сжимается до 50% от оригинала, когда вот-вот умрет
        transform.localScale = originalScale * (0.5f + 0.5f * hpPercent);
    }
    // Обработка поедания через физику 2D (нужны коллайдеры с IsTrigger)
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(foodTag))
        {
            hp = Mathf.Min(hp + 30f, maxHp + 1f); // Восстанавливаем HP

            // Если мы съели траву - уничтожаем траву. 
            // Если хищник съел травоядное, травоядное умрет само через hp <= 0 или мы его убиваем:
            Destroy(col.gameObject);
        }
    }

    private void Reproduce()
    {
        hp -= 40f; // Роды отнимают энергию

        // Создаем копию себя
        GameObject childObj = Instantiate(gameObject, transform.position + (Vector3)Random.insideUnitCircle, Quaternion.identity);
        Bacteria child = childObj.GetComponent<Bacteria>();
        child.epoch = epoch + 1;
        childObj.name = child.epoch.ToString();

        // Передаем мутировавший мозг
        child.ai = Mutate(this.ai);
        child.hp = 40f; // Начальное HP ребенка
    }

    // Глубокое копирование и мутация весов из твоего класса
    private SimpleNeuralNetwork Mutate(SimpleNeuralNetwork parentNet)
    {
        string json = JsonUtility.ToJson(parentNet);
        SimpleNeuralNetwork childNet = JsonUtility.FromJson<SimpleNeuralNetwork>(json);

        // Мутируем 10% генов (весов)
        foreach (var layer in childNet.layers)
        {
            foreach (var neuron in layer.neurons)
            {
                for (int i = 0; i < neuron.weights.Length; i++)
                {
                    if (Random.value < 0.5f) neuron.weights[i] += Random.Range(-0.5f, 0.5f);
                }
                if (Random.value < 0.5f) neuron.bias += Random.Range(-0.5f, 0.5f);
            }
        }
        return childNet;
    }

    // Вспомогательный метод поиска (в реальном проекте лучше использовать OverlapCircle, но для старта пойдет это)
    private Transform FindNearest(string tag)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        Transform bestTarget = null;
        float closestDist = sensorRadius;

        foreach (GameObject t in targets)
        {
            if (t == this.gameObject) continue;
            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = t.transform;
            }
        }
        return bestTarget;
    }
}