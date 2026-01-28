using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class WalkingScript : MonoBehaviour
{
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int RotationDifference = Animator.StringToHash("RotationDifference");
    private static readonly int Strife = Animator.StringToHash("Strife");

    public float speed = 20f;
    public float turnSpeed = 10f;

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
        //_velocity.z *= transform.forward.z;
        //to keep running ahead
        _cc.SimpleMove(new Vector3(
            _velocity.z * _camera.transform.forward.x + _velocity.x * _camera.transform.right.x,
            0,
            _velocity.z * _camera.transform.forward.z + _velocity.x * _camera.transform.right.z));

        //to rotate to camera when moving
        // var cameraRotationDifference = Mathf.DeltaAngle(_camera.transform.eulerAngles.y, transform.eulerAngles.y);
        if (_velocity.sqrMagnitude > 0f)
        {
            //transform.rotation = new Quaternion(0, _camera.transform.rotation.y, 0, 0);
            var test = new Quaternion(transform.rotation.x, _camera.transform.rotation.y, transform.rotation.z,transform.rotation.w);
            transform.rotation = test;
            // var rotationDirection = cameraRotationDifference > 0 ? -1 : 1;
            // transform.Rotate(new Vector3(0, rotationDirection * turnSpeed * Time.deltaTime, 0));
            // _animator.SetFloat(RotationDifference, cameraRotationDifference);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        _velocity = new Vector3(value.x, 0, value.y) * speed;


        //animations
        _animator.SetFloat(Velocity, value.y);
        _animator.SetFloat(Strife, value.x);
        //_animator.SetFloat(RotationDifference, rotationDifference);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        _animator.SetTrigger("Jump");
    }

    public void ChangeRotation(float value)
    {
        transform.Rotate(new Vector3(0, value, 0));
    }
}