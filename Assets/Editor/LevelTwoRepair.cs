#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class LevelTwoRepair
{
    [MenuItem("Getaway/Repair Level 2 Route")]
    public static void Apply()
    {
        const string path="Assets/Prefabs/Maps/TemplateMap2.prefab";
        var map=PrefabUtility.LoadPrefabContents(path);
        try
        {
            Transform Find(string name) { foreach(var t in map.GetComponentsInChildren<Transform>(true)) if(t.name==name) return t; return null; }
            if (Find("Access step") != null) return; // Already repaired.
            // Shift complete prefab groups so sprites and collision remain aligned.
            Find("Platform (1)").position+=Vector3.down*3.5f;
            Find("Platform (2)").position+=Vector3.down*1.5f;
            Find("FreePlatform").position+=new Vector3(-2.5f,.34f,0);
            Find("Border").position+=Vector3.up*5;
            var oldStep=Find("Access step"); if(oldStep!=null) Object.DestroyImmediate(oldStep.gameObject);
            var step=new GameObject("Access step"); step.transform.SetParent(map.transform,false); step.transform.position=new Vector3(.5f,-1.3f,0);
            var collider=step.AddComponent<BoxCollider2D>(); collider.size=new Vector2(3,1.6f);
            var sprite=step.AddComponent<SpriteRenderer>();
            sprite.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png"); sprite.drawMode=SpriteDrawMode.Tiled; sprite.size=collider.size; sprite.color=new Color(.65f,.69f,.67f);
            var props=step.AddComponent<ObjectProperties>(); props.transparency=0;
            // Author navigation and respawn points on the actual safe landing surfaces.
            var wm=map.GetComponentInChildren<WaypointManager>();
            foreach(var w in wm.GetComponentsInChildren<Waypoint>()) Object.DestroyImmediate(w.gameObject);
            Vector2[] points={new Vector2(-4.5f,-1.8f),new Vector2(.4f,-.3f),new Vector2(5.4f,1.2f),new Vector2(11,1.2f),new Vector2(18.4f,2.2f),new Vector2(26,3.7f),new Vector2(40,3.7f),new Vector2(46,5.2f)};
            for(int i=0;i<points.Length;i++)
            {
                var go=new GameObject("Route "+i); go.transform.SetParent(wm.transform,false); go.transform.position=points[i];
                var wp=go.AddComponent<Waypoint>(); wp.horizontalZone=Waypoint.HorizontalDirection.RIGHT; wp.zoneOffset=new Vector2(-1,0); wp.spawnable=i<points.Length-1;
            }
            // Move buried pickups above their platforms; remove the tutorial's stray ragdoll.
            Find("PowerUpBox (3)").position=new Vector3(8,2.7f,0);
            Find("PowerUpBox (4)").position=new Vector3(18,3.9f,0);
            Find("PowerUpBox (5)").position=new Vector3(30,5.2f,0);
            var doll=Find("RagdollPlayer"); if(doll!=null) Object.DestroyImmediate(doll.gameObject);
            PrefabUtility.SaveAsPrefabAsset(map,path);
        }
        finally { PrefabUtility.UnloadPrefabContents(map); }
    }
}
#endif
