using UnityEngine;

// Scene-owned voices let the final shot ring out after an empty gun is removed.
public class WeaponAudio : MonoBehaviour
{
    static WeaponAudio instance;
    readonly AudioSource[] voices=new AudioSource[12];
    int next;
    public static int ShotsPlayed { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() { instance=null; ShotsPlayed=0; }
    public static string SoundName(Gun gun)
    {
        string name=gun.name.Replace("(Clone)","");
        switch(name) { case "Pistol": case "SMG": case "Shotgun": case "Sniper": case "RPG": case "TeleportGun": return name; default: return gun is ProjectileGun?"Throw":"Pistol"; }
    }
    public static AudioClip ClipFor(Gun gun)=>Resources.Load<AudioClip>("Audio/"+SoundName(gun));
    public static void Fire(Gun gun) { if(Play(ClipFor(gun),.52f))ShotsPlayed++; }
    public static void Reload()=>Play(Resources.Load<AudioClip>("Audio/Reload"),.28f);
    static bool Play(AudioClip clip,float volume)
    {
        if(clip==null)return false;
        if(instance==null)instance=new GameObject("Weapon audio").AddComponent<WeaponAudio>();
        var source=instance.voices[instance.next];instance.next=(instance.next+1)%instance.voices.Length;
        source.Stop();source.clip=clip;source.volume=volume;source.pitch=1;source.Play();return true;
    }
    void Awake()
    {
        for(int i=0;i<voices.Length;i++) { var source=gameObject.AddComponent<AudioSource>(); source.playOnAwake=false;source.spatialBlend=0;source.dopplerLevel=0;voices[i]=source; }
    }
}
