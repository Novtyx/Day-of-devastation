using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLineManager : MonoBehaviour
{
    public static TimeLineManager instance;
    [SerializeField] GameObject sliderpanel;
    public Slider slider;
    public Slider minislider;

    // Флаги для отслеживания запущенных корутин
    private bool isTimeCoroutineRunning = false;
    private bool isMiniTimeCoroutineRunning = false;
    private bool isPlaceboTimeCoroutineRunning = false;

    // Ссылки на корутины
    private Coroutine timeCoroutine;
    private Coroutine miniTimeCoroutine;
    private Coroutine placeboTimeCoroutine;

    private void Start()
    {
        instance = this;
    }
    public void StartTime(float craftingTime, Action onComplete)
    {
        // Запускаем корутины только если они не запущены
        if (!isTimeCoroutineRunning)
        {
            timeCoroutine = StartCoroutine(ChangeTime(craftingTime, onComplete)); ;
        }
    }

    IEnumerator ChangeTime(float craftingTime, Action onComplete)
    {
        isTimeCoroutineRunning = true;

        Movement.instance.move = false;

        sliderpanel.SetActive(true);
        Characters.instance.StartTime();
        float elapsedTime = 0f;
        while (elapsedTime < craftingTime)
        {
            elapsedTime += Time.deltaTime;
            slider.value = elapsedTime / craftingTime;
            yield return null; // Wait for the next frame
        }
        sliderpanel.SetActive(false); // Hide the slider
        Characters.instance.StopTime();

        Movement.instance.move = true;

        onComplete?.Invoke();

        isTimeCoroutineRunning = false;
    }

    public void StartMiniTime(int craftingTime, Action onComplete)
    {
        // Запускаем корутины только если они не запущены
        if (!isMiniTimeCoroutineRunning)
        {
            miniTimeCoroutine = StartCoroutine(ChangeMiniTime(craftingTime, onComplete)); ;
        }
    }

    IEnumerator ChangeMiniTime(int craftingTime, Action onComplete)
    {
        isMiniTimeCoroutineRunning = true;

        Movement.instance.move = false;

        minislider.gameObject.SetActive(true);
        Characters.instance.StartTime();
        float elapsedTime = 0f;
        while (elapsedTime < craftingTime)
        {
            elapsedTime += Time.deltaTime;
            minislider.value = elapsedTime / craftingTime;
            minislider.transform.position = new Vector3(Movement.instance.gameObject.transform.position.x, Movement.instance.gameObject.transform.position.y + 1f, 0);
            yield return null; // Wait for the next frame
        }
        minislider.gameObject.SetActive(false); // Hide the slider
        Characters.instance.StopTime();

        Movement.instance.move = true;

        onComplete?.Invoke();

        isMiniTimeCoroutineRunning = false;
    }

    // Только визуальное представление
    public void StartPlacebo(float craftingTime, int count)
    {
        // Запускаем корутины только если они не запущены
        if (!isPlaceboTimeCoroutineRunning)
        {
            placeboTimeCoroutine = StartCoroutine(ChangePlacebo(craftingTime, count)); ;
        }
    }

    IEnumerator ChangePlacebo(float craftingTime, int count)
    {
        yield return null; // Wait for the next frame
        isPlaceboTimeCoroutineRunning = true;

        minislider.gameObject.SetActive(true);
        minislider.value = 1f;
        Characters.instance.StartTime();
        /*while (Movement.instance.pointsByLooting.Count > 0)
        {
            minislider.value = 1f - (Movement.instance.pointsByLooting.Count / craftingTime);
            Debug.Log($"{Movement.instance.pointsByLooting.Count} {craftingTime} {Movement.instance.pointsByLooting.Count/craftingTime}");
            minislider.transform.position = new Vector3(Movement.instance.gameObject.transform.position.x, Movement.instance.gameObject.transform.position.y + 1f, 0);
            yield return null; // Wait for the next frame
        } */
        minislider.gameObject.SetActive(false); // Hide the slider
        Characters.instance.StopTime();

        isPlaceboTimeCoroutineRunning = false;
    }

    public void StomTimeLine()
    {
        if (Time.time - lastClickTime < doubleClickTime)
        {
            OnDoubleClick();
        }
        else
        {
            //DropMenuShow();
            lastClickTime = Time.time;
        }
    }

    public float doubleClickTime = 0.3f;
    private float lastClickTime = 0f;

    private void OnDoubleClick()
    {
        StopCoroutine(timeCoroutine);
        sliderpanel.SetActive(false); // Hide the slider
        Characters.instance.StopTime();
        isTimeCoroutineRunning = false;
    }
}
