using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ArcadeSessionUI : MonoBehaviour
{
    public static bool InputBlocked { get; private set; }
    GameObject hud,overlay,shade,actions;
    PixelLabel announcement;
    RectTransform[] markers;
    Gamemanager manager;
    WaypointManager route;
    bool started,counting,paused,ended;
    public void Init(Gamemanager owner,Canvas canvas)
    {
        manager=owner;
        var scaler=canvas.GetComponent<CanvasScaler>();
        if(scaler!=null) { scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1100,680); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand; }
        InputBlocked=false;
        hud=PixelUI.Root("Race HUD",canvas.transform);
        var pause=PixelUI.Button(hud.transform,"",new Vector2(-500,294),new Vector2(38,42),PixelUI.Cream,TogglePause);
        PixelUI.Fill("Left bar",pause.transform,new Vector2(-6,0),new Vector2(7,23),PixelUI.Ink);
        PixelUI.Fill("Right bar",pause.transform,new Vector2(6,0),new Vector2(7,23),PixelUI.Ink);
        PixelUI.Fill("Progress ink",hud.transform,new Vector2(0,294),new Vector2(282,12),PixelUI.Ink);
        PixelUI.Fill("Progress track",hud.transform,new Vector2(0,294),new Vector2(274,4),PixelUI.Cream);
        markers=new RectTransform[2];
        for(int i=0;i<2;i++) markers[i]=PixelUI.Fill("P"+(i+1),hud.transform,new Vector2(-135,294),new Vector2(12,20),i==0?PixelUI.Red:PixelUI.Blue).rectTransform;
        overlay=PixelUI.Root("Race overlay",canvas.transform);
        shade=PixelUI.Root("Scrim",overlay.transform); shade.AddComponent<Image>().color=new Color(.07f,.16f,.18f,.7f);
        announcement=PixelUI.Label(overlay.transform,"",new Vector2(0,70),new Vector2(780,150),90,PixelUI.Cream);
        actions=PixelUI.Root("Actions",overlay.transform);
        PixelUI.Button(actions.transform,"RESUME",new Vector2(0,-62),new Vector2(300,62),PixelUI.Green,TogglePause);
        PixelUI.Button(actions.transform,"MENU",new Vector2(0,-158),new Vector2(300,62),PixelUI.Blue,()=>manager.ReturnToLobby());
        hud.SetActive(false); overlay.SetActive(false);
    }
    public void BeginRound()
    {
        started=true; route=FindAnyObjectByType<WaypointManager>(); hud.SetActive(true);
        markers[1].gameObject.SetActive(manager.numHumanPlayers==2); StartCoroutine(Countdown());
    }
    IEnumerator Countdown()
    {
        counting=true; InputBlocked=true; Time.timeScale=0;
        overlay.SetActive(true); shade.SetActive(false); actions.SetActive(false);
        for(int i=3;i>0;i--) { announcement.Text=i.ToString(); yield return new WaitForSecondsRealtime(.55f); }
        announcement.Text="GO!"; yield return new WaitForSecondsRealtime(.25f);
        overlay.SetActive(false); Time.timeScale=1; InputBlocked=false; counting=false;
    }
    public void FinishRound() { ended=true; hud.SetActive(false); }
    void Update()
    {
        if(!started||ended)return;
        if(!counting&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
        if(route!=null) for(int i=0;i<manager.numHumanPlayers;i++)
            markers[i].anchoredPosition=new Vector2(Mathf.Lerp(-135,135,route.GetProgress(manager.GetPlayer(i).transform)),294);
    }
    void TogglePause()
    {
        if(!started||counting||ended)return;
        paused=!paused; AudioListener.pause=paused; InputBlocked=paused; Time.timeScale=paused?0:1;
        foreach(var p in FindObjectsByType<PlayerHumanController>()) p.ClearHeldInput();
        overlay.SetActive(paused); shade.SetActive(true); actions.SetActive(true); announcement.letterHeight=60; announcement.Text="PAUSED";
    }
    void OnDestroy() { AudioListener.pause=false; Time.timeScale=1; InputBlocked=false; }
}
