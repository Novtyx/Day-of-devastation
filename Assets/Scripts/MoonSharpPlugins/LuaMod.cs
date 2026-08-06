using UnityEngine;
using XLua;
using System.IO;
using System;

public class LuaMod
{
    public readonly string name;
    public readonly string modDir;
    public ModConfig config;
    private LuaScript[] scripts;

    public LuaMod(string name, string modDir, string[] luaScriptsPath)
    {
        this.name = name;
        this.modDir = modDir;
        scripts = new LuaScript[luaScriptsPath.Length];

        for (int i = 0; i < luaScriptsPath.Length; i++)
        {
            scripts[i] = new LuaScript(luaScriptsPath[i]);
        }
    }

    public void CallStart()
    {
        foreach (LuaScript script in scripts) script.CallStart();
    }

    public void CallUpdate()
    {
        foreach (LuaScript script in scripts) script.CallUpdate();
    }

    // Вызывается при удалении/перезагрузке модов для очистки памяти
    public void Dispose()
    {
        foreach (LuaScript script in scripts) script.Dispose();
    }

    private class LuaScript
    {
        public string path;
        private LuaTable scriptEnv;
        private Action startFunc;
        private Action updateFunc;

        public LuaScript(string path)
        {
            this.path = path;

            try
            {
                // 1. Создаем изолированную таблицу для скрипта
                scriptEnv = LuaModsManager.GlobalLuaEnv.NewTable();

                // 2. Устанавливаем метатаблицу
                LuaTable meta = LuaModsManager.GlobalLuaEnv.NewTable();
                meta.Set("__index", LuaModsManager.GlobalLuaEnv.Global);
                scriptEnv.SetMetaTable(meta);
                meta.Dispose();

                InitAPI();

                // 3. Считываем текст
                string luaCode = File.ReadAllText(path, System.Text.Encoding.UTF8);

                // ВЫПОЛНЕНИЕ КОДА (Здесь чаще всего падают синтаксические ошибки Lua)
                LuaModsManager.GlobalLuaEnv.DoString(luaCode, Path.GetFileName(path), scriptEnv);

                // 4. Достаем функции (Здесь падают ошибки биндинга/каста делегатов)
                startFunc = scriptEnv.Get<Action>("start");
                updateFunc = scriptEnv.Get<Action>("update");

                Debug.Log($"<color=green>[xLua]</color> Скрипт успешно инициализирован: {Path.GetFileName(path)}");
            }
            catch (Exception e)
            {
                // Теперь мы точно увидим, из-за чего прерывается выполнение!
                Debug.LogError($"<color=red>[xLua Error]</color> Ошибка при загрузке скрипта {Path.GetFileName(path)}:\n{e.Message}\n{e.StackTrace}");
            }
        }

        private void InitAPI()
        {
            scriptEnv.Set("changeColor", (Action)delegate ()
            {
                GameObject.Find("Square").GetComponent<SpriteRenderer>().color = Color.magenta;
            });
        }

        public void CallStart() => startFunc?.Invoke();
        public void CallUpdate() => updateFunc?.Invoke();

        public void Dispose()
        {
            startFunc = null;
            updateFunc = null;
            scriptEnv?.Dispose();
        }
    }
}