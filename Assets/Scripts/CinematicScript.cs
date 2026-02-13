using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
[RequireComponent(typeof(PlayerInput))]
public class CinematicScript : MonoBehaviour
{
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private PlayerScript player;
    [SerializeField] private EnemiesController enemiesController;
    [SerializeField] private GameObject cinematicControls;
    [SerializeField] private GameObject gameplayControls;
    
    private PlayableDirector _director;
    private PlayerInput _input;
    
    private void Start()
    {
        _director = GetComponent<PlayableDirector>();
        _input = GetComponent<PlayerInput>();
    }

    private void Awake()
    {
        cinematicControls.SetActive(true);
        gameplayControls.SetActive(false);
    }

    public void changeTextBox(string text)
    {
        textBox.text = text;
    }

    public void SwitchToGameMode()
    {
        cinematicControls.SetActive(false);
        gameplayControls.SetActive(true);

        //to focus the player camera
        playerCamera.Priority.Value = 10;
        changeTextBox("");
        _input.enabled = false;
        _director.Stop();
        player.EnableControls();
        enemiesController.SpawnEnemy();
    }

    public void OnSkip(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        SwitchToGameMode();
    }
}