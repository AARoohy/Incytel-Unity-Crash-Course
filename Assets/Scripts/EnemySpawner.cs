using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.01f)] private float spawnInterval = 2f;
    [SerializeField, Min(1)] private int maximumEnemies = 10;
    [SerializeField] private bool spawnImmediately = true;

    private readonly List<EnemyController> spawnedEnemies = new List<EnemyController>();
    private float nextSpawnTime;
    private int lastSpawnPoint = -1;

    private void Start()
    {
        if (player == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();

            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        nextSpawnTime = spawnImmediately ? Time.time : Time.time + spawnInterval;
    }

    private void Update()
    {
        RemoveDestroyedEnemies();

        if (Time.time >= nextSpawnTime && spawnedEnemies.Count < maximumEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        int spawnIndex = GetRandomSpawnPointIndex();
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            return;
        }

        EnemyController enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        enemy.SetTarget(player);
        spawnedEnemies.Add(enemy);
    }

    private int GetRandomSpawnPointIndex()
    {
        if (spawnPoints.Length == 1)
        {
            lastSpawnPoint = 0;
            return 0;
        }

        int index;

        do
        {
            index = Random.Range(0, spawnPoints.Length);
        }
        while (index == lastSpawnPoint);

        lastSpawnPoint = index;
        return index;
    }

    private void RemoveDestroyedEnemies()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
    }
}
