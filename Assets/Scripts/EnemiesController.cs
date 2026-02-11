using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemiesController : MonoBehaviour
{
    public GameObject enemyPrefab;
    public PlayerScript player;
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
        enemy.transform.position = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
        ani.enabled = true;
    }

    private void OnEnemyDie()
    {
        ++_killCount;
        if (_killCount == _enemyCount)
        {
            var rand = new System.Random();
            for (int i = 0; i < rand.Next(3, 11); i++)
            {
                SpawnEnemy();
            }
        }
    }
}