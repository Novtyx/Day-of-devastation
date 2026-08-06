using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;
    public GameObject EventPanel;
    public GameObject ParentEventPanel;
    public Button button;
    public Button button1;
    public TMP_Text title;
    public TMP_Text description;
    private Event Event;


    private void Start()
    {
        instance = this;
    }

    public void StartEvent(Event event1)
    {
        Event = null;
        button.onClick.RemoveAllListeners();
        EventPanel.GetComponent<Image>().sprite = event1.sprite;
        this.title.text = event1.title;
        this.description.text = event1.description;
        Event = event1;
        if (event1.IsSearching) button.onClick.AddListener(SearchEvent);
        button.gameObject.GetComponent<Image>().color = event1.buttonColor;
        button.gameObject.GetComponentInChildren<TMP_Text>().color = event1.buttonColor;
        button1.gameObject.GetComponent<Image>().color = event1.buttonColor;
        button1.gameObject.GetComponentInChildren<TMP_Text>().color = event1.buttonColor;
        ParentEventPanel.GetComponent<Image>().color = new Color(event1.panelColor.r, event1.panelColor.g, event1.panelColor.b, 0f);
        title.color = event1.textColor;
        description.color = event1.textColor;
        Animations.instance.ShowHowPlay();
    }

    public void StartEvent1(Event event1, GameObject gameObject)
    {
        Event = null;
        locationbutton.onClick.RemoveAllListeners();
        //locationEventPanel.GetComponent<Image>().sprite = event1.sprite;
        this.locationtitle.text = event1.title;
        this.locationdescription.text = event1.description;
        Event = event1;
        if (event1.IsSearching) locationbutton.onClick.AddListener(() => locationSearchEvent(gameObject));
        //locationbutton.gameObject.GetComponent<Image>().color = event1.buttonColor;
        //locationbutton.gameObject.GetComponentInChildren<TMP_Text>().color = event1.buttonColor;
        //locationbutton1.gameObject.GetComponent<Image>().color = event1.buttonColor;
        //locationbutton1.gameObject.GetComponentInChildren<TMP_Text>().color = event1.buttonColor;
        //locationEventPanel.GetComponent<Image>().color = new Color(event1.panelColor.r, event1.panelColor.g, event1.panelColor.b, 0f);
        //locationtitle.color = event1.textColor;
        //locationdescription.color = event1.textColor;
    }

    public GameObject locationPanel;
    public GameObject locationEventPanel;
    public Button locationbutton;
    public Button locationbutton1;
    public TMP_Text locationtitle;
    public TMP_Text locationdescription;

    public void SearchEvent()
    {
        for (int i = 0; i < Event.items.Count; i++)
        {
            Inventory.instance.AddItem(Event.items[i], Event.itemsCounts[i], null);
        }
        Animations.instance.HideHowPlay();
        RandomIvents.instance.ReportLootOutcome(true);
    }
    public void locationSearchEvent(GameObject gameObject)
    {
        for (int i = 0; i < Event.items.Count; i++)
        {
            Inventory.instance.AddItem(Event.items[i], Event.itemsCounts[i], null);
        }
        Animations.instance.HideSecretLocationfromMap(locationbutton.gameObject);
        Destroy(gameObject);
    }
}
