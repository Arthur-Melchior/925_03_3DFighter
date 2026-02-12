using System;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
        public UIControls data;
        public TMP_Text description;
        public Image leftImage;
        public Image rightImage;

        private void OnEnable()
        {
                description.text = data.description;
                leftImage.sprite = data.keyboardSprite;
                rightImage.sprite = data.controllerSprite;
        }
}