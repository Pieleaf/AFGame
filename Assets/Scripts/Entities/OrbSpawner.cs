using UnityEngine;

public class OrbSpawner : MonoBehaviour
{
    public Orb orbPrefab;

    public float spawnRate = 1f; // per second
    public float spawnDelay = 1f;

    public float noiseSpawnAt = 0.4f; // Turn spawning on when noise meets this

    public float noiseOffset = 0f;
    public float noiseScale = 0.01f;

    [ReadOnly]
    public bool isSpawning = false;
    [ReadOnly]
    public float noise;
    [ReadOnly]
    public int orbsSpawned = 0;

    int index = 0;

    void Start()
    {
        noiseOffset += Random.Range(0 , 100000);
        spawnDelay += Random.value;

        InvokeRepeating(nameof(SpawnOrb), spawnDelay, spawnRate);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        index++;
        noise = Mathf.PerlinNoise1D((index + noiseOffset) * noiseScale);

        isSpawning = noise >= noiseSpawnAt;        
    }

    private void SpawnOrb()
    {
        if (isSpawning)
        {
            //Debug.Log($"{this} ({transform.parent}) SpawnOrb()");
            GameManager.Instance.SpawnOrb(transform.position);
        }
    }
}
