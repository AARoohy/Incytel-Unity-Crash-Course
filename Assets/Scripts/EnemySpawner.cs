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
    [SerializeField, Min(0f)] private float playerExclusionRadius = 5f;
    [SerializeField] private bool spawnImmediately = true;

    private readonly List<EnemyController> spawnedEnemies = new List<EnemyController>();
    private float nextSpawnTime;

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

        if (spawnedEnemies.Count >= maximumEnemies || Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void SpawnEnemy()
    {
        RemoveDestroyedEnemies();

        if (spawnedEnemies.Count >= maximumEnemies || enemyPrefab == null)
        {
            return;
        }

        if (!TryGetValidSpawnPoint(out Transform spawnPoint))
        {
            return;
        }

        EnemyController enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        spawnedEnemies.Add(enemy);
    }

    private bool TryGetValidSpawnPoint(out Transform spawnPoint)
    {
        spawnPoint = null;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return false;
        }

        int startingIndex = Random.Range(0, spawnPoints.Length);
        float exclusionRadiusSquared = playerExclusionRadius * playerExclusionRadius;

        for (int offset = 0; offset < spawnPoints.Length; offset++)
        {
            Transform candidate = spawnPoints[(startingIndex + offset) % spawnPoints.Length];

            if (candidate == null)
            {
                continue;
            }

            bool playerIsTooClose = player != null
                && (candidate.position - player.position).sqrMagnitude <= exclusionRadiusSquared;

            if (!playerIsTooClose)
            {
                spawnPoint = candidate;
                return true;
            }
        }

        return false;
    }

    private void RemoveDestroyedEnemies()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, playerExclusionRadius);
            }
        }
    }
}
