#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[InitializeOnLoad]
public static class TraversalValidation
{
    static double deadline,ready,report;
    static bool launched,started,failed;
    static Keyboard keyboard;
    static Key[] queuedKeys=Array.Empty<Key>();
    static bool[] holding=new bool[2];
    static float[] desired=new float[2],lastJump=new float[2];
    static int[] target=new int[2];
    static int[] jumps=new int[2];
    static bool[] finished=new bool[2];
    static Vector2[] route={new Vector2(.3f,-.5f),new Vector2(5.2f,1),new Vector2(11.6f,1),new Vector2(17.6f,2),new Vector2(19.7f,2),new Vector2(25.3f,3.5f),new Vector2(31.8f,3.5f),new Vector2(38.3f,3.5f),new Vector2(41.7f,3.5f),new Vector2(46,5)};
    static TraversalValidation()
    {
        if(!SessionState.GetBool("Traversal.Running",false))return;
        ready=EditorApplication.timeSinceStartup+3; deadline=ready+160;
        EditorApplication.update+=Tick;
        Application.logMessageReceived+=(message,stack,type)=> { if((type==LogType.Exception||type==LogType.Error)&&!stack.Contains("UnityEditor.Search."))failed=true; };
    }
    public static void RunThird() { SessionState.SetInt("Traversal.Map",2); Run(); }
    public static void Run()
    {
        if(!Application.isBatchMode)throw new Exception("Use a disposable batch project");
        SessionState.SetBool("Traversal.Running",true); EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<ready)return;
        if(EditorApplication.timeSinceStartup>deadline) { Debug.LogError("TRAVERSAL TIMEOUT"); Exit(1);return; }
        try
        {
            var gm=UnityEngine.Object.FindAnyObjectByType<Gamemanager>();
            if(!launched)
            {
                InputSystem.settings=UnityEngine.Object.Instantiate(InputSystem.settings);
                InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                Application.runInBackground=true;
                keyboard=InputSystem.AddDevice<Keyboard>();
                InputSystem.onBeforeUpdate+=()=> { if(InputState.currentUpdateType==InputUpdateType.Dynamic) InputSystem.QueueStateEvent(keyboard,new KeyboardState(queuedKeys)); };
                var state=(GameState)typeof(Gamemanager).GetField("state",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null); state.curMap=SessionState.GetInt("Traversal.Map",1);
                if(state.curMap==2)route=new[]{new Vector2(2,2),new Vector2(8,2),new Vector2(13.5f,1),new Vector2(17.5f,1),new Vector2(21.5f,2.66f),new Vector2(27,-.5f),new Vector2(33,-.5f),new Vector2(39,-.5f),new Vector2(41.5f,-.5f),new Vector2(44.7f,.75f),new Vector2(48,2)};
                var ui=UnityEngine.Object.FindAnyObjectByType<CharacterSelectUI>();ui.SendMessage("SetMode",2);ui.SendMessage("SetBots",false);ui.SendMessage("StartMatch");
                keyboard=(Keyboard)PlayerInput.GetPlayerByIndex(0).devices[0];
                Debug.Log("PAIRED KEYBOARD " + keyboard.deviceId + " P1 " + PlayerInput.GetPlayerByIndex(0).currentActionMap.enabled + " P2 " + PlayerInput.GetPlayerByIndex(1).devices[0].deviceId);
                launched=true;ready=EditorApplication.timeSinceStartup+3;return;
            }
            if(ArcadeSessionUI.InputBlocked)return;
            if(EditorApplication.timeSinceStartup>report)
            {
                report=EditorApplication.timeSinceStartup+4;
                for(int j=0;j<2;j++) { var pl=gm.GetPlayer(j);Debug.Log($"TRACE P{j} pos={pl.transform.position} ground={pl.GetComponent<PlayerMovement>().IsGrounded()} rot={pl.GetComponent<PlayerBody>().GetRotation()} held={holding[j]} keyboard={keyboard.eKey.isPressed}/{keyboard.oKey.isPressed} enabled={pl.GetComponent<PlayerHumanController>().enabled} action={PlayerInput.GetPlayerByIndex(j).currentActionMap.FindAction("MoveRight").ReadValue<float>()} controller={typeof(PlayerHumanController).GetField("isRightDown",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pl.GetComponent<PlayerHumanController>())}"); }
            }
            if(!started)
            {
                started=true; Time.timeScale=2;
                // Keep the finish trigger active for both players, without ending the round after the first arrival.
                var finish=UnityEngine.Object.FindAnyObjectByType<Finish>(); finish.finishTime=180;
                finish.OnPlayerEntered+=(p,first)=> { finished[p.GetPlayerID()]=true; Debug.Log("PLAYER "+p.GetPlayerID()+" CROSSED LEVEL "+(SessionState.GetInt("Traversal.Map",1)+1)+" after "+jumps[p.GetPlayerID()]+" keyboard jumps"); };
            }
            var keys=new List<Key>();
            for(int i=0;i<2;i++)
            {
                var p=gm.GetPlayer(i);if(finished[i])continue;
                if(!p.gameObject.activeSelf)throw new Exception("Player "+i+" died during traversal at target "+target[i]);
                var movement=p.GetComponent<PlayerMovement>(); float x=p.transform.position.x,y=p.transform.position.y;
                if(holding[i])
                {
                    if(-p.GetComponent<PlayerBody>().GetRotation()>=desired[i])
                    {
                        holding[i]=false;lastJump[i]=Time.time;jumps[i]++;
                        Debug.Log($"JUMP P{i} from {p.transform.position} angle {desired[i]:F1} to {route[target[i]]}");
                    }
                }
                else if(Time.time-lastJump[i]>.32f && movement.IsGrounded())
                {
                    while(target[i]<route.Length-1&&x>route[target[i]].x-1.0f)target[i]++;
                    Vector2 destination=route[target[i]] + Vector2.right * i * .65f;
                    float dx=Mathf.Max(.8f,destination.x-x),dy=destination.y-y;
                    float v=movement.jumpForce,g=-Physics2D.gravity.y*p.GetComponent<Rigidbody2D>().gravityScale;
                    float disc=v*v*v*v-g*(g*dx*dx+2*dy*v*v);
                    if(disc<0)throw new Exception($"Unreachable target P{i}: from {p.transform.position} to {destination}");
                    desired[i]=Mathf.Clamp(90-Mathf.Atan((v*v+Mathf.Sqrt(disc))/(g*dx))*Mathf.Rad2Deg,6,48);
                    holding[i]=true;
                }
                if(holding[i])keys.Add(i==0?Key.E:Key.O);
            }
            queuedKeys=keys.ToArray();
            if(finished[0]&&finished[1]) { Debug.Log(failed?"TRAVERSAL FAILED":"TRAVERSAL PASSED: BOTH PLAYERS CROSSED LEVEL "+(SessionState.GetInt("Traversal.Map",1)+1)+" USING KEYBOARD INPUT; NO TELEPORTS");Exit(failed?1:0); }
        }
        catch(Exception e) { Debug.LogException(e); Exit(1); }
    }
    static void Exit(int code) { SessionState.SetBool("Traversal.Running",false);SessionState.EraseInt("Traversal.Map");EditorApplication.update-=Tick;EditorApplication.Exit(code); }
}
#endif
