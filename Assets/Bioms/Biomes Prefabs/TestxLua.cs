using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class TestxLua : MonoBehaviour
{
    LuaEnv luaenv = new LuaEnv();
    void Start()
    {
        luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            luaenv.DoString("CS.Movement.instance:Movement_start()");
        }
    }
    private void OnDestroy()
    {
        luaenv.Dispose();
    }
}
