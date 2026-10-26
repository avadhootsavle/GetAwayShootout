using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VictoryScreenUI : MonoBehaviour
{
    public event Action OnRematchRequested;
    public event Action OnChangeCharactersRequested;
    GameObject panel;
    bool showing;
    AudioSource victorySound;
    public void ShowVictory(int winnerId,int characterIndex,int[] scores)
    {
        if(showing)return; // A match result must never replay its sound.
        if(panel!=null) Destroy(panel);
        panel=PixelUI.Root("VictoryPanel",transform); ArcadeBackdrop.Add(panel.transform);
        PixelUI.Label(panel.transform,"P"+(winnerId+1)+" WINS!",new Vector2(0,218),new Vector2(740,110),68,PixelUI.Cream);
        FighterArt.Preview(panel.transform,characterIndex,new Vector2(0,22),245,true);
        var emblem=PixelUI.Fill("Victory emblem",panel.transform,new Vector2(0,152),new Vector2(96,46),Color.white);
        emblem.sprite=Resources.Load<Sprite>("Branding/VictoryEmblem");emblem.preserveAspect=true;
        float spacing=150;
        for(int i=0;i<scores.Length;i++)
            PixelUI.Label(panel.transform,"P"+(i+1)+"  "+scores[i],new Vector2((i-(scores.Length-1)*.5f)*spacing,-141),new Vector2(134,54),24,i==winnerId?PixelUI.Green:PixelUI.Cream);
        PixelUI.Button(panel.transform,"AGAIN",new Vector2(-150,-237),new Vector2(230,60),PixelUI.Green,Rematch);
        PixelUI.Button(panel.transform,"MENU",new Vector2(150,-237),new Vector2(230,60),PixelUI.Blue,ChangeCharacters);
        showing=true;
        if(victorySound==null)
        {
            victorySound=gameObject.AddComponent<AudioSource>();
            victorySound.playOnAwake=false;victorySound.loop=false;victorySound.spatialBlend=0;victorySound.volume=.85f;
            victorySound.clip=Resources.Load<AudioClip>("Audio/Win");
        }
        if(victorySound.clip!=null) { BackgroundMusic.Duck(victorySound.clip.length);victorySound.Play(); }
    }
    void Update()
    {
        var k=Keyboard.current; if(!showing||k==null) return;
        if(k.spaceKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame) Rematch();
        else if(k.escapeKey.wasPressedThisFrame||k.cKey.wasPressedThisFrame) ChangeCharacters();
    }
    void Rematch() { if(!showing)return; showing=false; if(victorySound!=null)victorySound.Stop(); panel.SetActive(false); OnRematchRequested?.Invoke(); }
    void ChangeCharacters() { if(!showing)return; showing=false; if(victorySound!=null)victorySound.Stop(); panel.SetActive(false); OnChangeCharactersRequested?.Invoke(); }
}
