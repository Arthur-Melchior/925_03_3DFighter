using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class EnemiesController : MonoBehaviour
{
    public UnityEvent gameWon;
    
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PlayerScript player;
    
    private float _enemyCount;
    private float _killCount;

    public void SpawnEnemy()
    {
        var enemy = Instantiate(enemyPrefab, transform);
        _enemyCount++;
        var script = enemy.GetComponent<EnemyScript>();
        script.player = player;
        script.onDie.AddListener(OnEnemyDie);
        StartCoroutine(EnableLater(enemy));
    }

    private IEnumerator EnableLater(GameObject enemy, float waitTime = 3f)
    {
        var ani = enemy.GetComponent<Animator>();
        yield return new WaitForSeconds(waitTime);
        ani.enabled = true;
    }

    private void OnEnemyDie()
    {
        ++_killCount;
        Debug.Log($"enemy killed {_killCount}");
        if (_killCount > 99)
        {
            gameWon?.Invoke();
            enabled = false;
        }
        else if (_killCount == _enemyCount)
        {
            var rand = new System.Random();
            for (int i = 0; i < rand.Next(3, 11); i++)
            {
                SpawnEnemy();
            }
        }
    }
}