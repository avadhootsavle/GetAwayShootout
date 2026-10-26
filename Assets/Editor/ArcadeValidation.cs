#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Run against a disposable copy: Unity -batchmode -executeMethod ArcadeValidation.Run
[InitializeOnLoad]
public static class ArcadeValidation
{
    static double next;
    static int step;
    static bool failed;
    static ArcadeValidation()
    {
        if (SessionState.GetBool("ArcadeValidation.Running",false))
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += Log;
            next=EditorApplication.timeSinceStartup+3;
        }
    }
    public static void PreviewHome() { SessionState.SetBool("ArcadeValidation.PreviewHome",true);Run(); }
    public static void PreviewVictory()
    {
        SessionState.SetBool("ArcadeValidation.PreviewVictory",true);
        Run();
    }

    public static void RunPresentation() { SessionState.SetBool("ArcadeValidation.Presentation",true);Run(); }
    public static void RunCharacters()
    {
        SessionState.SetBool("ArcadeValidation.Characters",true);
        Run();
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new Exception("Run validation in a disposable batch-mode project copy.");
        var state=new GameState(4) { numHumanPlayers=2, enableBots=false };
        state.ResetMatch(); Require(state.playerOrder.Length==2,"Duel roster");
        state.numHumanPlayers=1; state.enableBots=true; state.ResetPlayerOrder();
        Require(state.playerOrder.Length==4,"Duel to solo roster");
        state.PlayerFinished(0); state.PlayerFinished(0); state.RoundOver(new[]{0,1,2,3});
        Require(state.GetScores()[0]==1,"Duplicate finishes must not award extra points");
        SessionState.SetBool("ArcadeValidation.Running",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        EditorApplication.isPlaying=true;
    }
    static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Log(string message,string stack,LogType type)
    {
        if(stack.Contains("UnityEditor.Search.")) return; // Unity editor indexer, unrelated to game runtime.
        if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert) failed=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
        if (ArcadeSessionUI.InputBlocked) { next=EditorApplication.timeSinceStartup+.25; return; }
        next=EditorApplication.timeSinceStartup+6;
        Debug.Log("ARCADE CHECK " + step);
        try
        {
            var gm=UnityEngine.Object.FindAnyObjectByType<Gamemanager>();
            var select=UnityEngine.Object.FindAnyObjectByType<CharacterSelectUI>();
            switch(step++)
            {
                case 0:
                    Require(select!=null,"Lobby created");
                    Require(Array.Exists(UnityEngine.Object.FindObjectsByType<Camera>(),c=>c.enabled && c.targetDisplay==0),"Menu has active display camera");
                    if(SessionState.GetBool("ArcadeValidation.PreviewVictory",false))
                    {
                        SessionState.SetBool("ArcadeValidation.PreviewVictory",false);
                        select.Hide();
                        UnityEngine.Object.FindAnyObjectByType<VictoryScreenUI>().ShowVictory(0,0,new[]{3,1,2,0});
                        Capture("victory"); Finish(0); return;
                    }
                    Capture("lobby");
                    if(SessionState.GetBool("ArcadeValidation.PreviewHome",false)) { SessionState.SetBool("ArcadeValidation.PreviewHome",false);Finish(0);return; }
                    select.SendMessage("StartMatch"); break;
                case 1:
                    Require(UnityEngine.Object.FindObjectsByType<Player>(FindObjectsInactive.Include).Length==4,"Solo spawns four racers");
                    Require(Time.timeScale==1 && !ArcadeSessionUI.InputBlocked,"Countdown releases play");
                    ValidateWeaponAudio(gm);
                    Capture("race");
                    var ui=UnityEngine.Object.FindAnyObjectByType<ArcadeSessionUI>();
                    ui.SendMessage("TogglePause"); Require(Time.timeScale==0 && AudioListener.pause,"Pause freezes race and audio");
                    ui.SendMessage("TogglePause"); Require(Time.timeScale==1 && !AudioListener.pause,"Resume releases race and audio");
                    if(SessionState.GetBool("ArcadeValidation.Presentation",false)) { Debug.Log(failed?"PRESENTATION VALIDATION FAILED":"PRESENTATION VALIDATION PASSED: menu camera, gun audio, pause/resume");Finish(failed?1:0);return; }
                    gm.ReturnToLobby(); break;
                case 2:
                    select.SendMessage("SetMode",2); select.SendMessage("SetBots",false);
                    if(SessionState.GetBool("ArcadeValidation.Characters",false))
                    {
                        Require(CharacterSelectUI.CharacterNames[0]=="KAVYA" && CharacterSelectUI.CharacterNames[1]=="KRISHNA","Correct existing names");
                        typeof(CharacterSelectUI).GetMethod("Cycle",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(select,new object[]{0,2});
                        Require(select.GetP1Character()==4,"Avadhoot selectable after Krishna");
                        Require(FighterArt.Face(4)!=null && FighterArt.Body(4)!=null && FighterArt.Original(4,"arm")!=null,"Avadhoot art loaded");
                    }
                    Capture("selection"); select.SendMessage("StartMatch"); break;
                case 3:
                    Require(UnityEngine.Object.FindObjectsByType<Player>(FindObjectsInactive.Include).Length==2,"Duel spawns two racers");
                    if(SessionState.GetBool("ArcadeValidation.Characters",false))
                    {
                        Require(gm.GetPlayer(0).GetComponent<PlayerBody>().characterIndex==4,"Avadhoot spawns in game");
                        Require(gm.GetPlayer(0).GetComponentInChildren<SpriteRenderer>()!=null,"Player renderer present");
                    }
                    Capture("two-player");
                    gm.ReturnToLobby(); break;
                case 4:
                    select.SendMessage("SetMode",1); select.SendMessage("StartMatch"); break;
                case 5:
                    Require(UnityEngine.Object.FindObjectsByType<Player>(FindObjectsInactive.Include).Length==4,"Duel to solo transition works");
                    gm.ReturnToLobby(); break;
                case 6:
                    select.SendMessage("SetMode",2); select.SendMessage("SetBots",true); select.SendMessage("StartMatch"); break;
                case 7:
                    Require(UnityEngine.Object.FindObjectsByType<Player>(FindObjectsInactive.Include).Length==4,"Local chaos spawns four racers");
                    gm.GetPlayer(0).GetComponent<PlayerBody>().SetBody(new Vector2(0,-17),0,0); break;
                case 8:
                    Require(gm.GetPlayer(0).gameObject.activeSelf,"Player respawns after falling below the map");
                    CompleteRound(gm); break;
                case 9:
                    Capture("level-2"); CompleteRound(gm); break;
                case 10:
                    Capture("level-3"); CompleteRound(gm); break;
                case 11:
                    Require(GameObject.Find("VictoryPanel")!=null,"Three wins show victory");
                    Capture("victory");
                    UnityEngine.Object.FindAnyObjectByType<VictoryScreenUI>().SendMessage("Rematch"); break;
                case 12:
                    Require(gm.GetPlayer(0).gameObject.activeSelf,"Rematch spawns a new player");
                    Debug.Log(failed?"ARCADE VALIDATION FAILED":"ARCADE VALIDATION PASSED: roster, scoring, solo, duel, chaos, pause, countdown, respawn, three-round victory, rematch");
                    Finish(failed?1:0); break;
            }
        }
        catch(Exception e) { Debug.LogException(e); Finish(1); }
    }
    static void ValidateWeaponAudio(Gamemanager gm)
    {
        foreach(string name in new[]{"Pistol","SMG","Shotgun","Sniper","RPG","TeleportGun","UsableBomb","UsableThrowingKnife"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PowerUps/"+name+".prefab");
            var go=UnityEngine.Object.Instantiate(prefab,gm.GetPlayer(0).transform);go.transform.position=new Vector3(0,1000,0);
            var gun=go.GetComponent<Gun>();gun.SendMessage("Start");
            var clip=WeaponAudio.ClipFor(gun);Require(clip!=null && clip.samples>0,"Sound for "+name);
            int before=WeaponAudio.ShotsPlayed;
            Require(gun.Use(gm.GetPlayer(0)),"Gun fires: "+name);
            Require(WeaponAudio.ShotsPlayed==before+1,"One sound per shot: "+name);
            Require(!gun.Use(gm.GetPlayer(0)) && WeaponAudio.ShotsPlayed==before+1,"No sound on blocked shot: "+name);
            UnityEngine.Object.Destroy(go);
        }
        Debug.Log("WEAPON AUDIO PASSED: all six guns and two throwables emit one sound per successful use");
    }
    static void CompleteRound(Gamemanager gm)
    {
        foreach(var bot in UnityEngine.Object.FindObjectsByType<PlayerComputerController>()) bot.enabled=false;
        UnityEngine.Object.FindAnyObjectByType<Finish>().SendMessage("PlayerEntered",gm.GetPlayer(0));
        gm.SendMessage("RoundOver");
        var state=(GameState)typeof(Gamemanager).GetField("state",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
        Debug.Log("ARCADE ROUND SCORES " + string.Join(",",state.GetActiveScores()));
    }
    static void Capture(string name)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var canvas=UnityEngine.Object.FindAnyObjectByType<Gamemanager>().scoreboardUI.GetComponentInParent<Canvas>();
        const int width=1280,height=800;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
        var pixels=new Color[width*height];
        var oldActive=RenderTexture.active;
        var oldMode=canvas.renderMode; var oldCamera=canvas.worldCamera; var oldSorting=canvas.sortingLayerID; var oldPlane=canvas.planeDistance;
        var cameras=UnityEngine.Object.FindObjectsByType<Camera>();
        canvas.enabled=false;
        foreach(var camera in cameras)
        {
            if(!camera.enabled)continue;
            var rect=camera.rect; int w=Mathf.RoundToInt(width*rect.width),h=Mathf.RoundToInt(height*rect.height);
            if(w==0||h==0)continue;
            var target=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var oldTarget=camera.targetTexture; camera.targetTexture=target;camera.rect=new Rect(0,0,1,1);
            foreach(var sky in camera.GetComponentsInChildren<CitySky>())sky.SendMessage("LateUpdate");
            camera.Render(); RenderTexture.active=target;
            var part=new Texture2D(w,h,TextureFormat.RGBA32,false);part.ReadPixels(new Rect(0,0,w,h),0,0);part.Apply();
            var colors=part.GetPixels();int x=Mathf.RoundToInt(rect.x*width),y=Mathf.RoundToInt(rect.y*height);
            for(int row=0;row<h;row++)for(int col=0;col<w;col++)pixels[(y+row)*width+x+col]=colors[row*w+col];
            camera.targetTexture=oldTarget;camera.rect=rect;
            UnityEngine.Object.Destroy(part);target.Release();UnityEngine.Object.Destroy(target);
        }
        var uiTarget=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
        var preview=new GameObject("UI preview").AddComponent<Camera>();preview.orthographic=true;preview.transform.position=new Vector3(0,0,-100);
        preview.clearFlags=CameraClearFlags.SolidColor;preview.backgroundColor=Color.clear;preview.cullingMask=1<<30;preview.targetTexture=uiTarget;
        var transforms=canvas.GetComponentsInChildren<Transform>(true);var layers=new int[transforms.Length];
        for(int i=0;i<transforms.Length;i++){layers[i]=transforms[i].gameObject.layer;transforms[i].gameObject.layer=30;}
        canvas.enabled=true;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=preview;canvas.planeDistance=1;canvas.sortingLayerID=SortingLayer.NameToID("UI");
        Canvas.ForceUpdateCanvases();preview.Render();RenderTexture.active=uiTarget;
        texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();var ui=texture.GetPixels();
        for(int i=0;i<pixels.Length;i++){pixels[i]=Color.Lerp(pixels[i],ui[i],ui[i].a);pixels[i].a=1;}
        texture.SetPixels(pixels);texture.Apply();
        System.IO.Directory.CreateDirectory("Validation");System.IO.File.WriteAllBytes("Validation/"+name+".png",texture.EncodeToPNG());
        RenderTexture.active=oldActive;canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.sortingLayerID=oldSorting;canvas.planeDistance=oldPlane;
        for(int i=0;i<transforms.Length;i++)transforms[i].gameObject.layer=layers[i];
        preview.targetTexture=null;UnityEngine.Object.Destroy(preview.gameObject);UnityEngine.Object.Destroy(texture);uiTarget.Release();UnityEngine.Object.Destroy(uiTarget);
    }

    static void Finish(int code)
    {
        SessionState.SetBool("ArcadeValidation.Running",false);
        SessionState.SetBool("ArcadeValidation.Characters",false);
        SessionState.SetBool("ArcadeValidation.Presentation",false);
        EditorApplication.update-=Tick;
        EditorApplication.Exit(code);
    }
}
#endif
