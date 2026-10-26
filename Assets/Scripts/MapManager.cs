using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject[] maps;
    public static int ActiveLevel { get; private set; }

    public void CreateMap(GameState state)
    {
        if(state.curMap == -1)
            state.curMap = 0;

        ActiveLevel=state.curMap;
        GameObject map = Instantiate(maps[state.curMap]);
        DaylightMapStyle.Apply(map);
        var fallZone=new GameObject("Fall recovery");fallZone.transform.SetParent(map.transform,false);
        fallZone.transform.position=new Vector3(20,-18,0);fallZone.tag="Death";
        var trigger=fallZone.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(2000,4);
        state.curMap = (state.curMap + 1) % maps.Length;
    }
}
