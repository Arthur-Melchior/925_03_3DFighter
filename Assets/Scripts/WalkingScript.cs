using System;
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

    public float speed = 20f;
    public float jumpHeight = 2f;

    private Animator _animator;
    private CharacterController _cc;
    private Vector3 _velocity;
    private Camera _camera;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    private void Update()
    {
        _velocity.y += Physics.gravity.y * Time.deltaTime;
        var move = new Vector3(
            _velocity.z * _camera.transform.forward.x + _velocity.x * _camera.transform.right.x,
            _velocity.y,
            _velocity.z * _camera.transform.forward.z + _velocity.x * _camera.transform.right.z) * Time.deltaTime;
        _cc.Move(move);

        transform.forward = new Vector3(move.x, 0, move.z);
        _animator.SetBool(Grounded, _cc.isGrounded);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        _velocity = new Vector3(value.x, 0, value.y) * speed;

        //animations
        _animator.SetFloat(Velocity, Mathf.Clamp(Mathf.Abs(value.y + value.x), 0, 1));
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!_cc.isGrounded) return;
        _velocity.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
        _animator.SetTrigger(Jumped);
    }
}