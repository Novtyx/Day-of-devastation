using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using XLua;

[LuaCallCSharp]
public class Movement : MonoBehaviour
{
    public static Movement instance;
    public bool go_not = false; // Флаг движения к курсору
    public bool move = true; // Общая возможность двигаться
    public GameObject target;
    public GameObject targetUi;
    public static float speed = 3;
    public static float globalSpeed = 3;
    private float speedCoefficient = 0.95f;
    private Rigidbody2D pla;

    [Space]

    public string NameBuild;
    public string DescriptionBuild;
    public Sprite imageBuild;

    public Image icon;
    [Space]

    private LootSlot currentItemSlot;

    [Header("UI панель")]
    public Image panel_material;
    public Image panel_fon;
    [Space]
    public Camera maincamera;
    public TMP_Text tileNameText;

    public string tile;
    private Biome currentBiome;

    public List<Biome> tiledata = new List<Biome>();

    public float Radius = 0.5f;
    public LayerMask Layer;

    [Header("Произвольное движение")]
    public Vector3 lastPosition;
    private float lastTime;
    public float interval = 10f;
    public bool CanMovie = false;
    public Vector3 TargetPosition;

    public Tilemap tilemap;

    // --- НОВАЯ СИСТЕМА ЗАДАЧ ---
    public Coroutine currentTask;

    public void StartTask(IEnumerator task)
    {
        // Если игрок уже что-то делает, отменяем старую задачу
        if (currentTask != null) StopCoroutine(currentTask);
        currentTask = StartCoroutine(task);
    }
    public void StopTask()
    {
        StopCoroutine(currentTask);
        currentTask = null;
    }

    // Универсальный метод для подхода к объекту
    public IEnumerator MoveToRoutine(Vector3 destination)
    {
        target.transform.position = destination;
        Movement_start();

        // Ждем, пока персонаж двигается
        while (go_not)
        {
            yield return null; // Ждем следующий кадр
        }

        // Если цикл прервался, проверяем, дошли ли мы (вдруг остановил враг)
        if (Vector2.Distance(transform.position, destination) > 0.5f)
        {
            // Мы не дошли, прерываем выполнение задачи
            yield break;
        }
    }
    // ----------------------------

    private void Start()
    {
        instance = this;
        pla = GetComponent<Rigidbody2D>();
        float x = PlayerPrefs.GetFloat("x", 2264f);
        float y = PlayerPrefs.GetFloat("y", 3296f);
        pla.position = new Vector2(x, y);
        target.transform.position = new Vector2(x + 10f, y);
        maincamera.transform.position = new Vector3(x, y, -10f);
        lastPosition = new Vector2(x, y);
    }

    public void Setitems(Biome tile)
    {
        if (Inventory.isLoot == false)
        {
            LootBuildManager.instance.SetLocationPanel(NameBuild.ToString(), DescriptionBuild, imageBuild);
            Material material = panel_material.material;
            panel_material.material = material;
            material.SetColor("_first", tile.FirstColor);
            material.SetColor("_second", tile.SecondColor);
            material.SetTexture("_Texture2D", tile.Sprite.texture);

            LootBuildManager.instance.SetButtonListener(OnUseButtonClicked);
        }
    }

    private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(10);
    public Sprite GetSprite(string path)
    {
        if (!_spriteCache.TryGetValue(path, out var sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            _spriteCache[path] = sprite;
        }
        return sprite;
    }

    [SerializeField] private Transform ActionsPanel;
    [SerializeField] private Button ActionButton;

    public void InitializeCustomIvents(List<CustomAction> actions)
    {
        Button[] tradingObjects = ActionsPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < tradingObjects.Length; i++)
        {
            Destroy(tradingObjects[i].gameObject);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            Button obj = Instantiate(ActionButton, ActionsPanel);
            obj.gameObject.GetComponentInChildren<TMP_Text>().text = actions[i].ActionName;
            obj.onClick.RemoveAllListeners();
            CustomAction action = actions[i];
            obj.onClick.AddListener(() => action.UseByCamp(null));
        }
    }

    private void Update()
    {
        bool isGroud = false;
        if (target.transform.position.x > 0 && target.transform.position.x < 8192 &&
            target.transform.position.y > 0 && target.transform.position.y < 4096) isGroud = true;

        Vector3 worldPosition = transform.position;

        if (tilemap == null) return;
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        TileBase tile = TilemapGenerator.instance.GetTileAtPosition(worldPosition);

        if (tile != null && !Inventory.isLoot && !Inventory.isTown)
        {
            string tileName = tile.name;
            if (tile.name != this.tile)
            {
                for (int i = 0; i < tiledata.Count; i++)
                {
                    if (tileName == tiledata[i].name)
                    {
                        NameBuild = tiledata[i].NameTile;
                        DescriptionBuild = tiledata[i].DescriptionTile;
                        tileNameText.text = tiledata[i].NameTile.ToString();
                        LootBuildManager.instance.RefreshUI(tiledata[i].items, tiledata[i].itemCounts);
                        LootBuildManager.instance.RefreshEnemys(tiledata[i].enemys);
                        speedCoefficient = tiledata[i].SpeedCoefficient;
                        InitializeCustomIvents(tiledata[i].CustomActions);
                        MusicManager.instance.ChangeTrack(tiledata[i].MusicIndex, 10);
                        Characters.instance.radiation_strength = tiledata[i].radiation;
                        Setitems(tiledata[i]);
                        this.tile = tiledata[i].Tile.name;
                        currentBiome = tiledata[i];
                        break;
                    }
                }
            }
        }

        if (go_not == true && move == true && isGroud)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * speedCoefficient * Time.deltaTime);
            if (transform.position == target.transform.position)
            {
                go_not = false;
                lastPosition = transform.position;
                Characters.instance.StopTime();
                SetNewTargetPosition(); // Сброс таргета произвольного движения
            }
        }
        else if (move == true)
        {
            if (CanMovie) Move();
            else StanOnPlace();
        }
    }

    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, TargetPosition, speed * speedCoefficient * Time.deltaTime);
        if (transform.position == TargetPosition)
        {
            CanMovie = false;
        }
    }

    void StanOnPlace()
    {
        lastTime += Time.deltaTime;
        if (lastTime > interval)
        {
            SetNewTargetPosition();
            CanMovie = true;
            lastTime = 0;
        }
    }

    void SetNewTargetPosition()
    {
        float x = lastPosition.x + Random.Range(-0.1f, 0.1f);
        float y = lastPosition.y + Random.Range(-0.1f, 0.1f);
        TargetPosition = new Vector3(x, y, 0);
        interval = 10f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.transform.position, Radius);
    }

    private void FixedUpdate()
    {
        PlayerPrefs.SetFloat("x", pla.position.x);
        PlayerPrefs.SetFloat("y", pla.position.y);
    }
    [LuaCallCSharp]
    public void Movement_start()
    {
        if (Characters.instance.currentheight > Characters.instance.maxheight)
        {
            NotifyManager.instance.SetMinNotify("Слишком тяжело");
            return;
        }
        if (go_not == false && Characters.instance.changedata == false)
        {
            Characters.instance.StartTime();
        }
        go_not = true;
    }

    public void test()
    {
        Animations.instance.CursorHide(target, targetUi, 1, 0);
        Vector3 pos = Input.mousePosition;
        Vector3 playpos = Camera.main.ScreenToWorldPoint(pos);
        Animations.instance.SetPosition(target.transform, new Vector3(playpos.x, playpos.y, 0), 0.05f);
        Animations.instance.CursorHide(target, targetUi, 0.1f, 5f);
    }
    public void CursorClick()
    {
        if (Time.time - lastClickTime < doubleClickTime)
        {
            Movement_start();
        }
        else
        {
            test();
            lastClickTime = Time.time;
        }
    }

    public float doubleClickTime = 0.3f;
    private float lastClickTime = 0f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            go_not = false;
            speed = 1;
            Characters.instance.StopTime();
        }
        if (collision.gameObject.CompareTag("Radiation"))
        {
            Characters.instance.aec_radiation_strength += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            speed = globalSpeed;
        }
        if (collision.gameObject.CompareTag("Radiation"))
        {
            Characters.instance.aec_radiation_strength -= 1;
        }
    }

    void OnUseButtonClicked()
    {
        if (Inventory.isLoot != true)
        {
            Inventory.instance.LoadCamp();
            //craftingTime = 1;
            ChangeTime(craftingTime);
        }
    }
    public int craftingTime;

    void ChangeTime(int time)
    {
        TimeLineManager.instance.StartTime(time, () =>
        {
            AllItems();
        });
    }

    public void AllItems()
    {
        SpawnItemInCamp();
        if (Random.Range(0, 100) < 2 && currentBiome.enemys.Count > 0)
        {
            Enemy enemy = currentBiome.enemys[Random.Range(0, currentBiome.enemys.Count)];
            AttackManager.instance.StartAttack(enemy);
        }
        else if (Random.Range(0, 100) < 2 && currentBiome.events.Count > 0)
        {
            Event event1 = currentBiome.events[Random.Range(0, currentBiome.events.Count)];
            EventManager.instance.StartEvent(event1);
        }
    }

    public void SpawnItemInCamp()
    {
        Debug.Log("TTT");
        Inventory.instance.LoadCamp();
        for (int i = 0; i < currentBiome.items.Count; i++)
        {
            int counti = Random.Range(0, currentBiome.itemCounts[i] + 1);
            if (counti < 1) continue;
            if (Inventory.isCamp == true)
            {
                Inventory.instance.Looting(currentBiome.items[i], counti);
            }
        }
        if (Inventory.isCamp == true)
        {
            Inventory.instance.Raise();
        }
    }

    public void ShowItemMenu(LootSlot campSlot)
    {
        currentItemSlot = null;
        currentItemSlot = campSlot;
        icon.sprite = campSlot.Item.icon;
        LootBuildManager.instance.ShowItemInfoPanel(campSlot.Item.Name, campSlot.Item.Description, campSlot.Item.icon);
    }

    public Button button;
    public void StopButton() => button.enabled = false;
    public void StartButton() => button.enabled = true;
}
