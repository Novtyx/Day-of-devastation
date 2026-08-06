using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class AttackManager : MonoBehaviour
{
    public static AttackManager instance;
    [SerializeField] Transform itemsParent;
    [SerializeField] AttackSlot[] itemSlots;

    public SimpleNeuralNetwork network;

    public GameObject itemMenu;
    public Image icon;
    public Text itemName;
    public Text itemDescription;
    public Button useButton;
    public Button closeButton;


    private AttackSlot currentItemSlot;
    public List<WeaponItem> items;                // Список оружия
    public List<int> count;                // Список предметов

    public GameObject AttackPanel;
    public Image enemyIcon;

    [Header("AttackSettings")]
    int _power; // Сила оружия
    int enemyCount, enemyPower;

    [Header("Texts")]
    [SerializeField] TMP_Text powerText;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text enemyPowerText;
    [SerializeField] TMP_Text protectionText;
    [SerializeField] TMP_Text bodyCoveringText;
    [SerializeField] TMP_Text winChanceText;
    [SerializeField] TMP_Text EnemyNameText;
    public Enemy enemy;

    [Header("Win/Lose Panels")]
    [SerializeField] private GameObject StatePanel;
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Diseases_text;
    [SerializeField] private TMP_Text Items_text;
    public float[] floats;
    public float[] outp;

    private float[][] trainingInputs;
    private float[][] trainingOutputs;
    [SerializeField] float forgetSpeed = 0.02f;
    [SerializeField] float baseIntuition = 0.3f;

    public void predict()
    {
        outp = network.Predict(floats);
    }

    void InitializeTrainingData()
    {
        // Примеры "хороших" решений
        // Формат: [здоровье, голод, в болоте, есть оружие...] -> [событие1, событие2...]

        trainingInputs = new float[][] {
            new float[] {0.8f, 0.2f, 0f}, // Лиса при большей силе
            new float[] {0.2f, 0.8f, 0f}, // Лиса при меньшей силе
            new float[] {0.9f, 0.2f, 1f}, // Волк +
            new float[] {0.3f, 0.8f, 1f}, // Волк -
            new float[] {0.9f, 0.1f, 2f}, // Медведь +
            new float[] {0.1f, 0.8f, 2f}, // Медведь -
            new float[] {0.9f, 0.1f, 3f}, 
            new float[] {0.1f, 0.8f, 3f},
        };

        trainingOutputs = new float[][] {
            new float[] {0.6f}, // Лиса +
            new float[] {0.5f}, // Лиса -
            new float[] {0.5f}, // Волк +
            new float[] {0.4f}, // ВОлк -
            new float[] {0.3f}, // Мдведь +
            new float[] {0.1f}, // Медведь -
            new float[] {1.0f},  
            new float[] {1.0f},  
        };
    }

    void TrainOnInitialData(int epoches)
    {
        // Несколько эпох обучения
        for (int epoch = 0; epoch < epoches; epoch++)
        {
            for (int i = 0; i < trainingInputs.Length; i++)
            {
                network.Train(trainingInputs[i], trainingOutputs[i], 0.1f);
            }
        }

        Debug.Log("Начальное обучение завершено!");
    }

    private void Start()
    {
        instance = this;
        network = new SimpleNeuralNetwork(3, 16, 12, 5, 1);
        if (itemsParent != null)
        {
            itemSlots = itemsParent.GetComponentsInChildren<AttackSlot>();
        }
        RefreshUI();
        // Создаем обучающие данные
        InitializeTrainingData();

        // Обучаем перед использованием
        TrainOnInitialData(10000);
    }

    private void Update()
    {
        for (int i = 0; i < trainingInputs.Length; i++)
        {
            network.Train(trainingInputs[i], trainingOutputs[i], 0.002f * Time.deltaTime);
        }
    }
    public void StartAttack(Enemy enemy)
    {
        if (AttackPanel.activeInHierarchy) return;
        Movement.instance.lastPosition = Movement.instance.gameObject.transform.position;
        NoRefresh();
        items.Clear();
        this.enemy = enemy;
        this.enemyCount = enemy.Count;
        this.enemyPower = enemy.Power;
        AttackPanel.SetActive(true);
        for (int i = 0; i < Inventory.instance.items.Count; i++)
        {
            if (Inventory.instance.items[i].Item is WeaponItem weapon1) items.Add(weapon1);
        }
        RefreshUI();
    }
    public void Secrecy()
    {
        AttackPanel.SetActive(false);
        RandomIvents.instance.ReportBattleOutcome(false, true);
        enemy = null;
    }
    public void Attack()
    {
        Diseases_text.text = "";
        Items_text.text = "";
        float result = Result();
        int result1 = Mathf.RoundToInt((float)result * 100);
        result1 = Mathf.Clamp(result1, 5, 95);
        if (Random.Range(0f, 100f) < result1)
        {
            float enemys = Mathf.Clamp(enemyCount * enemyPower / 100f, 0f, 1f);
            float player = Mathf.Clamp((1 + _power + Characters.instance.protection + Characters.instance.body_covering) / 100f, 0f, 1f);
            Debug.Log(enemy.index);
            for (int i = 0; i < 5; i++) network.Train(new float[3] { player, enemys, enemy.index}, new float[1] { 0.8f }, 0.1f);
            if (enemy.diseaseOnWin != null)
            {
                HealthManager.instance.AddDisease(enemy.diseaseOnWin);
                Diseases_text.text = "Получена болезнь: " + enemy.diseaseOnWin.Name;
            }
            List<string> _items = new List<string>();
            if (enemy.itemsOnWin.Count > 0) _items.Add("получены предметы:");
            for (int i = 0; i < enemy.itemsOnWin.Count; i++)
            {
                Inventory.instance.AddItem(enemy.itemsOnWin[i], enemy.itemsOnWinCount[i], null);
                _items.Add($"{enemy.itemsOnWin[i].Name} x{enemy.itemsOnWinCount[i]}");
            }
            string res = string.Join("\n", _items);
            Items_text.text = res;
            Title.text = "победа";
            RandomIvents.instance.ReportBattleOutcome(true, false);
        }
        else
        {
            float enemys = Mathf.Clamp(enemyCount * enemyPower / 100f, 0f, 1f);
            float player = Mathf.Clamp((1 + _power + Characters.instance.protection + Characters.instance.body_covering) / 100f, 0f, 1f);
            network.Train(new float[3] { player, enemys, enemy.index }, new float[1] { 0.05f }, 0.1f);
            if (enemy.diseaseOnLose != null)
            {
                HealthManager.instance.AddDisease(enemy.diseaseOnLose);
                Diseases_text.text = "Получена болезнь: " + enemy.diseaseOnLose.Name;
            }
            Title.text = "поражение";
            Characters.instance.death += 70;
            RandomIvents.instance.ReportBattleOutcome(false, false);
        }
        StatePanel.SetActive(true);
        AttackPanel.SetActive(false);
    }
    void NoRefresh()
    {
        _power = 0;
        enemyCount = 0;
        enemyPower = 0;
    }
    float Result()
    {
        float enemy = Mathf.Clamp(enemyCount * enemyPower / 100f, 0f, 1f);
        float player = Mathf.Clamp((1 + _power + Characters.instance.protection + Characters.instance.body_covering) / 100f, 0f, 1f);
        int res = Mathf.RoundToInt((float)player / enemy * 100);
        float[] newres = network.Predict(new float[3] { player, enemy, this.enemy.index });
        outp = newres;
        Debug.Log($"{player} / {enemy}");
        return newres[0];
    }
    public void RefreshUI()
    {
        int i = 0;
        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = items[i];
        }
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
        }
        powerText.text = "Сила оружия: " + _power.ToString();
        enemyCountText.text = "Количество врагов: " + enemyCount.ToString();
        enemyPowerText.text = "Сила противников: " + enemyPower.ToString();
        protectionText.text = "Ваша защита: " + Characters.instance.protection.ToString();
        bodyCoveringText.text = "Покрытие тела: " + Characters.instance.body_covering.ToString();
        float result = Result();
        int result1 = Mathf.RoundToInt((float)result * 100);
        result1 = Mathf.Clamp(result1, 5, 95);
        winChanceText.text = "Шанс выиграть: " + result1.ToString();
        EnemyNameText.text = "Враг: " + enemy.EnemyName;
        enemyIcon.sprite = enemy.sprite;
    }



    public void ShowItemMenu(AttackSlot campSlot)
    {
        currentItemSlot = null;
        currentItemSlot = campSlot;
        icon.sprite = campSlot.Item.icon;
        itemName.text = campSlot.Item.Name;
        itemDescription.text = campSlot.Item.Description;

        itemMenu.SetActive(true);
        // Очищаем предыдущие слушатели событий
        useButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // Добавляем новые слушатели событий
        useButton.onClick.AddListener(OnUseButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    void OnUseButtonClicked()
    {
        _power = currentItemSlot.Item.power;
        RefreshUI();
    }
    public GameObject sliderpanel;
    public Slider slider;
    public bool iscreating;
    public int craftingTime;


    public void OnCloseButtonClicked()
    {
        itemMenu.SetActive(false);
    }

    private void OnDisable()
    {
        useButton.onClick.RemoveListener(OnUseButtonClicked);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        currentItemSlot = null;
    }
}
