using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AsyncScene : MonoBehaviour
{
    public GameObject Slider_object;
    public Slider SliderSlider;
    public List<string> sovets;
    public TMP_Text sovet;
    public void StartGame(int SceneIndex)
    {
        StartCoroutine(LoadAsync(SceneIndex));
    }
    IEnumerator LoadAsync(int SceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneIndex);
        Slider_object.SetActive(true);
        int i = Random.RandomRange(0, sovets.Count);
        sovet.text = sovets[i];
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            SliderSlider.value = progress;
            yield return null;
        }
    }
}
