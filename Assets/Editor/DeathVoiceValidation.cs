#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class DeathVoiceValidation
{
    static double next,deadline;
    static int step;
    static BackgroundMusic originalMusic;
    static bool failed;
    static readonly BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic;
    static DeathVoiceValidation()
    {
        if(!SessionState.GetBool("DeathVoiceValidation.Running",false))return;
        next=EditorApplication.timeSinceStartup+3;deadline=next+90;
        EditorApplication.update+=Tick;
        Application.logMessageReceived+=(message,stack,type)=> {if((type==LogType.Error||type==LogType.Exception)&&!stack.Contains("UnityEditor.Search."))failed=true;};
    }
    public static void Run()
    {
        if(!Application.isBatchMode)throw new Exception("Run in a disposable project copy.");
        SessionState.SetBool("DeathVoiceValidation.Running",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");EditorApplication.isPlaying=true;
    }
    static void Require(bool value,string label){if(!value)throw new Exception(label);}
    static int[] Counts(CharacterDeathAudio audio)=>(int[])typeof(CharacterDeathAudio).GetField("playCounts",fields).GetValue(audio);
    static AudioClip[] Clips(CharacterDeathAudio audio)=>(AudioClip[])typeof(CharacterDeathAudio).GetField("lastClips",fields).GetValue(audio);
    static Weapon testWeapon;
    static void Hit(Player p,float damage)
    {
        if(testWeapon==null)testWeapon=new GameObject("Test weapon").AddComponent<ImpactWeapon>();
        p.GetComponent<Health>().Damage(new Health.DamageInfo(damage,null,testWeapon));
    }
    static void Kill(Player p)=>Hit(p,1000);
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("Death voice validation timeout");Exit(1);return;}
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next||ArcadeSessionUI.InputBlocked)return;
        next=EditorApplication.timeSinceStartup+3;
        try
        {
            var gm=UnityEngine.Object.FindAnyObjectByType<Gamemanager>();var ui=UnityEngine.Object.FindAnyObjectByType<CharacterSelectUI>();
            var music=UnityEngine.Object.FindObjectsByType<BackgroundMusic>();
            Require(music.Length==1,"Exactly one music loop across scenes");
            var musicSource=music[0].GetComponent<AudioSource>();
            Require(musicSource.clip!=null && musicSource.loop && musicSource.isPlaying,"Music loaded and playing");
            Require(musicSource.volume<=.121f,"Music stays quiet");
            int listeners=0;foreach(var listener in UnityEngine.Object.FindObjectsByType<AudioListener>())if(listener.isActiveAndEnabled)listeners++;
            Require(listeners==1,"Exactly one active audio listener in menu and gameplay");
            switch(step++)
            {
                case 0:
                    originalMusic=music[0];
                    ui.SendMessage("SetMode",2);ui.SendMessage("SetBots",true);
                    typeof(CharacterSelectUI).GetMethod("Cycle",fields).Invoke(ui,new object[]{1,1});
                    Require(ui.GetP1Character()==0&&ui.GetP2Character()==4,"Select Kavya and Avadhoot");
                    ui.SendMessage("StartMatch");break;
                case 1:
                    foreach(var bot in UnityEngine.Object.FindObjectsByType<PlayerComputerController>())bot.enabled=false;
                    var a=gm.GetComponent<CharacterDeathAudio>();var counts=Counts(a);
                    Require(counts[0]==0&&counts[1]==0,"No voice on spawn");
                    // A nonfatal hit followed by an actual fall-trigger death must stay silent.
                    Hit(gm.GetPlayer(0),1);
                    Require(counts[0]==0,"Nonlethal hit is silent");
                    foreach(int id in new[]{0,1})
                    {
                        gm.GetPlayer(id).SendMessage("OnTriggerEnter2D",GameObject.Find("Fall recovery").GetComponent<Collider2D>());
                        Require(counts[id]==0&&!gm.GetPlayer(id).WasKilledByHit,"Fall death is silent, even after a prior hit");
                        gm.GetPlayer(id).Respawn();
                    }
                    Kill(gm.GetPlayer(0));Kill(gm.GetPlayer(0));Kill(gm.GetPlayer(1));
                    Require(counts[0]==1&&counts[1]==1,"Exactly once per real death, including simultaneous deaths");
                    Require(Clips(a)[0].name=="Kavya"&&Clips(a)[1].name=="Avadhoot","Correct voice mapping");
                    Require(a.gameObject.activeInHierarchy&&!gm.GetPlayer(0).gameObject.activeSelf,"Audio survives player deactivation");
                    foreach(var clip in Clips(a)){Require(clip.samples>0&&clip.length>.05f,"Imported recording is valid");var data=new float[clip.samples*clip.channels];Require(clip.GetData(data,0)&&Array.Exists(data,x=>Mathf.Abs(x)>.001f),"Recording contains decoded audio");}
                    gm.GetPlayer(2).SetCharacter(0);gm.GetPlayer(3).SetCharacter(4);Kill(gm.GetPlayer(2));Kill(gm.GetPlayer(3));
                    Require(counts[0]==1&&counts[1]==1,"Bots never trigger voices");
                    break;
                case 2:
                    var audio=gm.GetComponent<CharacterDeathAudio>();var c=Counts(audio);
                    Require(gm.GetPlayer(0).gameObject.activeSelf&&gm.GetPlayer(1).gameObject.activeSelf,"Normal respawn still works");
                    Require(c[0]==1&&c[1]==1,"Respawn is silent");
                    gm.GetPlayer(0).SetCharacter(4);gm.GetPlayer(1).SetCharacter(0);Kill(gm.GetPlayer(0));Kill(gm.GetPlayer(1));
                    Require(c[0]==2&&c[1]==2&&Clips(audio)[0].name=="Avadhoot"&&Clips(audio)[1].name=="Kavya","Voice follows character, not player slot");
                    gm.GetPlayer(0).Respawn();gm.GetPlayer(0).SetCharacter(1);Kill(gm.GetPlayer(0));
                    gm.GetPlayer(1).Respawn();gm.GetPlayer(1).SetCharacter(3);Kill(gm.GetPlayer(1));
                    Require(c[0]==2&&c[1]==2,"Other characters silent");
                    var session=UnityEngine.Object.FindAnyObjectByType<ArcadeSessionUI>();session.SendMessage("TogglePause");Require(AudioListener.pause,"Pause applies to voice playback");session.SendMessage("TogglePause");Require(!AudioListener.pause,"Resume restores playback");
                    foreach(var source in audio.GetComponents<AudioSource>())Require(!source.loop&&!source.playOnAwake&&!source.ignoreListenerPause,"Voice source settings");
                    gm.GetPlayer(0).Respawn();gm.GetPlayer(0).SetCharacter(0);gm.SendMessage("RoundOver");
                    Require(c[0]==2&&c[1]==2,"Round completion is silent");
                    gm.ReturnToLobby();break;
                case 3:
                    Require(originalMusic==music[0],"Music continues through menu reload");
                    Require(UnityEngine.Object.FindAnyObjectByType<CharacterDeathAudio>()==null,"No lingering voice host in menu");
                    ui.SendMessage("SetMode",1);ui.SendMessage("StartMatch");break;
                case 4:
                    var solo=gm.GetComponent<CharacterDeathAudio>();
                    gm.GetPlayer(0).SetCharacter(0);Kill(gm.GetPlayer(0));gm.GetPlayer(0).Respawn();gm.GetPlayer(0).SetCharacter(4);Kill(gm.GetPlayer(0));
                    Require(Counts(solo)[0]==0&&Counts(solo)[1]==0,"Single-player has no death recordings");
                    Debug.Log(failed?"DEATH VOICE VALIDATION FAILED":"DEATH VOICE VALIDATION PASSED: music loop, scene persistence, listener switching, lethal hits only, falls silent, nonlethal hits silent, names, both slots, duplicate death, simultaneous deaths, bots excluded, other characters excluded, respawn, pause, round end, menu cleanup, single-player excluded");Exit(failed?1:0);break;
            }
        }
        catch(Exception e){Debug.LogException(e);Exit(1);}
    }
    static void Exit(int code){SessionState.SetBool("DeathVoiceValidation.Running",false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
#endif
