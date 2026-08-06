using UnityEngine;

public class EcosystemManager : MonoBehaviour
{
    public static EcosystemManager instance;
    public GameObject grassPrefab;
    public GameObject Herbivore;
    public GameObject Carnivore;
    public int maxGrass = 50;
    public float mapSize = 15f;

    [Header("Simulation Speed")]
    [Range(1f, 100f)] // Ползунок в инспекторе от 1x до 100x
    public float timeScale = 1f;

    void Update()
    {
        instance = this;
        // Применяем ускорение времени к игре
        Time.timeScale = timeScale;

        // Поддерживаем популяцию травы
        if (GameObject.FindGameObjectsWithTag("Grass").Length < maxGrass)
        {
            Vector2 randomPos = new Vector2(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
            Instantiate(grassPrefab, randomPos, Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            for (int i = 0; i < 30; i++)
            {
                if (Random.value < 0.7f)
                {
                    Vector2 randomPos = new Vector2(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
                    Instantiate(Herbivore, randomPos, Quaternion.identity);
                }
                else
                {
                    Vector2 randomPos = new Vector2(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
                    Instantiate(Carnivore, randomPos, Quaternion.identity);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
                Instantiate(Carnivore, randomPos, Quaternion.identity);
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            for (int i = 0; i < 100; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
                Instantiate(Herbivore, randomPos, Quaternion.identity);
            }
        }
    }

    // ВАЖНО: Если остановишь игру, время должно вернуться в норму, 
    // иначе редактор Unity может начать вести себя странно.
    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}