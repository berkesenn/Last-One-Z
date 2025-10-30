using UnityEngine;
using UnityEditor;

public class LightmapChecker : EditorWindow
{
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Check Baked Lightmaps")]
    static void ShowWindow()
    {
        GetWindow<LightmapChecker>("Lightmap Checker");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Lightmap Bake Status", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Check All Objects", GUILayout.Height(30)))
        {
            CheckLightmaps();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Select Unbaked Objects", GUILayout.Height(30)))
        {
            SelectUnbakedObjects();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Make All Static", GUILayout.Height(30)))
        {
            MakeAllStatic();
        }
    }
    
    void CheckLightmaps()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        int bakedCount = 0;
        int unbakedCount = 0;
        
        Debug.Log("=== LIGHTMAP CHECK STARTED ===");
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex == -1 || renderer.lightmapIndex == 65535)
            {
                // Bake edilmemiş
                Debug.LogWarning($"❌ NOT BAKED: {GetFullPath(renderer.transform)}", renderer.gameObject);
                unbakedCount++;
            }
            else
            {
                // Bake edilmiş
                bakedCount++;
            }
        }
        
        Debug.Log($"=== RESULTS ===");
        Debug.Log($"✅ Baked Objects: {bakedCount}");
        Debug.Log($"❌ Unbaked Objects: {unbakedCount}");
        Debug.Log($"📊 Total Objects: {renderers.Length}");
        
        if (unbakedCount == 0)
        {
            Debug.Log("🎉 ALL OBJECTS ARE BAKED!");
        }
    }
    
    void SelectUnbakedObjects()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        System.Collections.Generic.List<GameObject> unbakedObjects = new System.Collections.Generic.List<GameObject>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex == -1 || renderer.lightmapIndex == 65535)
            {
                unbakedObjects.Add(renderer.gameObject);
            }
        }
        
        if (unbakedObjects.Count > 0)
        {
            Selection.objects = unbakedObjects.ToArray();
            Debug.Log($"Selected {unbakedObjects.Count} unbaked objects in Hierarchy");
        }
        else
        {
            Debug.Log("No unbaked objects found!");
        }
    }
    
    void MakeAllStatic()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        int madeStaticCount = 0;
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Player, zombiler, silahlar gibi dinamik objeleri atla
            if (renderer.gameObject.CompareTag("Player") || 
                renderer.gameObject.name.Contains("Zombie") ||
                renderer.gameObject.name.Contains("Weapon"))
            {
                continue;
            }
            
            if (!GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.ContributeGI))
            {
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.ContributeGI);
                madeStaticCount++;
            }
        }
        
        Debug.Log($"Made {madeStaticCount} objects static (Lightmap Static)");
        
        if (madeStaticCount > 0)
        {
            Debug.Log("Now go to Lighting window and click 'Generate Lighting'");
        }
    }
    
    string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
