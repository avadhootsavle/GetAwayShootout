using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class HealthEvents : MonoBehaviour
{
    public UnityEvent<Health.DamageInfo> OnDamage;
    public UnityEvent<Health.DamageInfo> OnDeath;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        health.OnDamage += OnDamage.Invoke;
        health.OnDeath += OnDeath.Invoke;
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.OnDamage -= OnDamage.Invoke;
        health.OnDeath -= OnDeath.Invoke;
    }
}
