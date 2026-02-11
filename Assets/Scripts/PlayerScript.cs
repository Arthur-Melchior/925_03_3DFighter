using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerScript : MonoBehaviour
{
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Strife = Animator.StringToHash("Strife");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Jumped = Animator.StringToHash("Jumped");
    private static readonly int InCombatAnimation = Animator.StringToHash("InCombat");
    private static readonly int Dodge = Animator.StringToHash("Dodge");
    private static readonly int RotationDifference = Animator.StringToHash("RotationDifference");
    private static readonly int Parry = Animator.StringToHash("LightAttack");
    private static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");
    private static readonly int Death = Animator.StringToHash("Death");

    [Header("Exploration")] public float speed = 10f;
    public float jumpHeight = 2f;
    public float coyoteTime = 0.2f;
    public float rotationSpeed = 0.1f;

    [Header("Combat")] public float aggroAreaRadius = 10f;
    public float dodgeBoost = 1.2f;
    public CapsuleCollider swordTrigger;
    public SphereCollider shieldHitbox;
    public bool inCombat;

    [Header("Camera")] public CinemachineCamera cinemachineCamera;
    public float combatCameraZoomOutDistance = 3f;

    private Animator _animator;
    private CharacterController _cc;
    private AudioSource _audioSource;
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
        _audioSource = GetComponent<AudioSource>();
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

        if (inCombat)
        {
            CombatLogic();
        }
        else
        {
            ExplorationLogic();
        }
    }

    private void ExplorationLogic()
    {
        //PHYSICS
        //to avoid applying gravity when grounded so that the velocity doesn't build up
        if (!_cc.isGrounded)
            _velocity.y += Physics.gravity.y * Time.deltaTime;

        //MOVEMENT
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

        _move = new Vector3(
                    _velocity.z * cinemachineCamera.transform.forward.x +
                    _velocity.x * cinemachineCamera.transform.right.x,
                    _velocity.y,
                    _velocity.z * cinemachineCamera.transform.forward.z +
                    _velocity.x * cinemachineCamera.transform.right.z) *
                Time.deltaTime;

        _cc.Move(_move);

        //to rotate the character in the same direction that he's moving in
        //_moveCanceled is to avoid the character rotating to 0,0,0 when the player releases the input
        if (!_moveCanceled)
        {
            var rotation = Vector3.Lerp(transform.forward, _move, rotationSpeed);
            rotation.y = 0;
            transform.forward = rotation;
        }

        _animator.SetBool(Grounded, _grounded);
    }

    private void CombatLogic()
    {
        //MOVEMENT
        //to avoid the direction changing while dodging
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

        if (!_isAttacking)
        {
            _cc.Move(_move);
        }

        //CAMERA
        if (_focusEnemy)
        {
            var closestEnemy = FindClosestEnemy();
            if (closestEnemy)
            {
                //Focuses the camera on the middle point between the player and enemy
                _lookAtTarget.transform.position = Vector3.Lerp(_lookAtTarget.transform.position,
                    (closestEnemy.transform.position + transform.position) / 2, 0.1f);

                //makes the character look at the closest enemy
                transform.LookAt(new Vector3(closestEnemy.transform.position.x, 0,
                    closestEnemy.transform.position.z));
            }
        }
        else
        {
            //makes the character look in the direction of the camera
            transform.forward = new Vector3(cinemachineCamera.transform.forward.x, 0,
                cinemachineCamera.transform.forward.z);
        }
    }

    public void EnterCombat()
    {
        inCombat = true;

        //homemade lerp to zoom out
        StartCoroutine(InputAxisLerp(_cinemachineOrbitalFollow.RadialAxis, combatCameraZoomOutDistance));

        //centers the camera
        _cinemachineRotationComposer.Composition.ScreenPosition = new Vector2(0, 0);

        _animator.SetBool(InCombatAnimation, true);
    }

    public void ExitCombat()
    {
        inCombat = false;

        //homemade lerp to zoom in
        StartCoroutine(InputAxisLerp(_cinemachineOrbitalFollow.RadialAxis, 1));

        //offsets camera
        _cinemachineRotationComposer.Composition.ScreenPosition = new Vector2(-0.05f, -0.01f);

        //Focuses the camera on the player
        _lookAtTarget.position = Vector3.Lerp(_lookAtTarget.transform.position,
            new Vector3(transform.position.x, _originalLookAtTargetPosition.y, transform.position.z), 0.1f);

        _animator.SetBool(InCombatAnimation, false);
    }

    public void UnlockInputs()
    {
        _isDodging = false;
        _isAttacking = false;
    }

    public void ToggleCombat(InputAction.CallbackContext ctx)
    {
        if (inCombat)
        {
            ExitCombat();
        }
        else
        {
            EnterCombat();
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
        if (!ctx.performed) return;

        if (inCombat)
        {
            if (_isDodging) return;

            _isDodging = true;
            _move *= dodgeBoost;
            _animator.SetTrigger(Dodge);
        }
        else
        {
            if (_jumped || !_grounded) return;

            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
            _jumped = true;
            _grounded = false;
            _animator.SetTrigger(Jumped);
        }
    }

    public void OnParry(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !inCombat) return;

        _isAttacking = true;

        //the shield hit boxes only exists when parrying and is set to false by an animation event
        shieldHitbox.enabled = true;
        _animator.SetTrigger(Parry);
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !inCombat) return;

        _isAttacking = true;
        //the sword hit boxes only exists when parrying and is set to false by an animation event
        swordTrigger.enabled = true;
        _animator.SetTrigger(HeavyAttack);
    }

    public void Die()
    {
        //invulnerable while parrying
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Parry")) return;
        _isDead = true;
        _animator.SetBool(Death, true);
        _animator.Play("Die");
    }

    public void Resurrect()
    {
        _isDead = false;
        _animator.SetBool(Death, false);
    }

    public void AttackFinished() => _isAttacking = false;
    public void DisableSword() => swordTrigger.enabled = false;
    public void DisableShield() => shieldHitbox.enabled = false;
    public void ToggleFocus(InputAction.CallbackContext ctx) => _focusEnemy = !_focusEnemy;

    public void EnableControls()
    {
        this.GetComponent<PlayerInput>().enabled = true;
        EnterCombat();
        _audioSource.time = 90f;
        _audioSource.Play();
    }

    //HELPER FUNCTIONS
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

    private IEnumerator InputAxisLerp(InputAxis axis, float zoomDistance, float zoomSpeed = 0.1f)
    {
        while (true)
        {
            if (Mathf.Approximately(axis.Value, zoomDistance))
                yield break;

            if (axis.Value < zoomDistance)
            {
                axis = new InputAxis
                {
                    Value = axis.Value + zoomSpeed, Center = axis.Center + zoomSpeed,
                    Range = new Vector2(axis.Range.x + zoomSpeed, axis.Range.y + zoomSpeed)
                };
            }
            else if (axis.Value > zoomDistance)
            {
                axis = new InputAxis
                {
                    Value = axis.Value - zoomSpeed, Center = axis.Center - zoomSpeed,
                    Range = new Vector2(axis.Range.x - zoomSpeed, axis.Range.y - zoomSpeed)
                };
            }

            _cinemachineOrbitalFollow.RadialAxis = axis;

            //Waits for next frame
            yield return null;
        }
    }

    private IEnumerator StartCoyoteTime(float time)
    {
        yield return new WaitForSeconds(time);
        _grounded = false;
        _jumped = true;
    }
}