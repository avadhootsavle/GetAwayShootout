#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class LevelThreeRepair
{
    [MenuItem("Getaway/Repair Level 3 Finish")]
    public static void Apply()
    {
        const string path="Assets/Prefabs/Maps/TemplateMap3.prefab";
        var map=PrefabUtility.LoadPrefabContents(path);
        try
        {
            Transform Find(string name) { foreach(var t in map.GetComponentsInChildren<Transform>(true)) if(t.name==name)return t;return null; }
            if(Find("Finish access step")!=null)return;
            Find("Border").position+=Vector3.up*5;
            var step=new GameObject("Finish access step");step.transform.SetParent(map.transform,false);step.transform.position=new Vector3(45.25f,-.375f,0);
            var collider=step.AddComponent<BoxCollider2D>();collider.size=new Vector2(4.5f,2.25f);
            var sprite=step.AddComponent<SpriteRenderer>();sprite.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");sprite.drawMode=SpriteDrawMode.Tiled;sprite.size=collider.size;sprite.color=new Color(.65f,.69f,.67f);
            step.AddComponent<ObjectProperties>().transparency=0;
            var landing=new GameObject("Gap landing step");landing.transform.SetParent(map.transform,false);landing.transform.position=new Vector3(15.5f,-.125f,0);
            var landingCollider=landing.AddComponent<BoxCollider2D>();landingCollider.size=new Vector2(7,2.25f);
            var landingSprite=landing.AddComponent<SpriteRenderer>();landingSprite.sprite=sprite.sprite;landingSprite.drawMode=SpriteDrawMode.Tiled;landingSprite.size=landingCollider.size;landingSprite.color=sprite.color;
            landing.AddComponent<ObjectProperties>().transparency=0;
            var wm=map.GetComponentInChildren<WaypointManager>();
            foreach(var old in wm.GetComponentsInChildren<Waypoint>())Object.DestroyImmediate(old.gameObject);
            Vector2[] points={new Vector2(-4.48f,2.2f),new Vector2(3,2.2f),new Vector2(9,2.2f),new Vector2(16.5f,1.2f),new Vector2(21.5f,2.86f),new Vector2(28,-.3f),new Vector2(40,-.3f),new Vector2(44.5f,.95f),new Vector2(48,2.2f)};
            for(int i=0;i<points.Length;i++)
            {
                var waypoint=new GameObject("Route "+i);waypoint.transform.SetParent(wm.transform,false);waypoint.transform.position=points[i];
                var wp=waypoint.AddComponent<Waypoint>();wp.horizontalZone=Waypoint.HorizontalDirection.RIGHT;wp.zoneOffset=new Vector2(-1,0);wp.spawnable=i<points.Length-1;
            }
            PrefabUtility.SaveAsPrefabAsset(map,path);
        }
        finally {PrefabUtility.UnloadPrefabContents(map);}
    }
}
#endif
