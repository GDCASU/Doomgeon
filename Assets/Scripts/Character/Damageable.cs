/*
 * Author: Cristion Dominguez
 * Date: 19 Jan. 2023
 */

using System;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public event Action<int> DamageReceived;
    public event Action<int> HealingReceived;
    public event Action Died;

    [SerializeField] StaticResource _health;
    [SerializeField] CharacterAllegiance _personalAllegiance;
    [SerializeField] CharacterAllegiance _opponentAllegiance;
    [SerializeField] Transform[] _targetPoints;
    [SerializeField] IntEventChannelSO _healthUpdateEvent = default;
    [SerializeField] VoidEventChannelSO _deathEvent = default;

    public int CurrentHealth => _health.Current;
    public CharacterAllegiance PersonalAllegiance
    {
        get => _personalAllegiance;
        set
        {
            _personalAllegiance = value;
            foreach (Hurtbox box in Hurtboxes)
                box.gameObject.layer = value.GetLayer();
        }
    }
    public CharacterAllegiance OpponentAllegiance => _opponentAllegiance;
    public bool IsDead => _health.Current <= 0;
    public Transform[] TargetPoints => _targetPoints;
    public Hurtbox[] Hurtboxes { get; private set; }

    void Awake()
    {
        Hurtboxes = GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox box in Hurtboxes)
            box.gameObject.layer = PersonalAllegiance.GetLayer();
    }

    public void ReceiveDamage(int damage)
    {
        if (IsDead)
            return;

        _health.Drain(damage);

        if (_health.Current <= 0)
        {
            Died?.Invoke();
            _deathEvent?.RaiseEvent();
        }

        DamageReceived?.Invoke(damage);
        _healthUpdateEvent?.RaiseEvent(_health.Current);
    }

    public void ReceiveHealing(int healing)
    {
        _health.Fill(healing);

        HealingReceived?.Invoke(healing);
        _healthUpdateEvent?.RaiseEvent(_health.Current);
    }
}
