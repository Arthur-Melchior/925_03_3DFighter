using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackPattern", menuName = "Scriptable Objects/AttackPatern")]
public class AttackPattern : ScriptableObject
{
    public List<Attack> attacks;

    public void Attack(int index,Animator animator)
    {
        animator.Play(attacks[index].animation.name);
    }
}
