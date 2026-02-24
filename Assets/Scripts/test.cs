using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class test : MonoBehaviour
{
    [SerializeField] private bool test2;
    [SerializeField] private MultiParentConstraint mpc;

    private void OnValidate()
    {
        EditorApplication.delayCall = truc;
    }

    private void truc()
    {
        mpc.data.sourceObjects.Add(new WeightedTransform(transform,0));
        GetComponent<RigBuilder>().Build();
    }
}
