using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AnimationsMenu : MonoBehaviour
{
    public GameObject HowPlayPanel;
    public GameObject MenuPanel;
    public GameObject clouds;
    public Vector3 pos;
    public Vector3 Cloudspos;
    private void Start()
    {
        pos = MenuPanel.GetComponent<RectTransform>().anchoredPosition;
        StartCoroutine(ChangeClouds());
    }

    public void ShowHowPlay()
    {
        StopCoroutine(Hid());
        MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, -800, pos.z), 1f).SetEase(Ease.OutCubic);
        HowPlayPanel.transform.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutCubic);
        Color i = HowPlayPanel.GetComponent<Image>().color;
        i.a = 121/255f;
        HowPlayPanel.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.OutCubic);
    }
    public void HideHowPlay()
    {
        StartCoroutine(Hid());
    }
    public IEnumerator Hid()
    {
        MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, pos.y, pos.z), 1f).SetEase(Ease.OutCubic);
        HowPlayPanel.transform.DOScale(new Vector3(40, 40, 40), 1f).SetEase(Ease.InCubic);
        Color i = HowPlayPanel.GetComponent<Image>().color;
        i.a = 0;
        HowPlayPanel.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(1f);
    }
    public IEnumerator ChangeClouds()
    {
        while (true)
        {
            clouds.GetComponent<RectTransform>().DOAnchorPos(Cloudspos, 30f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(30f);
            clouds.GetComponent<RectTransform>().anchoredPosition = -Cloudspos;
        }
    }
}
