using System;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponentInParent<EnemyScript>();
        if (enemy) enemy.GetParried();
    }
}