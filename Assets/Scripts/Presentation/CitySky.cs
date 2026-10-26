using UnityEngine;

public class CitySky : MonoBehaviour
{
    static readonly Sprite[] artworks=new Sprite[3];
    SpriteRenderer sky;
    Camera view;
    public static void Attach(Camera camera,int index)
    {
        var go=new GameObject("Daylight city"); go.transform.SetParent(camera.transform,false); go.transform.localPosition=new Vector3(0,0,40); go.layer=14+index;
        var component=go.AddComponent<CitySky>(); component.view=camera; component.sky=go.AddComponent<SpriteRenderer>(); component.sky.sprite=GetArtwork(); component.sky.sortingOrder=-1000; component.LateUpdate();
    }
    void LateUpdate()
    {
        float h=view.orthographicSize*2; float scale=Mathf.Max(h*view.aspect/sky.sprite.bounds.size.x,h/sky.sprite.bounds.size.y);
        transform.localScale=new Vector3(scale,scale,1);
    }
    static Sprite GetArtwork()
    {
        int level=Mathf.Clamp(MapManager.ActiveLevel,0,2);
        if(artworks[level]!=null)return artworks[level];
        const int w=640,h=360; var pixels=new Color[w*h];
        Color skyColor=level==0?new Color32(41,190,219,255):level==1?new Color32(237,167,114,255):new Color32(42,48,83,255);
        for(int i=0;i<pixels.Length;i++)pixels[i]=skyColor;
        void Box(int x,int y,int width,int height,Color c) { for(int yy=Mathf.Max(0,y);yy<Mathf.Min(h,y+height);yy++)for(int xx=Mathf.Max(0,x);xx<Mathf.Min(w,x+width);xx++)pixels[yy*w+xx]=c; }
        for(int i=0;i<5;i++) { int x=i*137+10,y=185+(i%3)*52; var c=new Color32(240,248,235,255); Box(x,y,53,8,c);Box(x+8,y+8,35,7,c);Box(x+20,y+15,15,6,c); }
        if(level==1) { Box(465,217,48,48,new Color32(255,228,156,255)); Box(0,0,w,53,new Color32(75,139,147,255)); }
        if(level==2) { Box(480,260,28,28,new Color32(248,238,191,255));for(int i=0;i<30;i++)Box((i*131)%w,165+(i*37)%190,2,2,new Color32(220,225,239,255)); }
        var rng=new System.Random(92+level*19);
        Color[] palette={new Color32(208,193,137,255),new Color32(154,176,154,255),new Color32(221,207,154,255),new Color32(183,192,162,255)};
        if(level==1)palette=new Color[]{new Color32(127,136,119,255),new Color32(154,119,94,255),new Color32(184,150,109,255)};
        if(level==2)palette=new Color[]{new Color32(66,77,105,255),new Color32(83,90,127,255),new Color32(54,65,92,255)};
        for(int layer=0;layer<2;layer++)
        {
            int x=0;
            while(x<w)
            {
                int bw=rng.Next(28,61),bh=rng.Next(38,130); Color c=palette[rng.Next(palette.Length)];
                if(layer==0)c=Color.Lerp(c,skyColor,.28f);
                Box(x,0,bw,bh,c);Box(x-2,bh,bw+4,4,Color.Lerp(c,Color.white,.15f));
                for(int xx=x+5;xx<x+bw-9;xx+=14)for(int yy=7;yy<bh-8;yy+=21) { Box(xx,yy,9,14,Color.Lerp(c,PixelUI.Ink,.15f)); Box(xx+2,yy+2,6,11,level==2?new Color32(233,196,117,255):new Color32(177,206,202,255)); }
                x+=bw+5;
            }
        }
        if(level==1)for(int i=0;i<3;i++) { int x=80+i*215;Color steel=new Color32(98,107,94,255);Box(x,65,7,115,steel);Box(x-38,175,115,6,steel);Box(x+60,125,2,50,steel);Box(x+52,116,18,10,steel); }
        if(level==2)for(int i=0;i<6;i++) { int x=25+i*113;Box(x,90,3,72,new Color32(82,92,123,255));Box(x-12,142,27,3,new Color32(82,92,123,255));Box(x-2,160,6,4,new Color32(235,107,102,255)); }
        var tex=new Texture2D(w,h,TextureFormat.RGB24,false); tex.filterMode=FilterMode.Point; tex.SetPixels(pixels);tex.Apply(false,true);
        artworks[level]=Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(.5f,.5f),32); return artworks[level];
    }
}
