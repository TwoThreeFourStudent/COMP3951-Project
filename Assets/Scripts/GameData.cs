using UnityEngine;

public class GameData : MonoBehaviour
{
    public int DeathCount { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void IncrementDeathCount()
    {
        DeathCount++;
    }

    public void SetDeathCount(int deathCountFromDatabase)
    {
        DeathCount = deathCountFromDatabase;
    }
}
