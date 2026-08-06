using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class VfxManager : MonoBehaviour
{
    public Transform parent;

    public GameObject[] lights;
    public GameObject tilemapPrefab;
    public GameObject treePrefab;
    public GameObject itemPrefab;
    public GameObject firePrefab;
    public Material NotLightPrefab;
    public Material LightPrefab;
    private void Start()
    {
        int x = PlayerPrefs.GetInt("generateparticles", 0);
        if (x == 1)
        {
            GameObject prefab = Resources.Load<GameObject>("Vfx/Particles");
            GameObject panel = Instantiate(prefab);
            panel.transform.SetParent(parent, false);
        }
        int y = PlayerPrefs.GetInt("zerno", 0);
        if (y == 1)
        {
            GameObject prefab = Resources.Load<GameObject>("Vfx/Zerno");
            GameObject panel = Instantiate(prefab);
            panel.transform.SetParent(parent, false);
        }
        int i = PlayerPrefs.GetInt("generateclouds", 0);
        if (i == 1)
        {
            GameObject prefab = Resources.Load<GameObject>("Vfx/Clouds");
            GameObject panel = Instantiate(prefab);
            panel.transform.SetParent(parent, false);
        }
        int p = PlayerPrefs.GetInt("postprocessing", 0);
        if (p == 1)
        {
            GameObject prefab = Resources.Load<GameObject>("Vfx/Post");
            GameObject panel = Instantiate(prefab);
            panel.transform.SetParent(parent, false);
        }
        int light = PlayerPrefs.GetInt("light", 0);
        if (light == 0)
        {
            tilemapPrefab.GetComponentInChildren<TilemapRenderer>().material = NotLightPrefab;
            itemPrefab.GetComponentInChildren<SpriteRenderer>().material = NotLightPrefab;
            treePrefab.GetComponentInChildren<SpriteRenderer>().material = NotLightPrefab;
            Destroy(Movement.instance.gameObject.GetComponent<Light2D>());
            Destroy(firePrefab.GetComponent<Light2D>());
            Destroy(lights[0]);
            Destroy(lights[1]);
        }
        else
        {
            tilemapPrefab.GetComponentInChildren<TilemapRenderer>().material = LightPrefab;
            itemPrefab.GetComponentInChildren<SpriteRenderer>().material = LightPrefab;
            treePrefab.GetComponentInChildren<SpriteRenderer>().material = LightPrefab;
        }
    }
}
