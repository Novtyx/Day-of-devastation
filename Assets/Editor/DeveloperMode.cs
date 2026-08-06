using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DeveloperMode : EditorWindow
{
    [MenuItem("Window/Custom/Enable Developer Mode")]
    static void EnableDevMode()
    {
        EditorPrefs.SetBool("DeveloperMode", true);
    }


    [MenuItem("Window/Custom/Disable Developer Mode")]
    static void DisableDevMode()
    {
        EditorPrefs.SetBool("DeveloperMode", false);
    }
}
