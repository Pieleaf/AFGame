using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using NUnit.Framework;
# endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Debug Tools")]
    public int startWithLimbs = 0;
    public int startWithPrey = 0;

    public bool debugSkipStartMenu = false;
    public bool debugCollectAllOrbs = false;

    [Header("Settings")]
    [Tooltip("Spawns new orbs if collectable orb count goes below this.")]
    public int minimumOrbs = 20;

    public Orb orbPrefab;
    public Prey preyPrefab;
    public PlayerController player;

    [ReadOnly]
    public int orbsSpawned;
    [ReadOnly]
    public int preySpawned;
    [ReadOnly]
    public PreyCage[] preyCages;
    [ReadOnly]
    public bool gameStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = player != null ? player : FindFirstObjectByType<PlayerController>();

        preyCages = Tools.GetComponentsInScene<PreyCage>().ToArray();

        AudioListener.volume = 0f;
    }

    private void Start()
    {
        if (debugCollectAllOrbs)
        {
            foreach (var orb in GetAllCollectableOrbs())
            {
                orb.Collect();
            }
        }

        if (debugSkipStartMenu)
        {
            StartGame();
        }
        else
        {
            UIManager.Instance.OpenStartMenu();
        }
    }

    public void OrbCollected(Orb orb)
    {
        Debug.Log($"Orb collected: {orb}");

        #if UNITY_EDITOR
        Assert.IsNotNull(orb);
        Assert.IsTrue(orb.state == OrbState.Collectable);
        # endif

        if (orb == null || orb.state != OrbState.Collectable)
            return;


        SFXPlayer.Instance.PlayCollect();
        EffectsManager.Instance.OrbCollected(orb);

        SnakeManager.Instance.AddLimb(orb);

        if (ShopManager.Instance.birdPurchases <= 0)
        {
            var orbsToSpawn = minimumOrbs - GetAllCollectableOrbs().Count;
            for (int i = 0; i < orbsToSpawn; i++)
            {
                SpawnOrb();
            }
        }
    }

    public void PreyCollected(Prey prey)
    {
        prey.Destroy();
    }

    public Orb SpawnOrb()
    {
        PreyCage cage = FindCage();
        if (cage == null)
        {
            Debug.LogWarning("SpawnOrb(): No active cage found");

            return SpawnOrb(new Vector3(
                Random.Range(-50, 50),
                Random.Range(-5, 20),
                Random.Range(-50, 50)));
        }

        var minX = cage.collid.bounds.min.x;
        var maxX = cage.collid.bounds.max.x;
        var minY = cage.collid.bounds.min.y;
        var maxY = cage.collid.bounds.max.y;
        var minZ = cage.collid.bounds.min.z;
        var maxZ = cage.collid.bounds.max.z;

        var pos = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            Random.Range(minZ, maxZ));

        return SpawnOrb(pos);
    }

    public Orb SpawnOrb(Vector3 position)
    {
        Orb orb = Instantiate(orbPrefab);
        orb.name += $" {orbsSpawned}";

        orb.transform.position = position;

        //Debug.Log($"Spawning orb: {orb}");
        orbsSpawned++;
        return orb;
    }

    public Prey SpawnPrey()
    {
        return SpawnPrey(Vector3.zero);
    }

    public Prey SpawnPrey(Vector3 position)
    {
        PreyCage cage = FindCage();
        if (cage == null)
            return null;

        Prey prey = Instantiate(preyPrefab);
        prey.name += $" {preySpawned}";

        prey.transform.parent = cage.transform;
        prey.transform.localPosition = Vector3.zero;

        preySpawned++;

        Debug.Log($"{this} Spawning prey {prey} in cage {cage}");
        return prey;
    }

    private PreyCage FindCage()
    {
        PreyCage cage = null;
        foreach (var c in preyCages)
        {
            if (c.gameObject.activeSelf && c.active)
            {
                cage = c;
                break;
            }
        }

        return cage;
    }

    internal List<Orb> GetAllCollectableOrbs()
    {
        var list = new List<Orb>();

        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            var foundOrbs = root.GetComponentsInChildren<Orb>();
            foreach (var o in foundOrbs)
            {
                if (o.state == OrbState.Collectable)
                    list.Add(o);
            }
        }
        return list;
    }

    internal List<Prey> GetAllPrey()
    {
        var list = new List<Prey>();

        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            var foundPrey = root.GetComponentsInChildren<Prey>();
            list.AddRange(foundPrey);
        }
        return list;
    }

    internal void GivePlayerOrb()
    {
        var orb = SpawnOrb(Vector3.zero);
        orb.Collect();
    }

    internal void StartGame()
    {
        Debug.Log($"{this} StartGame()");

        AudioListener.volume = 1f;

        UIManager.Instance.SetPaused(false);
        player.Activate();
        gameStarted = true;

        for (int i = 0; i < startWithLimbs; i++)
        {
            GivePlayerOrb();
        }
        for (int i = 0; i < startWithPrey; i++)
        {
            SpawnPrey();
        }
    }

    internal void ClearAllOrbs()
    {
        foreach (var orb in GetAllCollectableOrbs())
        {
            Destroy(orb);
        }
    }
}
