using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua; // Заменяем MoonSharp на xLua

public class LuaModsManager : MonoBehaviour
{
    public bool isLoaded = false;
    // Единое окружение xLua для всей игры (создавать несколько крайне ресурсоемко)
    public static LuaEnv GlobalLuaEnv;

    public List<LuaMod> mods;
    private string modFolderPath;

    [SerializeField] private ModListItem modListPrefab;
    [SerializeField] private Transform modScrollContent;
    [SerializeField] private FilterMode iconFilterMode = FilterMode.Bilinear;
    [SerializeField] private string authorPrefix = "Автор";

    private void Awake()
    {
        try
        {
            // Инициализируем xLua при старте
            if (GlobalLuaEnv == null)
            {
                GlobalLuaEnv = new LuaEnv();
            }

            if (mods != null)
            {
                foreach (LuaMod luaMod in mods)
                {
                    ModConfig config = luaMod.config;
                    string iconPath = Path.Combine(luaMod.modDir, config.iconPath);
                    SpawnModListItem(config, iconPath);
                }
                return;
            }

            LoadMods();
            Debug.Log("loadsmods");
        }
        catch
        {
            isLoaded = true;
        }
        isLoaded = true;
    }

    public void ReloadMods()
    {
        if (mods != null)
        {
            // В xLua обязательно нужно очищать ссылки на Lua-функции перед удалением
            foreach (var mod in mods) mod.Dispose();
            mods.Clear();
        }

        mods = null;
        LoadMods();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadMods()
    {
        mods = new List<LuaMod>();

        // ВАЖНО ДЛЯ ANDROID: Используем persistentDataPath вместо Environment.CurrentDirectory
        modFolderPath = Path.Combine(Application.persistentDataPath, "mods");

        if (Directory.Exists(modFolderPath) == false)
            Directory.CreateDirectory(modFolderPath);

        foreach (string modDir in Directory.GetDirectories(modFolderPath))
        {
            Debug.Log(modDir);
            string configPath = Path.Combine(modDir, "config.json");

            if (File.Exists(configPath) == false)
            {
                Debug.Log("null config");
                continue;
            }
            Debug.Log(modDir);
            string[] scriptFiles = Directory.GetFiles(modDir, "*.lua*", SearchOption.AllDirectories);
            Debug.Log(modDir);
            if (scriptFiles.Length > 0)
            {
                Debug.Log(scriptFiles.Length);
                LuaMod mod = new LuaMod(Path.GetFileName(modDir), modDir, scriptFiles);
                Debug.Log(scriptFiles.Length);
                ModConfig config = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
                Debug.Log(scriptFiles.Length);
                string iconPath = Path.Combine(modDir, config.iconPath);
                Debug.Log(scriptFiles.Length);
                mod.config = config;

                Debug.Log(scriptFiles.Length);
                mods.Add(mod);
                Debug.Log(scriptFiles.Length);
                SpawnModListItem(config, iconPath);
                Debug.Log("lllalallalala");
            }
            Debug.Log(modDir);
        }
    }

    public void SpawnModListItem(ModConfig config, string iconPath)
    {
        GameObject listObj = GameObject.Instantiate(modListPrefab.gameObject, modScrollContent);
        ModListItem listItem = listObj.GetComponent<ModListItem>();

        listItem.icon.sprite = LoadSpriteFull(iconPath, iconFilterMode);
        listItem.modName.text = config.name;
        listItem.description.text = config.description;
        listItem.author.text = $"{authorPrefix} {config.author}";
    }

    private Sprite LoadSpriteFull(string fullPath, FilterMode mode = FilterMode.Bilinear, int pixelsPerUnit = 32)
    {
        Texture2D texture = LoadTextureFull(fullPath, mode);
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one, pixelsPerUnit);
    }

    public Texture2D LoadTextureFull(string fullPath, FilterMode mode = FilterMode.Bilinear)
    {
        byte[] data;
        try { data = File.ReadAllBytes(fullPath); }
        catch { return null; }

        Texture2D texture = new Texture2D(0, 0);
        if (!texture.LoadImage(data)) throw new InvalidDataException("Texture load failed");

        texture.filterMode = mode;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private void Start()
    {
        if (mods != null)
        {
            foreach (LuaMod mod in mods) mod.CallStart();
        }
    }

    private void Update()
    {
        if (mods != null)
        {
            foreach (LuaMod mod in mods) mod.CallUpdate();
        }
    }

    private void OnDestroy()
    {
        // Очистка окружения при закрытии игры
        if (mods != null)
        {
            foreach (var mod in mods) mod.Dispose();
        }

        if (GlobalLuaEnv != null)
        {
            GlobalLuaEnv.Dispose();
            GlobalLuaEnv = null;
        }
    }
}