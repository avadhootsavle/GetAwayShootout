using UnityEngine;

// One quiet loop shared across menus, rounds, and rematches.
public sealed class BackgroundMusic : MonoBehaviour
{
    static BackgroundMusic instance;
    AudioSource source;
    float duckUntil;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState(){instance=null;}
    public static void EnsurePlaying()
    {
        if(instance!=null)return;
        new GameObject("Background music").AddComponent<BackgroundMusic>();
    }
    public static void Duck(float duration)
    {
        if(instance!=null)instance.duckUntil=Mathf.Max(instance.duckUntil,Time.time+duration);
    }
    void Awake()
    {
        if(instance!=null){Destroy(gameObject);return;}
        instance=this;DontDestroyOnLoad(gameObject);
        source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;
        source.spatialBlend=0;source.volume=0;source.priority=200;
        source.clip=Resources.Load<AudioClip>("Audio/BackgroundMusic");
        if(source.clip!=null)source.Play();
    }
    void Update()
    {
        float target=Time.time<duckUntil?.035f:.12f;
        source.volume=Mathf.MoveTowards(source.volume,target,Time.unscaledDeltaTime*.2f);
    }
}
