using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] private string levelSceneName;
    [SerializeField] private SpawnId spawnIdForTarget = SpawnId.None;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SceneTransition.nextSpawnId = spawnIdForTarget;
        Debug.Log($"[LevelController] Loading '{levelSceneName}' with spawnId {spawnIdForTarget}");

        SceneManager.LoadScene(levelSceneName);
    }
}
