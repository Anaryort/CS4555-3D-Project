public enum SpawnId
{
    None,
    FromPrevious,  // came from previous level (e.g., L1 -> L2)
    FromNext       // came from next level (e.g., L3 -> L2)
}

public static class SceneTransition
{
    public static SpawnId nextSpawnId = SpawnId.None;
}
