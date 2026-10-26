using System;
using UnityEngine;

/// <summary>
/// Class for player identity and core player functions.
/// </summary>
public class Player : MonoBehaviour
{
    public GameObject ragdoll;

    public event Action<Player> onDeathEvent;
    public event Action<Transform> onRagdollCreateEvent;
    public event Action onRespawn;

    public bool WasKilledByHit { get; private set; }

    private int id;
    private PlayerBody body;
    private Health health;

    private void Start()
    {
        body = GetComponent<PlayerBody>();
        health = GetComponent<Health>();
        health.OnDeath += Die;
    }

    public void OnDestroy()
    {
        if (health != null) health.OnDeath -= Die;
    }

    public int GetPlayerID()
    {
        return id;
    }

    public void SetPlayerID(int id)
    {
        this.id = id;
    }

    public void SetCharacter(int characterIndex)
    {
        if (body == null)
            body = GetComponent<PlayerBody>();
        if (body != null)
            body.SetCharacter(characterIndex);
    }

    public void TeleportPlayer(PlayerBody other)
    {
        body.SwapWithOther(other);

        // In future handle other components to be copied here (such as player zipline) by creating an onPlayerTeleport action
    }

    void Die(Health.DamageInfo info)
    {
        WasKilledByHit = info.damage > 0 && info.weapon != null;
        // Game pool object system in future?
        PlayerBody ragdollBody = Instantiate(ragdoll, transform.position, transform.rotation).GetComponent<PlayerBody>();
        ragdollBody.SetFromOther(body);

        Destroy(ragdollBody.gameObject, 15f);

        gameObject.SetActive(false);

        onRagdollCreateEvent?.Invoke(ragdollBody.transform);
        onDeathEvent?.Invoke(this);
    }

    public void Respawn()
    {
        WasKilledByHit = false;
        gameObject.SetActive(true);
        GetComponent<Health>().SetMaxHealth();
        onRespawn?.Invoke();
    }

    public void Deactivate()
    {
        if (!gameObject.activeSelf)
            return;

        PlayerBody ragdollBody = Instantiate(ragdoll, transform.position, transform.rotation).GetComponent<PlayerBody>();
        ragdollBody.SetFromOther(body);

        gameObject.SetActive(false);
        
        onRagdollCreateEvent?.Invoke(ragdollBody.transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Death")
        {
            health.Die(new Health.DamageInfo());
        }
    }
}
