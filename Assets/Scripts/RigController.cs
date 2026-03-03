using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class RigController : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint leftArmBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint rightArmBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint rightLegBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint leftLegBoneIKConstraint;
    [SerializeField] private RigBuilder rigBuilder;
    private Transform _leftArmboneIKTarget;
    private Transform _rightArmboneIKTarget;

    private void Start()
    {
        _leftArmboneIKTarget = leftArmBoneIKConstraint.data.target;
        _rightArmboneIKTarget = rightArmBoneIKConstraint.data.target;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        //trouve ledge et le mettre comme target
        var hits = Physics.SphereCastAll(transform.position, 30, transform.forward);
        foreach (var raycastHit in hits)
        {
            MoveIkTarget(raycastHit, "Right Hand Ledge", rightArmBoneIKConstraint);
            MoveIkTarget(raycastHit, "Left Hand Ledge", leftArmBoneIKConstraint);
            MoveIkTarget(raycastHit, "Right Leg Ledge", rightLegBoneIKConstraint);
            MoveIkTarget(raycastHit, "Left Leg Ledge", leftLegBoneIKConstraint);
        }
    }

    private void MoveIkTarget(RaycastHit raycastHit, string ledge, TwoBoneIKConstraint twoBoneIKConstraint)
    {
        if (raycastHit.transform.gameObject.CompareTag(ledge))
        {
            twoBoneIKConstraint.weight = 1f;
            twoBoneIKConstraint.data.target = raycastHit.transform;

            rigBuilder.Build();
        }
    }

    public void ResetTargets()
    {
        _leftArmboneIKTarget.transform.position = leftArmBoneIKConstraint.data.target.position;
        leftArmBoneIKConstraint.data.target = _leftArmboneIKTarget;

        _rightArmboneIKTarget.transform.position = rightArmBoneIKConstraint.data.target.position;
        rightArmBoneIKConstraint.data.target = _rightArmboneIKTarget;
        GetComponent<RigBuilder>().Build();
    }
}