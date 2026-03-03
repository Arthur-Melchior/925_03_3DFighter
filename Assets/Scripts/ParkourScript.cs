using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ParkourScript : MonoBehaviour
{
    public AnimationCurve Curve;
    [SerializeField] private Transform target;
    public UnityEvent jumpFinished;
    private Animator _animator;
    private CharacterController _characterController;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        _animator.SetBool("isJumping", true);

        var hits = Physics.SphereCastAll(transform.position, 30, transform.forward);
        foreach (var raycastHit in hits)
        {
            if (raycastHit.transform.gameObject.CompareTag("Ledge"))
            {
                target = raycastHit.transform;
                transform.LookAt(raycastHit.transform.parent.position);
                StartCoroutine(MoveTo(raycastHit.transform.position, 1));
                break;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        var value = ctx.ReadValue<Vector2>();
        _characterController.Move(target.right * value.x);
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            var t = time / duration;
            transform.position = Vector3.Lerp(start, target, Curve.Evaluate(t));
            yield return null;
        }

        transform.position = target;
        jumpFinished?.Invoke();
    }
}