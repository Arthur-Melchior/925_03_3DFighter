using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(MultiParentConstraint))]
public class SwordScript : MonoBehaviour
{
    private MultiParentConstraint _mpc;
    public RigBuilder RigBuilder;

    private void Start()
    {
        _mpc = GetComponent<MultiParentConstraint>();
    }

    public void EnterCombat()
    {
        transform.rotation = new Quaternion(0.533681035f, 0.377067357f, 0.674525142f, 0.343541384f);
        _mpc.data.sourceObjects.SetWeight(0,0.9f);
        RigBuilder.Clear();
        RigBuilder.Build();
    }

    public void ExitCombat()
    {
        transform.rotation = new Quaternion(-0.751786888f, 0.260531098f, -0.596502125f, 0.105476134f);
    }

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