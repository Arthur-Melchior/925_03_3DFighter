using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemiesController : MonoBehaviour
{
    public GameObject enemyPrefab;
    public PlayerScript player;
    
    public void SpawnEnemy()
    {
        var enemy = Instantiate(enemyPrefab, transform);
        enemy.GetComponent<EnemyScript>().player = player;
        StartCoroutine(EnableLater(enemy));
    }

    private IEnumerator EnableLater(GameObject enemy, float waitTime = 2f)
    {
        var ani = enemy.GetComponent<Animator>();
        yield return new WaitForSeconds(waitTime);
        enemy.transform.position = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
        ani.enabled = true;
    }
}