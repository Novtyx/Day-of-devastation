using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowPlay : MonoBehaviour
{
    public List<GameObject> panels;

    private void Start()
    {
        if (PlayerPrefs.GetInt("howplay", 0) == 0) panels[0].SetActive(true);
    }

    public void Skip()
    {
        PlayerPrefs.SetInt("howplay", 1);
    }
}
