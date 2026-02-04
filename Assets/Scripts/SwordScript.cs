using System;
using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            var x = other.gameObject.GetComponentInParent<EnemyScript>();
            if (x)
            {
                x.Die();
            }
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            var x = other.gameObject.GetComponentInParent<PlayerScript>();
            if (x)
            {
                x.Die();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            var x = other.gameObject.GetComponentInParent<EnemyScript>();
            if (x)
            {
                x.Die();
            }
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            var x = other.gameObject.GetComponentInParent<PlayerScript>();
            if (x)
            {
                x.Die();
            }
        }
    }
}