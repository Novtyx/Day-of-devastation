using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public GameObject profile_panel;
    public GameObject profile_close;

    public void Profil_open()
    {
        Dont_move();
        profile_panel.SetActive(true);
        profile_close.SetActive(true);
    }
    public void Profil_close()
    {
        profile_panel.SetActive(false);
        profile_close.SetActive(false);
        Yes_move();
    }

    private void Yes_move()
    {
        Movement.instance.move = true;
    }
    private void Dont_move()
    {
        Movement.instance.move = false;
    }
}
