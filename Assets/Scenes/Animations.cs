using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Animations : MonoBehaviour
{
    public Camera camera;
    public GameObject MovieLines;
    public GameObject HowPlayPanel;
    public GameObject HowPlayPanel1;
    [SerializeField] private GameObject SleepPanel;
    [SerializeField] private GameObject SleepButton;
    public GameObject MenuPanel;
    public GameObject clouds;

    public GameObject cursor;
    public GameObject cursorircle;

    public Vector3 pos;
    public Vector3 Cloudspos;
    public static Animations instance;
    public AnimationCurve curve;
    public Sequence CursorSequence;

    [Space]
    public GameObject SecretPanel;
    private void Start()
    {
        instance = this;
        pos = MenuPanel.GetComponent<RectTransform>().anchoredPosition;
        StartCoroutine(ChangeClouds());
    }

    public void ShowHowPlay()
    {
        HowPlayPanel.SetActive(true);
        Color i = HowPlayPanel.GetComponent<Image>().color;
        i.a = 121/255f;
        Color i1 = HowPlayPanel1.GetComponent<Image>().color;
        i1.a = 255 / 255f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(HowPlayPanel.transform.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutCubic))
            .Join(MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, -800, pos.z), 1f).SetEase(Ease.OutCubic))
            .Join(HowPlayPanel.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.OutCubic))
            .Append(HowPlayPanel1.GetComponent<Image>().DOColor(i1, 1f).SetEase(Ease.OutCubic));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void HideHowPlay()
    {
        // StartCoroutine(Hid());
        Color i1 = HowPlayPanel1.GetComponent<Image>().color;
        i1.a = 0;
        Color i = HowPlayPanel.GetComponent<Image>().color;
        i.a = 0;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(HowPlayPanel1.GetComponent<Image>().DOColor(i1, 1f).SetEase(Ease.InCubic))
            .Append(MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, pos.y, pos.z), 1f).SetEase(Ease.OutCubic))
            .Join(HowPlayPanel.transform.DOScale(new Vector3(40, 40, 40), 1f).SetEase(Ease.InCubic))
            .Join(HowPlayPanel.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.InCubic))
            .OnComplete(() => HideImage(HowPlayPanel));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void ShowSleep()
    {
        SleepPanel.SetActive(true);
        Color i = SleepPanel.GetComponent<Image>().color;
        i.a = 255 / 255f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(SleepPanel.transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutCirc))
            .Join(SleepPanel.GetComponent<Image>().DOColor(i, 0.5f).SetEase(Ease.Linear));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void HideSleep()
    {
        // StartCoroutine(Hid());
        Color i1 = SleepPanel.GetComponent<Image>().color;
        i1.a = 0;
        Vector3 scale = SleepButton.GetComponent<RectTransform>().lossyScale;
        Vector3 pos = SleepButton.GetComponent<RectTransform>().localPosition;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(SleepPanel.transform.DOScale(new Vector3(40, 40, 40), 0.5f).SetEase(Ease.InCirc))
            .Join(SleepPanel.GetComponent<Image>().DOColor(i1, 0.5f).SetEase(Ease.Linear))
            .OnComplete(() => HideImage(SleepPanel));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    private void HideImage(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
    private void HideImageAndSetAlpha0(GameObject gameObject)
    {
        Color color = gameObject.GetComponent<Image>().color;
        gameObject.SetActive(false);
        gameObject.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 219 / 255f);
        gameObject.transform.localScale = new Vector3(1, 1, 1);
    }
    private void HideSecret()
    {
        SecretPanel.SetActive(false);
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
    public void ShowSecretLocation(GameObject button)
    {
        Color color = MovieLines.GetComponent<Image>().color;
        color.a = 120/255f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(SecretPanel.GetComponent<RectTransform>().DOSizeDelta(new Vector2(900f, 500f), 1f).SetEase(Ease.OutCubic))
            .JoinCallback(() => button.SetActive(true))
            .Join(camera.transform.DORotate(new Vector3(0f, 0f, -12.1f), 1f).SetEase(curve))
            .Join(MovieLines.GetComponent<Image>().DOColor(color, 1f).SetEase(Ease.InCubic))
            .Join(MovieLines.transform.DOScale(new Vector3(1, 1, 1), 1f));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void HideSecretLocation(GameObject button)
    {
        Color color = MovieLines.GetComponent<Image>().color;
        color.a = 0f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(SecretPanel.GetComponent<RectTransform>().DOSizeDelta(new Vector2(350f, 117f), 1f).SetEase(Ease.InCubic))
            .JoinCallback(() => button.SetActive(false))
            .Join(camera.transform.DORotate(new Vector3(0f, 0f, 0f), 1f).SetEase(Ease.OutCirc))
            .Join(MovieLines.GetComponent<Image>().DOColor(color, 1f).SetEase(Ease.OutCubic))
            .Join(MovieLines.transform.DOScale(new Vector3(2, 2, 2), 1f));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void HideSecretLocationfromMap(GameObject button)
    {
        Color color = MovieLines.GetComponent<Image>().color;
        color.a = 0f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(SecretPanel.GetComponent<RectTransform>().DOSizeDelta(new Vector2(350f, 117f), 1f).SetEase(Ease.InCubic))
            .JoinCallback(() => button.SetActive(false))
            .Join(camera.transform.DORotate(new Vector3(0f, 0f, 0f), 1f).SetEase(Ease.OutCirc))
            .Join(MovieLines.GetComponent<Image>().DOColor(color, 1f).SetEase(Ease.OutCubic))
            .Join(MovieLines.transform.DOScale(new Vector3(2, 2, 2), 1f))
            .OnComplete(() => HideSecret());
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void ShowCursor(Vector2 pos, Sprite sprite, float a, Vector2 circleScale)
    {
        Color color = cursor.GetComponent<Image>().color;
        color.a = 0f;
        Color color2 = color;
        color2.a = a;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(cursor.GetComponent<Image>().DOColor(color, 0.1f).SetEase(Ease.InCubic))
            .AppendCallback(() => cursor.GetComponent<Image>().sprite = sprite)
            .Append(cursorircle.transform.DOScale(circleScale, 0.5f).SetEase(Ease.OutCubic))
            .Join(cursor.GetComponent<RectTransform>().DOSizeDelta(pos, 0.5f).SetEase(Ease.OutCubic))
            .Join(cursor.GetComponent<Image>().DOColor(color2, 0.1f).SetEase(Ease.InCubic));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }

    public void ShowTimingSlider(GameObject obj)
    {
        obj.transform.localScale = new Vector3(40, 40, 40);
        Color i = obj.GetComponent<Image>().color;
        i.a = 219 / 255f;
        obj.GetComponent<Image>().color = new Color(i.r, i.g, i.b, 0f);
        obj.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(obj.transform.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutCirc))
            .Join(obj.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.InCirc));
    }
    public void HideTimingSlider(GameObject obj)
    {
        Color i = obj.GetComponent<Image>().color;
        i.a = 0 / 255f;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(obj.transform.DOScale(new Vector3(40, 40, 40), 1f).SetEase(Ease.InCirc))
            .Join(obj.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.InCirc))
            .OnComplete(() => HideImageAndSetAlpha0(obj));
    }

    public void CursorHide(GameObject gameObject, GameObject uiObject, float a, float time = 0.5f)
    {
        CursorSequence.Kill();
        Color colorUi = uiObject.GetComponent<Image>().color;
        colorUi.a = a;
        Color color = gameObject.GetComponent<SpriteRenderer>().color;
        color.a = a;
        CursorSequence.Play();
        CursorSequence
            .Join(gameObject.GetComponent<SpriteRenderer>().DOColor(color, time).SetEase(Ease.Linear))
            .Join(uiObject.GetComponent<Image>().DOColor(colorUi, time).SetEase(Ease.Linear));
    }

    // Методы для вызова в других скриптах

    public void SetPosition(Transform transform, Vector3 pos, float time)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(pos, time).SetEase(Ease.OutCubic));
    }

    public void SetUiScale(GameObject gameObject, Vector2 vector2)
    {

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(gameObject.GetComponent<RectTransform>().DOSizeDelta(vector2, 0.5f).SetEase(Ease.OutCubic));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }

    public void SetUiAlpha(GameObject gameObject, float a, float time = 0.5f)
    {
        Color color = gameObject.GetComponent<Image>().color;
        color.a = a;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(gameObject.GetComponent<Image>().DOColor(color, time).SetEase(Ease.Linear));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
    public void SetObjectAlpha(GameObject gameObject, float a, float time = 0.5f)
    {
        Color color = gameObject.GetComponent<SpriteRenderer>().color;
        color.a = a;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(gameObject.GetComponent<SpriteRenderer>().DOColor(color, time).SetEase(Ease.Linear));
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
    }
}
