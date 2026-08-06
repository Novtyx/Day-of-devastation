using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class ToggleG : MonoBehaviour
{
    ToggleGroup toggleGroup;
    public Toggle fps_30;
    public Toggle fps_60;
    public Toggle fps_90;
    public Toggle fps_120;
    // Start is called before the first frame update
    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        string togle = PlayerPrefs.GetString("fps", "60");
        if (togle == "60") fps_60.isOn = true;
        else if (togle == "90") fps_90.isOn = true;
        else if (togle == "120") fps_120.isOn = true;
        else fps_30.isOn = true;
    }

    public void Sumbit()
    {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        PlayerPrefs.SetString("fps", toggle.GetComponentInChildren<TMP_Text>().text);
        Application.targetFrameRate = int.Parse(toggle.GetComponentInChildren<TMP_Text>().text);
    }
}
