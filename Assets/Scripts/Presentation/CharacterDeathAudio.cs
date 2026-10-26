using UnityEngine;

// Lives with the match manager, so disabling a dead player cannot cut off their voice.
public sealed class CharacterDeathAudio : MonoBehaviour
{
    AudioSource[] voices;
    readonly int[] playCounts=new int[2];
    readonly AudioClip[] lastClips=new AudioClip[2];
    AudioClip kavya, avadhoot;
    bool enabledForMatch;

    public void Configure(int humanPlayers)
    {
        enabledForMatch=humanPlayers==2;
        if(!enabledForMatch)return;
        kavya=Resources.Load<AudioClip>("Audio/DeathVoices/Kavya");
        avadhoot=Resources.Load<AudioClip>("Audio/DeathVoices/Avadhoot");
        voices=new AudioSource[2];
        for(int i=0;i<voices.Length;i++)
        {
            var voice=gameObject.AddComponent<AudioSource>();
            voice.playOnAwake=false; voice.loop=false; voice.spatialBlend=0;
            voice.volume=.85f; voice.pitch=1; voice.dopplerLevel=0;
            voices[i]=voice;
        }
    }

    public void PlayDeath(Player player)
    {
        if(!enabledForMatch || player==null || !player.WasKilledByHit)return;
        int id=player.GetPlayerID();
        if(id<0 || id>=voices.Length)return; // Human slots only; optional bots stay silent.
        int character=player.GetComponent<PlayerBody>().characterIndex;
        AudioClip clip=character==0?kavya:character==4?avadhoot:null;
        if(clip==null)return;
        // A fresh death plays once; simultaneous deaths use independent player voices.
        BackgroundMusic.Duck(clip.length);
        voices[id].PlayOneShot(clip);
        playCounts[id]++; lastClips[id]=clip;
    }
}
