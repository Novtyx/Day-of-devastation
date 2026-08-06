using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private AudioSource audioSource;
    [SerializeField] AudioClip[] audioClips;
    private bool isFade = false;
    private Coroutine coroutine;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }
    public void ChangeTrack(int index, float fadeDuration = 2f)
    {
        int i = PlayerPrefs.GetInt("embient", 1);
        if (index < 0 || index >= audioClips.Length || i == 0) return;
        if (isFade)
        {
            StopCoroutine(coroutine);
            isFade = false;
        }
        coroutine = StartCoroutine(FadeToTrack(index, fadeDuration));
    }
    public void ChangeTrack(int index)
    {
        int i = PlayerPrefs.GetInt("embient", 1);
        if (index < 0 || index >= audioClips.Length || i == 0) return;
        StopCoroutine(FadeToTrack(index, 2));
        StartCoroutine(FadeToTrack(index, 2));
    }
    private IEnumerator FadeToTrack(int index, float fadeduration)
    {
        isFade = true;
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeduration/2; t += Time.deltaTime)
        {
            if (!isFade) yield break;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / (fadeduration / 2));
            yield return null;
        }

        if (!isFade) yield break;

        audioSource.Stop();
        audioSource.clip = audioClips[index];
        audioSource.Play();
        for (float t = 0; t < fadeduration / 2; t += Time.deltaTime)
        {
            if (!isFade) yield break;
            audioSource.volume = Mathf.Lerp(0f, 0.4f, t / (fadeduration / 2));
            yield return null;
        }
        if (!isFade) yield break;
        audioSource.volume = 0.4f;
        isFade = false;
    }
}
