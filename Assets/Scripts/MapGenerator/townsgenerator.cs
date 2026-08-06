using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class townsgenerator : MonoBehaviour
{
    public static townsgenerator instance;
    public List<GameObject> points;
    public List<GameObject> Typetowns;
    public List<LootBuild> buildings;
    public List<string> names;
    public List<List<ItemInstance>> items = new List<List<ItemInstance>>();
    public List<List<int>> itemchances = new List<List<int>>();            // Список для хранения количества предметов
    public List<int> townids;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        StartCoroutine(LoadTowns());
    }
    IEnumerator LoadTowns()
    {
        yield return new WaitUntil(() => Inventory.instance.LuaModsManager.isLoaded);

        for (int i = 0; i < points.Count; i++)
        {
            GameObject house = Instantiate(Typetowns[i], points[i].transform.position, Quaternion.identity);
            house.GetComponent<visiblechild>().child.GetComponent<TopDownGenerator>().nameoftown.text = points[i].name;
            house.SetActive(true);
        }
    }
}
