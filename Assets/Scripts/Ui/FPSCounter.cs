using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;

    private float accum = 0f;
    private int frames = 0;
    private float timeleft;
    private float fps = 0f;

    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeleft <= 0f)
        {
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0f;
            frames = 0;

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }
        }
    }
}