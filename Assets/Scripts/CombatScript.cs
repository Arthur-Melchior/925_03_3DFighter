using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CombatScript : MonoBehaviour
{
    private static readonly int LightAttack = Animator.StringToHash("LightAttack");
    private static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");
    private Animator _animator;
    private bool _attackBuffered;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void LAttack()
    {
        _animator.SetBool(LightAttack, true);
    }

    public void HAttack()
    {
        _animator.SetBool(HeavyAttack, true);
    }

    public void SetLightAttack(string value)
    {
        var isTrue = value.ToLower() == "true";
        _animator.SetBool(LightAttack, isTrue);
    }

    public void SetHeavyAttack(string value)
    {
        var isTrue = value.ToLower() == "true";
        _animator.SetBool(HeavyAttack, isTrue);
    }

    public void Die()
    {
        _animator.enabled = false;
    }
}