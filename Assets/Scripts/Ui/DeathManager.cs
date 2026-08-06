using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public void RemovCharacters()
    {
        PlayerPrefs.SetInt("Year", 1980);
        PlayerPrefs.SetInt("Month", 8);
        PlayerPrefs.SetInt("Day", 1);
        PlayerPrefs.SetInt("Hour", 7);
        PlayerPrefs.SetInt("Minute", 40);

        File.Delete(Path.Combine(Application.persistentDataPath, "inventory.json"));
        File.Delete(Path.Combine(Application.persistentDataPath, "diseas.json"));
        File.Delete(Path.Combine(Application.persistentDataPath, "builds.json"));

        PlayerPrefs.SetFloat("x", 1860f);
        PlayerPrefs.SetFloat("y", 3137f);

        PlayerPrefs.SetInt("death", 0);
        PlayerPrefs.SetInt("energy", 0);
        PlayerPrefs.SetInt("food", 0);
        PlayerPrefs.SetInt("water", 0);
        PlayerPrefs.SetFloat("radiation", 0);
        PlayerPrefs.SetInt("depature", 0);
        PlayerPrefs.SetInt("blood", 0);

        PlayerPrefs.SetInt("idCamp", 1);
        PlayerPrefs.SetInt("idHouse", 1);
        SceneManager.LoadScene("Menu");
    }
}
