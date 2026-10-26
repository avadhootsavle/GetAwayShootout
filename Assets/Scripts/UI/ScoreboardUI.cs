using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    PixelLabel[] labels;
    Image[] portraits;
    public void Init(int[] scores)
    {
        var layout=GetComponent<HorizontalLayoutGroup>(); if(layout!=null) layout.enabled=false;
        foreach(Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        var rt=GetComponent<RectTransform>(); rt.anchoredPosition=new Vector2(0,-44); rt.sizeDelta=new Vector2(0,60);
        labels=new PixelLabel[scores.Length]; portraits=new Image[scores.Length];
        for(int i=0;i<scores.Length;i++)
        {
            float x=scores.Length==2?(i==0?-330:375):new[]{-375f,-240f,270f,405f}[i];
            var icon=PixelUI.Fill("Face "+i,transform,new Vector2(x-30,0),new Vector2(38,44),Color.white);
            portraits[i]=icon;
            icon.sprite=FighterArt.Face(i); icon.preserveAspect=true;
            labels[i]=PixelUI.Label(transform,scores[i].ToString(),new Vector2(x+18,0),new Vector2(50,52),30,i==0?PixelUI.Red:i==1?PixelUI.Blue:i==2?PixelUI.Green:new Color(1,.8f,.2f));
        }
    }
    public void BindPlayers(Player[] players)
    {
        for (int i=0;i<players.Length;i++) portraits[i].sprite=FighterArt.Face(players[i].GetComponent<PlayerBody>().characterIndex);
    }
    public void UpdateScores(int[] scores)
    {
        if(labels==null||labels.Length!=scores.Length) { Init(scores); return; }
        for(int i=0;i<scores.Length;i++) labels[i].Text=scores[i].ToString();
    }
}
