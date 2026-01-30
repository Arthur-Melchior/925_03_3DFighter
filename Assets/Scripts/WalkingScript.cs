using System;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class WalkingScript : MonoBehaviour
{
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Strife = Animator.StringToHash("Strife");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Jumped = Animator.StringToHash("Jumped");
    private static readonly int InCombatAnimation = Animator.StringToHash("InCombat");

    public float speed = 20f;
    public float jumpHeight = 2f;
    public float coyoteTime = 0.2f;
    public bool inCombat;
    public CinemachineCamera cinemachineCamera;

    private Animator _animator;
    private CharacterController _cc;
    private Vector3 _velocity;
    private Camera _camera;
    private bool _grounded = true;
    private bool _jumped;
    private bool _moveCanceled;

    private Collider[] _hits;
    private float _aggroAreaRadius;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (!_cc.isGrounded)
            _velocity.y += Physics.gravity.y * Time.deltaTime;

        var move = new Vector3(
            _velocity.z * _camera.transform.forward.x + _velocity.x * _camera.transform.right.x,
            _velocity.y,
            _velocity.z * _camera.transform.forward.z + _velocity.x * _camera.transform.right.z) * Time.deltaTime;
        _cc.Move(move);

        //for coyote time
        //checks if in the air and hasn't jumped
        if (_grounded && !_cc.isGrounded && !_jumped)
        {
            StartCoroutine(StartCoyoteTime(coyoteTime));
        }
        else if (!_grounded && _cc.isGrounded)
        {
            StopAllCoroutines();
            _grounded = true;
            _jumped = false;
            _velocity.y = 0;
        }

        if (!_moveCanceled && !inCombat)
            transform.forward = new Vector3(move.x, 0, move.z);
        else if (inCombat)
            cinemachineCamera.Target.LookAtTarget = FindClosestEnemy().transform;


        _animator.SetBool(Grounded, _grounded);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveCanceled = ctx.canceled;

        var value = ctx.ReadValue<Vector2>() * speed;
        _velocity.x = value.x;
        _velocity.z = value.y;

        //animations
        if (inCombat)
        {
            _animator.SetFloat(Velocity, value.y);
            _animator.SetFloat(Strife, value.x);
        }
        else
        {
            _animator.SetFloat(Velocity, Mathf.Clamp(Math.Abs(value.y) + Math.Abs(value.x), 0, 1));
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (_jumped || !_grounded) return;
        _velocity.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
        _jumped = true;
        _grounded = false;
        _animator.SetTrigger(Jumped);
    }

    public void StartCombat(InputAction.CallbackContext ctx)
    {
        inCombat = !inCombat;
        _animator.SetBool(InCombatAnimation, inCombat);
    }

    private Collider FindClosestEnemy()
    {
        Physics.OverlapSphereNonAlloc(transform.position, _aggroAreaRadius, _hits);
        var minDist = 100f;
        Collider closestEnemy = null;

        foreach (var col in _hits)
        {
            if (!col.CompareTag("Enemy")) continue;

            if (Vector3.Distance(transform.position, col.transform.position) < minDist)
            {
                closestEnemy = col;
            }
        }

        return closestEnemy;
    }

    private IEnumerator StartCoyoteTime(float time)
    {
        yield return new WaitForSeconds(time);
        _grounded = false;
        _jumped = true;
    }
}