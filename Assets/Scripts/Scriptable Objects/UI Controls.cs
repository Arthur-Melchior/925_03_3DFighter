using UnityEngine;
using UnityEngine.UI;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "UIControls", menuName = "Scriptable Objects/UIControls")]
    public class UIControls : ScriptableObject
    {
        public Sprite keyboardSprite;
        public Sprite controllerSprite;
        public string description;
    }
}
