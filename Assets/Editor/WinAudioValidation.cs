#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class WinAudioValidation
{
    static double next,deadline;
    static int step;
    static bool failed;
    static WinAudioValidation()
    {
        if(!SessionState.GetBool("WinAudioValidation.Running",false))return;
        next=EditorApplication.timeSinceStartup+3;deadline=next+90;
        EditorApplication.update+=Tick;
        Application.logMessageReceived+=(m,s,t)=>{if((t==LogType.Error||t==LogType.Exception)&&!s.Contains("UnityEditor.Search."))failed=true;};
    }
    public static void Run()
    {
        if(!Application.isBatchMode)throw new Exception("Use a disposable copy");
        SessionState.SetBool("WinAudioValidation.Running",true);EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");EditorApplication.isPlaying=true;
    }
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static AudioSource Sound(VictoryScreenUI ui)=>(AudioSource)typeof(VictoryScreenUI).GetField("victorySound",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("Win audio timeout");Exit(1);return;}
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next||ArcadeSessionUI.InputBlocked)return;
        next=EditorApplication.timeSinceStartup+4;
        try
        {
            var gm=UnityEngine.Object.FindAnyObjectByType<Gamemanager>();var victory=UnityEngine.Object.FindAnyObjectByType<VictoryScreenUI>();
            switch(step++)
            {
                case 0:
                    Require(Sound(victory)==null,"Menu is silent for victory audio");
                    var select=UnityEngine.Object.FindAnyObjectByType<CharacterSelectUI>();select.SendMessage("SetMode",2);select.SendMessage("SetBots",false);select.SendMessage("StartMatch");break;
                case 1: case 2: case 3:
                    Require(Sound(victory)==null,"No win recording during individual stages");
                    UnityEngine.Object.FindAnyObjectByType<Finish>().SendMessage("PlayerEntered",gm.GetPlayer(0));gm.SendMessage("RoundOver");
                    if(step==4)next=EditorApplication.timeSinceStartup+2;
                    break;
                case 4:
                    var sound=Sound(victory);Require(sound!=null&&sound.clip.name=="Win"&&sound.isPlaying&&!sound.loop,"Final winner plays nonlooping Win recording");
                    float time=sound.time;var panel=GameObject.Find("VictoryPanel");
                    victory.ShowVictory(0,0,new[]{3,0});Require(GameObject.Find("VictoryPanel")==panel&&sound.time>=time,"Repeated result cannot restart recording");
                    next=EditorApplication.timeSinceStartup+sound.clip.length+1;break;
                case 5:
                    Require(!Sound(victory).isPlaying,"Recording stops naturally after one play");victory.SendMessage("Rematch");break;
                case 6:
                    Require(Sound(victory)==null,"New match does not start victory sound");
                    Debug.Log(failed?"WIN AUDIO VALIDATION FAILED":"WIN AUDIO VALIDATION PASSED: stages silent, final victory only, duplicate result ignored, stops after one play, rematch silent");Exit(failed?1:0);break;
            }
        }
        catch(Exception e){Debug.LogException(e);Exit(1);}
    }
    static void Exit(int code){SessionState.SetBool("WinAudioValidation.Running",false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
#endif
