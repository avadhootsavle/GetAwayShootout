using UnityEngine;

// Adds non-colliding edge lights without altering the authored map geometry.
public static class NeonMapStyle
{
    public static void Apply(GameObject map)
    {
        foreach(var sr in map.GetComponentsInChildren<SpriteRenderer>())
        {
            string n=(sr.gameObject.name + " " + (sr.sprite != null ? sr.sprite.name : "")).ToLowerInvariant();
            // The camera skyline replaces only the far background artwork.
            if (SortingLayer.GetLayerValueFromID(sr.sortingLayerID) < 0) { sr.enabled=false; continue; }
            if (sr.GetComponentInParent<PowerUpBox>() == null && !n.Contains("finish") && !n.Contains("alarm"))
                sr.color=new Color(.38f,.48f,.64f);
            if(n.Contains("wall") || n=="back") sr.color=new Color(.32f,.43f,.65f);
            if(!n.Contains("platform") || sr.bounds.size.x<.5f) continue;
            var edge=new GameObject("Neon edge"); edge.transform.SetParent(map.transform,true);
            Bounds b=sr.bounds; edge.transform.position=new Vector3(b.center.x,b.max.y-.025f,sr.transform.position.z-.01f);
            var glow=edge.AddComponent<SpriteRenderer>(); glow.sprite=UIThemeHelper.GetRoundedSprite(2,Color.white,Color.white,0);
            glow.color=new Color(.18f,.88f,.96f); glow.sortingLayerID=sr.sortingLayerID; glow.sortingOrder=sr.sortingOrder+1;
            edge.transform.localScale=new Vector3(b.size.x/glow.sprite.bounds.size.x,.045f/glow.sprite.bounds.size.y,1);
        }
    }
}
