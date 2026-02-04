using System;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Die()
    {
        _animator.enabled = false;
    }
        
}