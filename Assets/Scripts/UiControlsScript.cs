using System;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiControlsScript : MonoBehaviour
{
        [SerializeField] private UIControls data;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Image leftImage;
        [SerializeField] private Image rightImage;

        private void OnEnable()
        {
                description.text = data.description;
                leftImage.sprite = data.keyboardSprite;
                rightImage.sprite = data.controllerSprite;
        }
}