using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using System;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    private string filePath;

    [SerializeField] private List<Build> builds = new();
    [SerializeField] private List<GameObject> buildsObjects = new();
    [SerializeField] private List<GameObject> MaybeBuilds;
    public struct Build
    {
        public string Name;
        public float x;
        public float y;
        public int index;
    }

    public class BuildData
    {
        public List<Build> builds = new();
    }
    private void Start()
    {
        instance = this;
        filePath = Path.Combine(Application.persistentDataPath, "builds.json");
        EnsureFileExists();
        LoadData();
    }
    public void AddBuildInPlayer(GameObject gameObject)
    {
        AddBuild(gameObject, Movement.instance.gameObject.transform.position);
    }
    public void RemoveBuild(GameObject _build)
    {
        Debug.Log(_build.name);
        for (int i = 0; i < builds.Count; i++)
        {
            if (_build.GetComponent<BuildObject>().index == builds[i].index)
            {
                builds.Remove(builds[i]);
                buildsObjects.Remove(_build);
                Destroy(_build);
                SaveData();
                break;
            }
        }
    }
    public void AddBuild(GameObject _build, Vector2 _pos)
    {
        GameObject buildObj = Instantiate(_build, _pos, Quaternion.identity);
        buildObj.GetComponent<BuildObject>().index = PlayerPrefs.GetInt("BuildIndex", 0) + 1;

        Build build = new Build();
        build.Name = buildObj.GetComponent<BuildObject>().Id;
        build.x = buildObj.transform.position.x;
        build.y = buildObj.transform.position.y;
        build.index = PlayerPrefs.GetInt("BuildIndex", 0) + 1;
        PlayerPrefs.SetInt("BuildIndex", PlayerPrefs.GetInt("BuildIndex", 0) + 1);
        builds.Add(build);
        buildsObjects.Add(buildObj);
        SaveData();
    }
    public void ResetBuild(GameObject _build, Build build1)
    {
        GameObject buildObj = Instantiate(_build, new Vector2(build1.x, build1.y), Quaternion.identity);
        buildObj.GetComponent<BuildObject>().index = build1.index;
        Build build = new Build();
        build.Name = buildObj.GetComponent<BuildObject>().Id;
        build.x = build1.x;
        build.y = build1.y;
        build.index = build1.index;
        builds.Add(build);
        buildsObjects.Add(buildObj);
        SaveData();
    }
    public void SaveData()
    {
        BuildData data = new BuildData();
        data.builds = this.builds;
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(filePath, json);
    }
    private void LoadData()
    {
        if (File.Exists(filePath))
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<BuildData>(json, settings);
            for (int i = 0; i < data.builds.Count; i++)
            {
                foreach (GameObject it in MaybeBuilds)
                {
                    if (data.builds[i].Name == it.GetComponent<BuildObject>().Id)
                    {
                        ResetBuild(it, data.builds[i]);
                    }
                }
            }
        }
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(filePath))
        {
            BuildData data = new BuildData();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(filePath, json);
        }
    }
}
