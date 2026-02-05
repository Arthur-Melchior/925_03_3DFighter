using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float agroRadius = 10f;
    public float attackRange = 1f;
    public float speed = 5f;
    public float attackRecovery = 2f;
    public CapsuleCollider swordTrigger;
    public PlayerScript player;
    private Animator _animator;
    private float _attackReload = 2f;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Die()
    {
        DisableSword();
        _animator.enabled = false;
    }

    private void Update()
    {
        if (!_animator.enabled)
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
            transform.position += distance.normalized * (speed * Time.deltaTime);
            transform.LookAt(player.transform);
        }
    }

    public void GetParried()
    {
        _animator.SetTrigger("Parried");
        swordTrigger.enabled = false;
    }

    public void DisableSword()
    {
        swordTrigger.enabled = false;
    }

    public void EnableSword()
    {
        swordTrigger.enabled = true;
    }
}