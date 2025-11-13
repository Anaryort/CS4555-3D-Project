using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] private string levelSceneName;           // the scene to load
    [SerializeField] private SpawnId spawnIdForTarget = SpawnId.None;  // which spawn slot the TARGET scene should use

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Tell the target scene which spawn type to use
        SceneTransition.nextSpawnId = spawnIdForTarget;
        Debug.Log($"[LevelController] Loading '{levelSceneName}' with spawnId {spawnIdForTarget}");

        SceneManager.LoadScene(levelSceneName);
    }
}
