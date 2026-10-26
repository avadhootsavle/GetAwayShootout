using UnityEngine;
using UnityEngine.UI;

// Body art stays pixel-based; photographic heads are separate, full-resolution sprites.
public static class FighterArt
{
    static Sprite[] originals;
    static readonly Sprite[] bodies=new Sprite[5];
    static readonly Sprite[] faces=new Sprite[5];
    static readonly Sprite[] arms=new Sprite[5];
    public static bool HasPhoto(int index)=>index==0 || index==1 || index==4;
    static Color Shirt(int index)=>index==0?new Color32(245,241,217,255):index==1?new Color32(228,88,73,255):new Color32(66,117,159,255);
    public static Sprite Face(int index)
    {
        if(!HasPhoto(index))
        {
            if(faces[index]==null) { var body=Original(index,"body"); var r=body.rect; faces[index]=Sprite.Create(body.texture,new Rect(r.x,r.y+50,r.width,26),new Vector2(.5f,.5f),32); }
            return faces[index];
        }
        if(faces[index]==null) faces[index]=Resources.Load<Sprite>(index==4?"Faces/Avadhoot":"Faces/Player"+(index+1));
        return faces[index];
    }
    public static Sprite Body(int index)
    {
        if(!HasPhoto(index)) return Original(index,"body");
        if(bodies[index]!=null) return bodies[index];
        var tex=new Texture2D(34,76,TextureFormat.RGBA32,false); tex.filterMode=FilterMode.Point;
        var px=new Color[34*76];
        void Box(int x,int y,int w,int h,Color c) { for(int yy=y;yy<y+h;yy++) for(int xx=x;xx<x+w;xx++) px[yy*34+xx]=c; }
        Color shirt=Shirt(index);
        Color pants=index==0?new Color32(66,113,184,255):new Color32(44,54,70,255);
        Color skin=new Color32(196,145,99,255);
        Box(12,4,11,26,pants); Box(12,4,3,26,Color.Lerp(pants,Color.black,.15f));
        Box(10,0,18,5,PixelUI.Ink); Box(10,4,15,2,new Color32(83,90,92,255));
        Box(11,26,13,21,shirt); Box(11,26,3,21,Color.Lerp(shirt,Color.black,.13f));
        Box(11,24,13,3,Color.Lerp(shirt,Color.black,.3f)); Box(15,46,6,5,skin);
        if(Face(index)==null) { Box(10,49,17,20,skin); Box(9,65,19,6,PixelUI.Ink); Box(23,57,2,2,PixelUI.Ink); }
        tex.SetPixels(px); tex.Apply(); bodies[index]=Sprite.Create(tex,new Rect(0,0,34,76),new Vector2(.5f,.5f),32); return bodies[index];
    }
    public static Sprite Original(int index,string part)
    {
        if(HasPhoto(index) && part=="arm")
        {
            if(arms[index]!=null) return arms[index];
            var tex=new Texture2D(34,12,TextureFormat.RGBA32,false); tex.filterMode=FilterMode.Point; var pixels=new Color[34*12];
            Color shirt=Shirt(index);
            for(int y=3;y<9;y++)for(int x=6;x<28;x++)pixels[y*34+x]=x<19?shirt:new Color32(196,145,99,255);
            tex.SetPixels(pixels);tex.Apply();arms[index]=Sprite.Create(tex,new Rect(0,0,34,12),new Vector2(.5f,.5f),32);return arms[index];
        }
        if(originals==null) originals=Resources.LoadAll<Sprite>("Players");
        foreach(var sprite in originals) if(sprite.name==$"Players_{index}_{part}") return sprite;
        return null;
    }
    public static RectTransform Preview(Transform parent,int index,Vector2 position,float height,bool animate=false)
    {
        var rt=PixelUI.Rect("Fighter "+index,parent,position,new Vector2(height*34/76,height));
        var body=rt.gameObject.AddComponent<Image>(); body.sprite=Body(index); body.preserveAspect=true; body.raycastTarget=false;
        var arm=PixelUI.Fill("Arm",rt,new Vector2(8*height/76,-2*height/76),new Vector2(34*height/76,12*height/76),Color.white);
        arm.sprite=Original(index,"arm"); arm.preserveAspect=true; arm.rectTransform.localRotation=Quaternion.Euler(0,0,-80);
        if(HasPhoto(index) && Face(index)!=null)
        {
            var head=PixelUI.Fill("Face",rt,new Vector2(0,22*height/76),new Vector2(24*height/76,30*height/76),Color.white);
            head.sprite=Face(index); head.preserveAspect=true;
        }
        if(animate) rt.gameObject.AddComponent<FighterIdle>();
        return rt;
    }
}
public class FighterIdle : MonoBehaviour
{
    Vector3 origin;
    void Start() { origin=transform.localPosition; }
    void Update() { float t=Time.unscaledTime*1.6f+origin.x; transform.localPosition=origin+Vector3.up*Mathf.Sin(t)*2; transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t)*1.5f); }
}
