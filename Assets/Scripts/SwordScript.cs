using System;
using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ennemy")) return;

        var x = other.GetComponentInParent<CombatScript>();
        x.Die();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Ennemy")) return;

        var x = other.gameObject.GetComponentInParent<CombatScript>();
        x.Die();
    }
}
