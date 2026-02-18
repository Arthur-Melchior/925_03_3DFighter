using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using Object = UnityEngine.Object;

[DisallowMultipleComponent]
[AddComponentMenu("Custom/Worm")]
[RequireComponent(typeof(Transform))]
[Icon("Assets/Editor/worm.png")]
[ExecuteAlways]
public class WormScript : MonoBehaviour
{
    [Min(0)] [SerializeField] private int numberOfElements = 5;
    [SerializeField] private float gap = 2f;
    [SerializeField] private PrimitiveType shape;
    [SerializeField] private Transform bonesTransform;

    private bool _isGenerating;

    private void OnValidate()
    {
        if (!_isGenerating)
        {
            _isGenerating = true;
        }
    }

    private void LateUpdate()
    {
        if (_isGenerating)
        {
            Generate();
        }
    }

    private void Generate()
    {
        for (var i = bonesTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(bonesTransform.GetChild(i).gameObject);
        }

        var previousPart = bonesTransform;

        for (var i = 0; i < numberOfElements; i++)
        {
            var newPart = GameObject.CreatePrimitive(shape);

            newPart.transform.SetParent(previousPart.transform);

            newPart.transform.position = transform.position + new Vector3(0, 0, i * gap);
            newPart.transform.rotation = Quaternion.identity;
            newPart.transform.localScale = Vector3.one;

            previousPart = newPart.transform;
        }

        _isGenerating = false;
    }
}