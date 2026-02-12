using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyScript : MonoBehaviour
{
    public float attackRange = 1f;
    public float speed = 5f;
    public float attackRecovery = 2f;
    public CapsuleCollider swordTrigger;
    public PlayerScript player;
    public UnityEvent onDie;
    public ParticleSystem deathVFX;
    private Animator _animator;
    private float _attackReload = 2f;
    private bool _isAttacking;
    private NavMeshAgent _navMeshAgent;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Die()
    {
        if (!_animator.enabled) return;
        DisableSword();
        onDie?.Invoke();
        _animator.enabled = false;
        deathVFX.Play();
        _navMeshAgent.enabled = false;
        enabled = false;
    }

    private void Update()
    {
        if (!_animator.enabled || _isAttacking)
        {
            return;
        }

        _attackReload += Time.deltaTime;

        var distance = player.transform.position - transform.position;
        if (distance.magnitude < attackRange && _attackReload > attackRecovery)
        {
            _attackReload = 0f;
            _animator.SetTrigger("Attack");
        }
        else if (distance.magnitude > attackRange)
        {
            //transform.position += distance.normalized * (speed * Time.deltaTime);
            _navMeshAgent.SetDestination(player.transform.position);
            transform.LookAt(player.transform);
        }
    }

    public void GetParried()
    {
        _animator.SetTrigger("Parried");
        DisableSword();
    }

    public void DisableSword()
    {
        swordTrigger.enabled = false;
        _isAttacking = false;
    }

    public void EnableSword()
    {
        _isAttacking = true;
        swordTrigger.enabled = true;
    }
}