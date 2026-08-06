using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    public GameObject roadPrefab;
    public GameObject[] otherCity;
    public GameObject spawnedroad;
    public int group;

    void Start()
    {

    }
    public void delete()
    {
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Road"))
        {
            Destroy(i);
        }
    }
    public void Generateroads()
    {
        for (int i = 0; i < otherCity.Length; i++)
        {
            if (otherCity[i].GetComponent<RoadGenerator>().group != group && group != 0 && Random.Range(0, 100) < 25)
            {
                Vector3 thisPosition = transform.position;
                Vector3 otherPosition = otherCity[i].gameObject.transform.position;

                // Создание вертикальной дороги
                Vector3 verticalPosition = new Vector3(thisPosition.x, (otherPosition.y + thisPosition.y) / 2, 1f);
                spawnedroad = Instantiate(roadPrefab, verticalPosition, transform.rotation, transform.parent);
                float verticalLength = Mathf.Abs(otherPosition.y - thisPosition.y);
                Vector3 verticalScale = new Vector3(0.2f, verticalLength / roadPrefab.GetComponent<SpriteRenderer>().bounds.size.y / 2.83f, 1);
                spawnedroad.transform.localScale = verticalScale;

                // Создание горизонтальной дороги
                Vector3 horizontalPosition = new Vector3((otherPosition.x + thisPosition.x) / 2, otherPosition.y, 1f);
                spawnedroad = Instantiate(roadPrefab, horizontalPosition, transform.rotation, transform.parent);
                float horizontalLength = Mathf.Abs(otherPosition.x - thisPosition.x);
                Vector3 horizontalScale = new Vector3(horizontalLength / roadPrefab.GetComponent<SpriteRenderer>().bounds.size.x / 2.23f, 0.2f, 1);
                spawnedroad.transform.localScale = horizontalScale;
            }
        }
    }
}

