using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class PixelLabel : MaskableGraphic
{
    public float letterHeight = 28;
    public bool outline = true;
    [SerializeField] string caption = "";
    public string Text { get => caption; set { caption = value; SetVerticesDirty(); } }
    static readonly Dictionary<char,string> Glyphs = new Dictionary<char,string>
    {
        {'A',"01110/11011/11011/11111/11011/11011/11011"}, {'B',"11110/11011/11011/11110/11011/11011/11110"},
        {'C',"01111/11000/11000/11000/11000/11000/01111"}, {'D',"11110/11011/11011/11011/11011/11011/11110"},
        {'E',"11111/11000/11000/11110/11000/11000/11111"}, {'F',"11111/11000/11000/11110/11000/11000/11000"},
        {'G',"01111/11000/11000/11011/11011/11011/01111"}, {'H',"11011/11011/11011/11111/11011/11011/11011"},
        {'I',"11111/00100/00100/00100/00100/00100/11111"}, {'J',"00111/00011/00011/00011/11011/11011/01110"},
        {'K',"11011/11011/11110/11100/11110/11011/11011"}, {'L',"11000/11000/11000/11000/11000/11000/11111"},
        {'M',"11011/11111/11111/11011/11011/11011/11011"}, {'N',"11001/11101/11101/11011/11011/11011/11001"},
        {'O',"01110/11011/11011/11011/11011/11011/01110"}, {'P',"11110/11011/11011/11110/11000/11000/11000"},
        {'Q',"01110/11011/11011/11011/11011/01110/00011"}, {'R',"11110/11011/11011/11110/11100/11010/11011"},
        {'S',"01111/11000/11000/01110/00011/00011/11110"}, {'T',"11111/00100/00100/00100/00100/00100/00100"},
        {'U',"11011/11011/11011/11011/11011/11011/01110"}, {'V',"11011/11011/11011/11011/11011/01010/00100"},
        {'W',"11011/11011/11011/11011/11111/11111/01010"}, {'X',"11011/11011/01010/00100/01010/11011/11011"},
        {'Y',"11011/11011/11011/01110/00100/00100/00100"}, {'Z',"11111/00011/00110/01100/11000/11000/11111"},
        {'0',"01110/11011/11011/11011/11011/11011/01110"}, {'1',"00100/01100/00100/00100/00100/00100/01110"},
        {'2',"01110/11011/00011/00110/01100/11000/11111"}, {'3',"11110/00011/00011/01110/00011/00011/11110"},
        {'4',"11011/11011/11011/11111/00011/00011/00011"}, {'5',"11111/11000/11000/11110/00011/00011/11110"},
        {'6',"01111/11000/11000/11110/11011/11011/01110"}, {'7',"11111/00011/00011/00110/00110/01100/01100"},
        {'8',"01110/11011/11011/01110/11011/11011/01110"}, {'9',"01110/11011/11011/01111/00011/00011/11110"},
        {'/',"00001/00011/00010/00100/01000/11000/10000"}, {'-',"00000/00000/00000/11111/00000/00000/00000"},
        {'<',"00011/00110/01100/11000/01100/00110/00011"}, {'>',"11000/01100/00110/00011/00110/01100/11000"},
        {':',"00000/00100/00100/00000/00100/00100/00000"}, {'!',"00100/00100/00100/00100/00100/00000/00100"},
        {'+',"00000/00100/00100/11111/00100/00100/00000"}
    };
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); if(string.IsNullOrEmpty(caption)) return;
        var lines=caption.ToUpperInvariant().Split('\n'); int longest=1;
        foreach(var line in lines) longest=Mathf.Max(longest,line.Length);
        float pixel=Mathf.Min(letterHeight/7f,(rectTransform.rect.width-14)/(longest*6f-1));
        pixel=Mathf.Min(pixel,(rectTransform.rect.height-12)/(lines.Length*9f-2));
        for(int pass=0;pass<(outline?3:1);pass++)
        for(int line=0;line<lines.Length;line++)
        {
            float left=-(lines[line].Length*6-1)*pixel*.5f;
            float top=(lines.Length*9-2)*pixel*.5f-line*9*pixel;
            for(int index=0;index<lines[line].Length;index++)
            {
                if(!Glyphs.TryGetValue(lines[line][index],out string glyph)) continue;
                for(int row=0;row<7;row++) for(int col=0;col<5;col++)
                {
                    if(glyph[row*6+col]!='1') continue;
                    float x=left+(index*6+col)*pixel, y=top-(row+1)*pixel;
                    bool ink=outline&&pass<2;
                    float padding=ink?pixel*(pass==0?.75f:.45f):0;
                    Color c=ink?(pass==0?new Color(.98f,.98f,.92f):PixelUI.Ink):color;
                    if(!ink && row>=5) c=Color.Lerp(c,PixelUI.Ink,.15f);
                    Quad(vh,x-padding,y-padding-(ink?pixel*.25f:0),pixel+padding*2,pixel+padding*2,c);
                }
            }
        }
    }
    static void Quad(VertexHelper vh,float x,float y,float w,float h,Color c)
    {
        int n=vh.currentVertCount; vh.AddVert(new Vector3(x,y),c,Vector2.zero); vh.AddVert(new Vector3(x,y+h),c,Vector2.zero);
        vh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero); vh.AddVert(new Vector3(x+w,y),c,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
    }
}
