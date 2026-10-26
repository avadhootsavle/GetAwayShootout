using UnityEngine;

public static class DaylightMapStyle
{
    public static void Apply(GameObject map)
    {
        foreach(var sr in map.GetComponentsInChildren<SpriteRenderer>())
        {
            if(SortingLayer.GetLayerValueFromID(sr.sortingLayerID)<0) { sr.enabled=false; continue; }
            string name=(sr.name+" "+(sr.sprite!=null?sr.sprite.name:"")).ToLowerInvariant();
            if(name.Contains("platform")||name.Contains("wall")||name.Contains("back")||name.Contains("step"))
                sr.color=MapManager.ActiveLevel==0?new Color(.84f,.86f,.83f):MapManager.ActiveLevel==1?new Color(.82f,.68f,.49f):new Color(.57f,.64f,.76f);
        }
        // Remove a leftover tutorial prop, not a racer.
        foreach(var body in map.GetComponentsInChildren<PlayerBody>())
            if(body.GetComponent<Player>()==null) Object.Destroy(body.gameObject);
    }
}
