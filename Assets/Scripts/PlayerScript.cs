using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Strife = Animator.StringToHash("Strife");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Jumped = Animator.StringToHash("Jumped");
    private static readonly int InCombatAnimation = Animator.StringToHash("InCombat");
    private static readonly int Dodge = Animator.StringToHash("Dodge");
    private static readonly int RotationDifference = Animator.StringToHash("RotationDifference");

    [Header("Exploration")] public float speed = 10f;
    public float jumpHeight = 2f;
    public float coyoteTime = 0.2f;
    public float rotationSpeed = 0.1f;

    [Header("Combat")] public float aggroAreaRadius = 10f;
    public float dodgeBoost = 1.2f;
    public CapsuleCollider swordTrigger;
    public SphereCollider shieldTrigger;
    public bool inCombat;

    [Header("Camera")] public CinemachineCamera cinemachineCamera;
    public float combatCameraZoomOutDistance = 3f;

    private Animator _animator;
    private CharacterController _cc;
    private Vector3 _velocity;
    private Vector3 _move;
    private bool _grounded = true;
    private bool _jumped;
    private bool _moveCanceled;
    private bool _isDodging;
    private bool _isAttacking;
    private bool _focusEnemy;

    private Transform _lookAtTarget;
    private Vector3 _originalLookAtTargetPosition;
    private CinemachineOrbitalFollow _cinemachineOrbitalFollow;
    private CinemachineRotationComposer _cinemachineRotationComposer;

    private Collider[] _hits;
    private bool _isDead;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();
        _lookAtTarget = cinemachineCamera.Target.LookAtTarget;
        _originalLookAtTargetPosition = _lookAtTarget.position;
        _cinemachineOrbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        _cinemachineRotationComposer = cinemachineCamera.GetComponent<CinemachineRotationComposer>();
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        //to avoid applying gravity when grounded so that the velocity doesn't build up
        if (!_cc.isGrounded)
            _velocity.y += Physics.gravity.y * Time.deltaTime;

        //for coyote time
        //checks if in the air and hasn't jumped
        //this also avoids the issue of starting the falling animation in stairs
        if (_grounded && !_cc.isGrounded && !_jumped)
        {
            StartCoroutine(StartCoyoteTime(coyoteTime));
        }
        else if (!_grounded && _cc.isGrounded)
        {
            StopAllCoroutines();
            _grounded = true;
            _jumped = false;
        }

        //applying movement to the character controller
        if (!_isDodging)
        {
            _move = new Vector3(
                        _velocity.z * cinemachineCamera.transform.forward.x +
                        _velocity.x * cinemachineCamera.transform.right.x,
                        _velocity.y,
                        _velocity.z * cinemachineCamera.transform.forward.z +
                        _velocity.x * cinemachineCamera.transform.right.z) *
                    Time.deltaTime;
        }

        _cc.Move(_move);


        //to rotate the character in the same direction that he's moving in
        //_moveCanceled is to avoid the character rotating to 0,0,0 when the player releases the input
        if (!_moveCanceled && !inCombat)
        {
            var rotation = Vector3.Lerp(transform.forward, _move, rotationSpeed);
            rotation.y = 0;
            transform.forward = rotation;
        }
        else if (inCombat)
        {
            var closestEnemy = FindClosestEnemy();

            if (closestEnemy)
            {
                if (_focusEnemy)
                {
                    //Focuses the camera on the middle point between the player and enemy
                    _lookAtTarget.transform.position = Vector3.Lerp(_lookAtTarget.transform.position,
                        (closestEnemy.transform.position + transform.position) / 2, 0.1f);
                }
                
                //homemade lerp to zoom out
                InputAxisLerp(_cinemachineOrbitalFollow.RadialAxis, combatCameraZoomOutDistance);

                //centers the camera
                _cinemachineRotationComposer.Composition.ScreenPosition = new Vector2(0, 0);

                //makes the character look at the closest enemy
                transform.LookAt(new Vector3(closestEnemy.transform.position.x, 0,
                    closestEnemy.transform.position.z));
            }
            else
            {
                //homemade lerp to zoom in
                InputAxisLerp(_cinemachineOrbitalFollow.RadialAxis, 1);

                //offsets camera
                _cinemachineRotationComposer.Composition.ScreenPosition = new Vector2(-0.05f, -0.01f);

                //Focuses the camera on the player
                _lookAtTarget.position = Vector3.Lerp(_lookAtTarget.transform.position,
                    new Vector3(transform.position.x, _originalLookAtTargetPosition.y, transform.position.z), 0.1f);

                transform.forward = new Vector3(cinemachineCamera.transform.forward.x, 0,
                    cinemachineCamera.transform.forward.z);
            }
        }

        _animator.SetBool(Grounded, _grounded);
        _animator.SetBool(InCombatAnimation, inCombat);
    }

    private void InputAxisLerp(InputAxis axis, float zoomDistance, float zoomSpeed = 0.1f)
    {
        if (Mathf.Approximately(axis.Value, zoomDistance))
            return;

        if (axis.Value < zoomDistance)
        {
            var rad = _cinemachineOrbitalFollow.RadialAxis;
            _cinemachineOrbitalFollow.RadialAxis = new InputAxis
            {
                Value = rad.Value + zoomSpeed, Center = rad.Center + zoomSpeed,
                Range = new Vector2(rad.Range.x + zoomSpeed, rad.Range.y + zoomSpeed)
            };
        }
        else if (axis.Value > zoomDistance)
        {
            var rad = _cinemachineOrbitalFollow.RadialAxis;
            _cinemachineOrbitalFollow.RadialAxis = new InputAxis
            {
                Value = rad.Value - zoomSpeed, Center = rad.Center - zoomSpeed,
                Range = new Vector2(rad.Range.x - zoomSpeed, rad.Range.y - zoomSpeed)
            };
        }
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
        if (_jumped || !_grounded || !ctx.performed || _isDodging) return;

        if (inCombat)
        {
            _isDodging = true;
            _move *= dodgeBoost;
            _animator.SetTrigger(Dodge);
        }
        else
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
            _jumped = true;
            _grounded = false;
            _animator.SetTrigger(Jumped);
        }
    }

    public void OnLAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (!inCombat)
        {
            inCombat = true;
        }

        _isAttacking = true;
        shieldTrigger.enabled = true;
        _animator.SetTrigger("LightAttack");
    }

    public void OnHAttack(InputAction.CallbackContext ctx)
    {
        if (!inCombat)
        {
            inCombat = true;
        }

        if (!ctx.performed) return;

        _isAttacking = true;
        swordTrigger.enabled = true;
        _animator.SetTrigger("HeavyAttack");
    }

    public void UnlockInputs()
    {
        _isDodging = false;
        _isAttacking = false;
    }

    public void ToggleCombat(InputAction.CallbackContext ctx)
    {
        inCombat = !inCombat;
        _animator.SetBool(InCombatAnimation, inCombat);
    }

    private Collider FindClosestEnemy()
    {
        _hits = Physics.OverlapSphere(transform.position, aggroAreaRadius, 1 << 8);
        var minDist = 100f;
        Collider closestEnemy = null;

        foreach (var col in _hits)
        {
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

    public void Die()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Parry")) return;
        _isDead = true;
        _animator.SetBool("Death", true);
    }

    public void Resurrect()
    {
        _isDead = false;
        _animator.SetBool("Death", false);
    }

    public void DisableSword() => swordTrigger.enabled = false;
    public void DisableShield() => shieldTrigger.enabled = false;
    public void ToggleFocus(InputAction.CallbackContext ctx) => _focusEnemy = !_focusEnemy;
}