using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Uitest : MonoBehaviour, IDragHandler, IDropHandler
{
    public GameObject[] chars;

    public void OnDrag(PointerEventData eventData)
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition += eventData.delta;
        foreach (GameObject obj in chars)
        {
            obj.GetComponent<RectTransform>().anchoredPosition += eventData.delta;
        }
    }
    private Button button;
    public void StopButton()
    {
        button = GetComponent<Button>();
        button.enabled = false;
    }
    public void StartButton()
    {
        button = GetComponent<Button>();
        button.enabled = true;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (gameObject.GetComponent<RectTransform>().anchoredPosition.y >= -83)
        {
            UIManager.stena = false;
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-851.5596f, 0);
            float xpos = -633.8301f;
            foreach (GameObject obj in chars)
            {
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xpos, 0);
                xpos += 217.23f;
            }
        }
        else if (gameObject.GetComponent<RectTransform>().anchoredPosition.y <= -83 && gameObject.GetComponent<RectTransform>().anchoredPosition.x <= -533)
        {
            UIManager.stena = true;
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-851.5596f, 0);
            float ypos = -84;
            foreach (GameObject obj in chars)
            {
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-851.5596f, ypos);
                ypos -= 84;
            }
        }
    }
}
