using UnityEngine;
using UnityEngine.UI;

// Editable pixel scenery, shared by the lobby and end screen.
[RequireComponent(typeof(CanvasRenderer))]
public class ArcadeBackdrop : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); var r=rectTransform.rect;
        Quad(vh,r.xMin,r.yMin,r.width,r.height,new Color32(41,190,219,255));
        Cloud(vh,r.xMin+r.width*.12f,r.yMin+r.height*.77f,5);
        Cloud(vh,r.xMin+r.width*.77f,r.yMin+r.height*.84f,4);
        Cloud(vh,r.xMin+r.width*.53f,r.yMin+r.height*.52f,3);
        var rng=new System.Random(28);
        Color[] walls={new Color32(210,194,133,255),new Color32(180,200,173,255),new Color32(148,172,160,255),new Color32(226,206,147,255)};
        for(int layer=0;layer<2;layer++)
        {
            float x=r.xMin-15;
            while(x<r.xMax)
            {
                float w=rng.Next(65,120), h=rng.Next(70,210)*(layer==0?1.3f:1);
                Color wall=walls[rng.Next(walls.Length)]; if(layer==0) wall=Color.Lerp(wall,new Color(.55f,.8f,.82f),.3f);
                Quad(vh,x,r.yMin+65,w,h,wall);
                Quad(vh,x-5,r.yMin+65+h,w+10,7,Color.Lerp(wall,Color.white,.15f));
                for(float xx=x+10;xx<x+w-14;xx+=26) for(float yy=r.yMin+84;yy<r.yMin+55+h;yy+=34)
                {
                    Quad(vh,xx,yy,16,23,Color.Lerp(wall,PixelUI.Ink,.22f));
                    Quad(vh,xx+4,yy+3,10,19,new Color32(184,209,205,255));
                }
                x+=w+9;
            }
        }
        Quad(vh,r.xMin,r.yMin,r.width,75,new Color32(121,129,132,255));
        Quad(vh,r.xMin,r.yMin+66,r.width,9,new Color32(222,225,216,255));
        Quad(vh,r.xMin,r.yMin+58,r.width,6,PixelUI.Ink);
    }
    static void Cloud(VertexHelper vh,float x,float y,float s)
    {
        Color c=new Color32(239,248,239,255);
        Quad(vh,x,y,27*s,4*s,c); Quad(vh,x+4*s,y+4*s,17*s,4*s,c); Quad(vh,x+11*s,y+8*s,7*s,3*s,c);
    }
    static void Quad(VertexHelper vh,float x,float y,float w,float h,Color c)
    {
        int n=vh.currentVertCount; vh.AddVert(new Vector3(x,y),c,Vector2.zero); vh.AddVert(new Vector3(x,y+h),c,Vector2.zero);
        vh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero); vh.AddVert(new Vector3(x+w,y),c,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
    }
    public static void Add(Transform parent)
    {
        var go=PixelUI.Root("City",parent); go.AddComponent<ArcadeBackdrop>().raycastTarget=false;
    }
}
