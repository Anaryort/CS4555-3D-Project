using UnityEngine;

public class LevelEntrySpawner : MonoBehaviour
{
    [SerializeField] private Transform defaultSpawnPoint;       // normal starting position for this level
    [SerializeField] private Transform fromPreviousSpawnPoint;  // for SpawnId.FromPrevious
    [SerializeField] private Transform fromNextSpawnPoint;      // for SpawnId.FromNext

    private void Start()
    {
        Transform target = defaultSpawnPoint;

        switch (SceneTransition.nextSpawnId)
        {
            case SpawnId.FromPrevious:
                if (fromPreviousSpawnPoint != null) target = fromPreviousSpawnPoint;
                break;

            case SpawnId.FromNext:
                if (fromNextSpawnPoint != null) target = fromNextSpawnPoint;
                break;

            case SpawnId.None:
            default:
                // use defaultSpawnPoint
                break;
        }

        if (target != null)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = target.position;
                rb.rotation = target.rotation;
            }
            else
            {
                transform.SetPositionAndRotation(target.position, target.rotation);
            }

            Debug.Log($"[LevelEntrySpawner] Teleporting to {target.name} (spawnId={SceneTransition.nextSpawnId})");
        }
        else
        {
            Debug.Log("[LevelEntrySpawner] No spawn point set; leaving player where they are.");
        }

        // Reset so this value doesn't leak into future loads
        SceneTransition.nextSpawnId = SpawnId.None;
    }
}
