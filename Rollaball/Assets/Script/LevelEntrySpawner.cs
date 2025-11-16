using UnityEngine;

public class LevelEntrySpawner : MonoBehaviour
{
    [Header("Spawn points")]
    [SerializeField] private Transform defaultSpawnPoint;       // normal starting pos for this level
    [SerializeField] private Transform fromPreviousSpawnPoint;  // used when SpawnId.FromPrevious
    [SerializeField] private Transform fromNextSpawnPoint;      // used when SpawnId.FromNext

    [Header("Players")]
    [SerializeField] private Rigidbody player1;
    [SerializeField] private Rigidbody player2;
    [SerializeField] private Vector3 player2Offset = new Vector3(1f, 0f, 0f); 
    // small offset so they don't spawn inside each other

    private void Start()
    {
        Transform basePoint = defaultSpawnPoint;

        switch (SceneTransition.nextSpawnId)
        {
            case SpawnId.FromPrevious:
                if (fromPreviousSpawnPoint != null)
                    basePoint = fromPreviousSpawnPoint;
                break;

            case SpawnId.FromNext:
                if (fromNextSpawnPoint != null)
                    basePoint = fromNextSpawnPoint;
                break;

            case SpawnId.None:
            default:
                // just use defaultSpawnPoint
                break;
        }

        if (basePoint != null)
        {
            // Player 1 at basePoint
            SpawnPlayer(player1, basePoint.position, basePoint.rotation);

            // Player 2 at basePoint + offset
            SpawnPlayer(player2, basePoint.position + player2Offset, basePoint.rotation);

            Debug.Log($"[LevelEntrySpawner] SpawnId={SceneTransition.nextSpawnId}, base={basePoint.name}");
        }
        else
        {
            Debug.LogWarning("[LevelEntrySpawner] No spawn point assigned; doing nothing.");
        }

        // Reset so it doesn't leak into future loads
        SceneTransition.nextSpawnId = SpawnId.None;
    }

    private void SpawnPlayer(Rigidbody rb, Vector3 pos, Quaternion rot)
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        rb.rotation = rot;
    }
}
