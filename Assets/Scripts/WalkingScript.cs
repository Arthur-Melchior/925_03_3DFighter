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

    public float speed = 20f;
    public float jumpHeight = 2f;

    private Animator _animator;
    private CharacterController _cc;
    private Vector3 _velocity;
    private Camera _camera;
    private bool _hasJumped;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    private void Update()
    {
        _cc.SimpleMove(new Vector3(
            _velocity.z * _camera.transform.forward.x + _velocity.x * _camera.transform.right.x,
            0,
            _velocity.z * _camera.transform.forward.z + _velocity.x * _camera.transform.right.z));

        if (_hasJumped)
        {
            _cc.Move(new Vector3(0, _velocity.y * Time.deltaTime, 0));
        }

        //to rotate to camera when moving
        if (_velocity.sqrMagnitude > 0f)
        {
            transform.eulerAngles = new Vector3(0, _camera.transform.eulerAngles.y, 0);
        }

        if (_cc.isGrounded && _hasJumped)
        {
            _hasJumped = false;
            _animator.SetBool(Grounded, true);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        _velocity = new Vector3(value.x, 0, value.y) * speed;

        //animations
        _animator.SetFloat(Velocity, value.y);
        _animator.SetFloat(Strife, value.x);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!_cc.isGrounded || _hasJumped) return;
        _velocity.y = Mathf.Sqrt(Mathf.Abs(jumpHeight * 2f * Physics.gravity.y));
        _hasJumped = true;
        _animator.SetBool(Grounded, false);
    }
}