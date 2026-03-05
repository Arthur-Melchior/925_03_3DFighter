using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ParkourScript : MonoBehaviour
{
    public UnityEvent jumpFinished;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private SplineContainer spline;
    [SerializeField] private float speed;
    private float _pointOnSpline;
    private Vector3 _pointOnSplinePosition;
    private float _directionX;
    private Vector3 _direction;
    private Animator _animator;
    private CharacterController _characterController;
    private float _splineLength;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _splineLength = spline.CalculateLength();
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
                transform.LookAt(raycastHit.transform.parent.position);
                StartCoroutine(MoveTo(raycastHit.transform.position, 1));
                break;
            }
        }
    }

    private void Update()
    {
        if (_directionX != 0)
        {
            _pointOnSpline += _directionX * speed * Time.deltaTime / _splineLength;
            _pointOnSpline = Mathf.Clamp01(_pointOnSpline);
            _pointOnSplinePosition = spline.EvaluatePosition(_pointOnSpline);
            _direction = _pointOnSplinePosition - transform.position;

            if (_direction.magnitude > 0.01f)
            {
                _characterController.Move(_direction);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        _directionX = value.x;
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        var start = transform.position;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            var t = time / duration;
            transform.position = Vector3.Lerp(start, target, jumpCurve.Evaluate(t));
            yield return null;
        }

        transform.position = target;
        jumpFinished?.Invoke();
    }
}