#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public static class LevelDifficultyPass
{
    public static void Apply()
    {
        for(int level=2;level<=3;level++)
        {
            string path=$"Assets/Prefabs/Maps/TemplateMap{level}.prefab";
            var map=PrefabUtility.LoadPrefabContents(path);
            try
            {
                if(map.transform.Find("Extra rooftop obstacles")!=null)continue;
                var root=new GameObject("Extra rooftop obstacles");root.transform.SetParent(map.transform,false);
                float[] xs=level==2?new[]{35f}:new[]{31f,38f};float floor=level==2?3.5f:-.5f;float h=level==2?.65f:.8f;
                foreach(float x in xs)
                {
                    var block=new GameObject("Rooftop vent step");block.transform.SetParent(root.transform,false);block.transform.position=new Vector3(x,floor+h/2,0);
                    var c=block.AddComponent<BoxCollider2D>();c.size=new Vector2(1.3f,h);
                    var sr=block.AddComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");sr.drawMode=SpriteDrawMode.Tiled;sr.size=c.size;sr.color=new Color(.65f,.69f,.67f);
                    block.AddComponent<ObjectProperties>();
                }
                PrefabUtility.SaveAsPrefabAsset(map,path);
            }
            finally {PrefabUtility.UnloadPrefabContents(map);}
        }
    }
}
#endif
