using UnityEngine;

[System.Serializable]
public class Attack
{
    public AnimationClip animation;
    public AttackType attackType;
}

public enum AttackType
{
    Light,
    Heavy,
    Special
}
