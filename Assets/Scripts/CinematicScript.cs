using TMPro;
using UnityEngine;

public class CinematicScript : MonoBehaviour
{
    public TMP_Text textBox;
    
    public void changeTextBox(string text)
    {
        textBox.text = text;
    }
}
