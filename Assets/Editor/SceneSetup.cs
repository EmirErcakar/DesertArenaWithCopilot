using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Linq;

public class SceneSetup
{
    [MenuItem("Tools/Setup Desert Arena Scene")]
    public static void SetupScene()
    {
        // 1. Arena (Plane)
        if (GameObject.Find("Arena") == null)
        {
            var arena = GameObject.CreatePrimitive(PrimitiveType.Plane);
            arena.name = "Arena";
            arena.transform.position = Vector3.zero;
            arena.transform.localScale = new Vector3(5f, 1f, 5f);
            Debug.Log("[SceneSetup] Arena oluşturuldu.");
        }

        // 2. Player (Capsule)
        if (GameObject.Find("Player") == null)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            AddComponentByName(player, "PlayerController");
            Debug.Log("[SceneSetup] Player oluşturuldu.");
        }

        // 3. Main Camera - CameraFollow
        var cam = Camera.main;
        if (cam != null)
        {
            AddComponentByName(cam.gameObject, "CameraFollow");
            Debug.Log("[SceneSetup] CameraFollow eklendi.");
        }

        // 4. Canvas + JoystickArea
        if (UnityEngine.Object.FindAnyObjectByType<Canvas>() == null)
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var joystickArea = new GameObject("JoystickArea");
            joystickArea.transform.SetParent(canvasGO.transform, false);
            AddComponentByName(joystickArea, "FloatingJoystick");
            Debug.Log("[SceneSetup] Canvas ve JoystickArea oluşturuldu.");
        }

        // 5. EventSystem
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[SceneSetup] EventSystem oluşturuldu.");
        }

        // 6. GameManager
        if (GameObject.Find("GameManager") == null)
        {
            var gm = new GameObject("GameManager");
            AddComponentByName(gm, "GameManager");
            AddComponentByName(gm, "LevelManager");
            AddComponentByName(gm, "EnemySpawner");
            Debug.Log("[SceneSetup] GameManager oluşturuldu.");
        }

        // 7. Bootstrapper
        if (GameObject.Find("Bootstrapper") == null)
        {
            var bootstrapper = new GameObject("Bootstrapper");
            AddComponentByName(bootstrapper, "GameBootstrapper");
            Debug.Log("[SceneSetup] Bootstrapper oluşturuldu.");
        }

        // 8. Tags
        AddTagIfMissing("Enemy");
        AddTagIfMissing("Projectile");

        Debug.Log("[SceneSetup] Desert Arena sahnesi başarıyla kuruldu!");
    }

    static void AddComponentByName(GameObject go, string typeName)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
            .FirstOrDefault(t => t.Name == typeName);

        if (type != null)
        {
            if (go.GetComponent(type) == null)
                go.AddComponent(type);
        }
        else
        {
            Debug.LogWarning($"[SceneSetup] '{typeName}' tipi bulunamadı — script henüz derlenmemiş olabilir.");
        }
    }

    static void AddTagIfMissing(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[SceneSetup] '{tag}' tag'i eklendi.");
    }
}
