using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectUI : MonoBehaviour
{
    public static readonly string[] CharacterNames={"KAVYA","KRISHNA","AGENT","BONES","AVADHOOT"};
    static readonly int[] CharacterOrder={0,1,4,2,3};
    public event Action OnMatchStartRequested;
    GameObject rootPanel,home,selection;
    Transform[] stages=new Transform[2];
    PixelLabel[] names=new PixelLabel[2];
    PixelLabel botLabel;
    int selectedMode=1,p1Char,p2Char=1;
    bool botsEnabled=true;
    public void Init(GameState state)
    {
        p1Char=state.p1Character; p2Char=state.p2Character; selectedMode=state.numHumanPlayers; botsEnabled=state.enableBots;
        Build();
    }
    void Build()
    {
        if(rootPanel!=null) Destroy(rootPanel);
        rootPanel=PixelUI.Root("CharacterSelectPanel",transform); ArcadeBackdrop.Add(rootPanel.transform);
        home=PixelUI.Root("Home",rootPanel.transform);
        BuildCollegeHeader();
        PixelUI.Label(home.transform,"GETAWAY\nSHOOTOUT",new Vector2(0,75),new Vector2(650,170),67,PixelUI.Cream);
        FighterArt.Preview(home.transform,0,new Vector2(-350,-90),225,true);
        FighterArt.Preview(home.transform,4,new Vector2(355,-90),225,true);
        PixelUI.Label(home.transform,"KAVYA",new Vector2(-350,-230),new Vector2(260,44),25,PixelUI.Cream);
        PixelUI.Label(home.transform,"AVADHOOT",new Vector2(355,-230),new Vector2(260,44),25,PixelUI.Cream);
        PixelUI.Button(home.transform,"1 PLAYER",new Vector2(0,-85),new Vector2(300,62),PixelUI.Red,()=>SetMode(1));
        PixelUI.Button(home.transform,"2 PLAYERS",new Vector2(0,-180),new Vector2(300,62),PixelUI.Blue,()=>SetMode(2));
        selection=PixelUI.Root("Choose players",rootPanel.transform);
        PixelUI.Label(selection.transform,"CHOOSE YOUR PLAYER",new Vector2(0,244),new Vector2(760,76),44,PixelUI.Cream);
        PixelUI.Button(selection.transform,"<",new Vector2(-468,250),new Vector2(48,48),PixelUI.Red,Back);
        for(int i=0;i<2;i++)
        {
            int player=i; float x=i==0?-245:245;
            PixelUI.Label(selection.transform,"P"+(i+1),new Vector2(x,174),new Vector2(120,45),28,i==0?PixelUI.Red:PixelUI.Blue);
            stages[i]=PixelUI.Rect("Preview",selection.transform,new Vector2(x,10),new Vector2(150,240));
            PixelUI.Fill("Ground",selection.transform,new Vector2(x,-111),new Vector2(230,10),PixelUI.Ink);
            PixelUI.Button(selection.transform,"<",new Vector2(x-122,20),new Vector2(44,50),PixelUI.Cream,()=>Cycle(player,-1));
            PixelUI.Button(selection.transform,">",new Vector2(x+122,20),new Vector2(44,50),PixelUI.Cream,()=>Cycle(player,1));
            names[i]=PixelUI.Label(selection.transform,"",new Vector2(x,-144),new Vector2(340,44),25,PixelUI.Cream);
        }
        var bot=PixelUI.Button(selection.transform,"",new Vector2(0,115),new Vector2(180,40),PixelUI.Green,ToggleBots);
        botLabel=PixelUI.Label(bot.transform,"",Vector2.zero,new Vector2(166,34),17,PixelUI.Ink,false);
        PixelUI.Button(selection.transform,"PLAY",new Vector2(0,-259),new Vector2(240,58),PixelUI.Green,StartMatch);
        selection.SetActive(false); Refresh();
    }
    void BuildCollegeHeader()
    {
        PixelUI.Label(home.transform,"SHRI BHAGUBHAI MAFATLAL\nPOLYTECHNIC AND COLLEGE\nOF ENGINEERING",new Vector2(0,258),new Vector2(710,108),26,PixelUI.Cream);
        foreach(int side in new[]{-1,1})
        {
            var plate=PixelUI.Fill("Logo outline",home.transform,new Vector2(side*455,253),new Vector2(140,140),PixelUI.Ink);
            PixelUI.Fill("Logo paper",plate.transform,Vector2.zero,new Vector2(134,134),PixelUI.Cream);
            var logo=PixelUI.Fill(side<0?"College crest":"SVKM logo",plate.transform,Vector2.zero,side<0?new Vector2(126,126):new Vector2(100,124),Color.white);
            logo.sprite=Resources.Load<Sprite>(side<0?"Branding/CollegeCrest":"Branding/SVKM");logo.preserveAspect=true;
        }
    }
    void Refresh()
    {
        for(int i=0;i<2;i++)
        {
            foreach(Transform child in stages[i]) Destroy(child.gameObject);
            int index=i==0?p1Char:(selectedMode==1?(p1Char==1?2:1):p2Char);
            FighterArt.Preview(stages[i],index,Vector2.zero,230,true);
            names[i].Text=(i==1&&selectedMode==1)?"CPU":CharacterNames[index];
        }
        botLabel.Text=selectedMode==1?"3 BOTS":botsEnabled?"BOTS ON":"BOTS OFF";
    }
    void Update()
    {
        var k=Keyboard.current; if(k==null||rootPanel==null||!rootPanel.activeSelf) return;
        if(k.digit1Key.wasPressedThisFrame) SetMode(1);
        if(k.digit2Key.wasPressedThisFrame) SetMode(2);
        if(k.escapeKey.wasPressedThisFrame) Back();
        if(selection.activeSelf)
        {
            if(k.aKey.wasPressedThisFrame) Cycle(0,-1); if(k.dKey.wasPressedThisFrame) Cycle(0,1);
            if(k.jKey.wasPressedThisFrame) Cycle(1,-1); if(k.lKey.wasPressedThisFrame) Cycle(1,1);
            if(k.bKey.wasPressedThisFrame) ToggleBots();
        }
        if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame) { if(selection.activeSelf) StartMatch(); else SetMode(1); }
    }
    void SetMode(int mode) { if(home.activeSelf && mode==2) botsEnabled=false; selectedMode=mode; home.SetActive(false); selection.SetActive(true); Refresh(); }
    void SetBots(bool enabled) { botsEnabled=enabled; Refresh(); }
    void ToggleBots() { if(selectedMode==2) botsEnabled=!botsEnabled; Refresh(); }
    void Cycle(int player,int delta)
    {
        int current=player==0?p1Char:p2Char;
        int position=Array.IndexOf(CharacterOrder,current);
        int next=CharacterOrder[(position+delta%CharacterOrder.Length+CharacterOrder.Length)%CharacterOrder.Length];
        if(player==0) p1Char=next; else if(selectedMode==2) p2Char=next;
        Refresh();
    }
    void Back() { selection.SetActive(false); home.SetActive(true); }
    void StartMatch() { if(!rootPanel.activeSelf) return; rootPanel.SetActive(false); OnMatchStartRequested?.Invoke(); }
    public int GetSelectedMode()=>selectedMode;
    public bool GetBotsEnabled()=>selectedMode==1||botsEnabled;
    public int GetP1Character()=>p1Char;
    public int GetP2Character()=>p2Char;
    public void Show() { rootPanel.SetActive(true); }
    public void Hide() { if(rootPanel!=null) rootPanel.SetActive(false); }
}
