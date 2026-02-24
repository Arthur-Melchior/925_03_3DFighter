using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Custom/Worm")]
[RequireComponent(typeof(RigBuilder))]
[ExecuteAlways]
public class WormScript : MonoBehaviour
{
    [Min(2)] [SerializeField] private int numberOfElements = 5;
    [SerializeField] private float gap = 2f;
    [SerializeField] private PrimitiveType shape;
    [SerializeField] private Transform bonesTransform;
    [SerializeField] private Transform rigTransform;
    [SerializeField] private GenerationType generationType;

    private RigBuilder _rigBuilder;

    private enum GenerationType
    {
        Chain,
        DampHead,
        DampBody,
        ChainDampBody,
        ChainDampHead,
    }

    private bool _isGenerating;

    private void Awake()
    {
        _rigBuilder = GetComponent<RigBuilder>();

        bonesTransform = GetOrCreateChild("Bones", transform);

        rigTransform = GetOrCreateChild("Rig", transform);
        GetOrAddComponent<Rig>(rigTransform.gameObject);
    }

    private void OnValidate()
    {
        if (!_isGenerating)
        {
            _isGenerating = true;
            switch (generationType)
            {
                case GenerationType.Chain:
                    EditorApplication.delayCall = GenerateChain;
                    break;
                case GenerationType.DampHead:
                    EditorApplication.delayCall = GenerateDampHead;
                    break;
                case GenerationType.DampBody:
                    EditorApplication.delayCall = GenerateDampBody;
                    break;
                case GenerationType.ChainDampBody:
                    EditorApplication.delayCall = GenerateChainDampBody;
                    break;
                case GenerationType.ChainDampHead:
                    //EditorApplication.delayCall = GenerateChainDampHead;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void GenerateChainDampBody()
    {
        DestroyOldElements();

        //create new elements
        var head = CreateNewPart(bonesTransform, transform.position, "Head");

        var oldPart = head.transform;

        for (var i = 1; i < numberOfElements; i++)
        {
            var newPart = CreateNewPart(oldPart, transform.position + new Vector3(0, 0, i * gap), $"Body {i}");

            CreateTransformConstraint(newPart.transform, oldPart.transform, $"Body {i} Constraint",
                0.8f / numberOfElements * i);

            oldPart = newPart.transform;
        }

        //create chain constraint
        CreateChainConstraint(head.transform.GetChild(0).transform, oldPart);

        _rigBuilder.Build();
        _isGenerating = false;
    }

    private void GenerateDampHead()
    {
        //Destroys old elements
        DestroyOldElements();

        var head = CreateNewPart(bonesTransform, transform.position, "Head");

        for (var i = 1; i < numberOfElements; i++)
        {
            var newPart = CreateNewPart(head.transform, transform.position + new Vector3(0, 0, i * gap), $"Body {i}");

            CreateTransformConstraint(newPart.transform, head.transform, $"Body {i} Constraint",
                0.8f / numberOfElements * i);
        }

        _rigBuilder.Build();
        _isGenerating = false;
    }

    private void GenerateDampBody()
    {
        //Destroys old elements
        DestroyOldElements();

        var head = CreateNewPart(bonesTransform, transform.position, "Head");
        var oldPart = head.transform;

        for (var i = 1; i < numberOfElements; i++)
        {
            var newPart = CreateNewPart(oldPart, transform.position + new Vector3(0, 0, i * gap), $"Body {i}");

            CreateTransformConstraint(newPart.transform, oldPart.transform, $"Body {i} Constraint",
                0.8f / numberOfElements * i);

            oldPart = newPart.transform;
        }

        _rigBuilder.Build();
        _isGenerating = false;
    }

    private void GenerateChain()
    {
        //Destroys old elements
        DestroyOldElements();

        //Creates new elements
        var head = CreateNewPart(bonesTransform, transform.position, "Head");
        var previousPart = bonesTransform;

        for (var i = 1; i < numberOfElements; i++)
        {
            var newPart = CreateNewPart(previousPart, transform.position + new Vector3(0, 0, i * gap), $"Body {i}");
            previousPart = newPart.transform;
        }

        //Updates constraints
        var body = bonesTransform.Find("Body 1");

        CreateTransformConstraint(body, head.transform, "Head Constraint", 0.2f);

        var chainConstraint = CreateChainConstraint(body, previousPart);

        CreateTransformConstraint(chainConstraint.transform, head.transform, "Tip Constraint");

        _rigBuilder.Build();
        _isGenerating = false;
    }

    private GameObject CreateChainConstraint(Transform root, Transform tip)
    {
        var chainConstraint = new GameObject
        {
            name = "Chain Constraint"
        };
        chainConstraint.transform.SetParent(rigTransform);
        var chainComponent = chainConstraint.AddComponent<ChainIKConstraint>();
        chainComponent.transform.position = transform.position + new Vector3(0, 0, numberOfElements * gap);
        chainComponent.data.root = root;
        chainComponent.data.tip = tip;
        chainComponent.data.target = chainConstraint.transform;
        chainComponent.data.chainRotationWeight = 0.5f;
        return chainConstraint;
    }

    private GameObject CreateTransformConstraint(Transform target, Transform source, string objectName = "Constraint",
        float dampValues = 0.5f)
    {
        var headConstraint = new GameObject
        {
            name = objectName
        };
        headConstraint.transform.SetParent(rigTransform);

        var headComponent = headConstraint.AddComponent<DampedTransform>();
        headComponent.data.constrainedObject = target;
        headComponent.data.sourceObject = source;
        headComponent.data.dampPosition = dampValues;
        headComponent.data.dampRotation = dampValues;

        return headConstraint;
    }

    private GameObject CreateNewPart(Transform parent, Vector3 position, string objectName = "Part")
    {
        var newPart = GameObject.CreatePrimitive(shape);
        newPart.name = objectName;

        newPart.transform.SetParent(parent);

        newPart.transform.position = position;
        newPart.transform.rotation = Quaternion.identity;
        newPart.transform.localScale = Vector3.one;

        return newPart;
    }
    
    private void DestroyOldElements()
    {
        for (var i = bonesTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(bonesTransform.GetChild(i).gameObject);
        }

        for (var i = rigTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(rigTransform.GetChild(i).gameObject);
        }
    }

    private Transform GetOrCreateChild(string transformName, Transform parent)
    {
        var child = parent.Find(transformName);
        if (child != null) return child;

        var go = new GameObject(transformName);
        go.transform.SetParent(parent);
        return go.transform;
    }
    
    private T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        return comp != null ? comp : obj.AddComponent<T>();
    }
}