using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotifyManager : MonoBehaviour
{
    public static NotifyManager instance;
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject MinNotifypanel;
    [SerializeField] Transform MinNotifypanelTransform;
    [SerializeField] GameObject PermanentNotifypanel;
    [SerializeField] Transform PermanentNotifypanelTransform;

    void Start()
    {
        instance = this;
    }
    public void SetNotify(string message)
    {
        panel.SetActive(true);
        text.text = message;
    }
    public void SetMinNotify(string message)
    {
        GameObject obj = Instantiate(MinNotifypanel, MinNotifypanelTransform);
        obj.GetComponentInChildren<TMP_Text>().text = message;
        Destroy(obj, 3f);
    }
    public void SetPermanentNotify(string message)
    {
        GameObject obj = Instantiate(PermanentNotifypanel, PermanentNotifypanelTransform);
        obj.GetComponentInChildren<TMP_Text>().text = message;
    }
}
