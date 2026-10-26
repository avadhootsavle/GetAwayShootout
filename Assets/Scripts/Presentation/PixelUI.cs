using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class PixelUI
{
    public static readonly Color Ink=new Color32(28,36,42,255);
    public static readonly Color Red=new Color32(237,83,76,255);
    public static readonly Color Blue=new Color32(77,116,230,255);
    public static readonly Color Green=new Color32(114,204,66,255);
    public static readonly Color Cream=new Color32(250,246,217,255);
    public static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform; rt.sizeDelta=size; rt.anchoredPosition=position; return rt;
    }
    public static GameObject Root(string name,Transform parent)
    {
        var rt=Rect(name,parent,Vector2.zero,Vector2.zero); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
        return rt.gameObject;
    }
    public static Image Fill(string name,Transform parent,Vector2 position,Vector2 size,Color color)
    {
        var rt=Rect(name,parent,position,size); var image=rt.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return image;
    }
    public static PixelLabel Label(Transform parent,string text,Vector2 position,Vector2 size,float height,Color color,bool outline=true)
    {
        var rt=Rect(text,parent,position,size); var label=rt.gameObject.AddComponent<PixelLabel>(); label.letterHeight=height; label.color=color; label.outline=outline; label.Text=text; label.raycastTarget=false; return label;
    }
    public static Button Button(Transform parent,string text,Vector2 position,Vector2 size,Color color,UnityEngine.Events.UnityAction action)
    {
        var rt=Rect(text,parent,position,size);
        Fill("Shadow",rt,new Vector2(0,-6),size+Vector2.one*10,Ink);
        Fill("Paper edge",rt,Vector2.zero,size+Vector2.one*10,Cream);
        Fill("Ink",rt,Vector2.zero,size+Vector2.one*4,Ink);
        var face=Fill("Face",rt,Vector2.zero,size,color); face.raycastTarget=true;
        Fill("Highlight",rt,new Vector2(0,size.y*.5f-3),new Vector2(size.x,6),Color.Lerp(color,Color.white,.28f));
        Fill("Foot",rt,new Vector2(0,-size.y*.5f+4),new Vector2(size.x,8),Color.Lerp(color,Ink,.22f));
        Label(rt,text,Vector2.zero,size-new Vector2(16,6),Mathf.Min(28,size.y*.44f),color.grayscale>.75f?Ink:Cream,false);
        var button=rt.gameObject.AddComponent<Button>(); button.targetGraphic=face; button.onClick.AddListener(action);
        var colors=button.colors; colors.highlightedColor=new Color(1.15f,1.15f,1.15f); colors.pressedColor=new Color(.8f,.8f,.8f); colors.fadeDuration=.08f; button.colors=colors;
        rt.gameObject.AddComponent<PixelButtonMotion>(); return button;
    }
}
public class PixelButtonMotion : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    Vector3 target=Vector3.one;
    public void OnPointerDown(PointerEventData e) { target=Vector3.one*.96f; }
    public void OnPointerUp(PointerEventData e) { target=Vector3.one; }
    public void OnPointerExit(PointerEventData e) { target=Vector3.one; }
    void OnDisable() { transform.localScale=target=Vector3.one; }
    void Update() { transform.localScale=Vector3.Lerp(transform.localScale,target,1-Mathf.Exp(-22*Time.unscaledDeltaTime)); }
}
