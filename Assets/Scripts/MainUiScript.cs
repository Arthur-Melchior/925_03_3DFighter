using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUiScript : MonoBehaviour
{
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button winButton;

    public void GameWon()
    {
        winText.gameObject.SetActive(true);
        winButton.gameObject.SetActive(true);
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}