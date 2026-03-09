using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public SaveData loadedData;
    private string savePath;

    private void Awake()
    {
        Instance = this;

        savePath = Application.persistentDataPath + "/save.json";

        LoadData();
    }

    public void SaveData()
    {
        var playerPath = SnakeManager.Instance.playerPath;

        List<Orb> orbs = GameManager.Instance.GetAllCollectableOrbs();
        List<Vector3> orbPositions = orbs.Select(o => o.transform.position).ToList();

        List<Prey> preys = GameManager.Instance.GetAllPrey();
        List<Vector3> preyPositions = preys.Select(p => p.transform.position).ToList();

        var orbsCollected = SnakeManager.Instance.collectedOrbs;
        var birdsPurchased = ShopManager.Instance.birdPurchases;
        var collectorPurchases = ShopManager.Instance.collectorPurchases;

        SaveData data = new SaveData();

        //data.playerPath = playerPath;
        //data.OrbPositions = orbPositions.ToArray();
        //data.PreyPositions = preyPositions.ToArray();
        data.orbsCollected = orbsCollected;
        data.birdsPurchased = birdsPurchased;
        data.collectorPurchases = collectorPurchases;

        Debug.Log($"SaveManager: Saving {data}");

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(savePath, json);

        Debug.Log($"SaveManager: Saved data to {savePath}");
    }

    public void LoadData()
    {
        Debug.Log($"SaveManager: Attempting to load data from {savePath}");

        if (System.IO.File.Exists(savePath))
        {
            string json = System.IO.File.ReadAllText(savePath);
            loadedData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"SaveManager: Loaded data: {loadedData}");
        }
        else
        {
            loadedData = null;
            Debug.Log("SaveManager: Failed to load any data");
        }

    }

    public void ApplyLoadedData()
    {
        if (loadedData == null)
            return;

        Debug.Log("SaveManager: Applying loaded data...");

        /*PathBuffer playerPath = loadedData.playerPath;

        if (playerPath == null || playerPath.Count == 0)
            return;


        var playerPos = playerPath.Get(playerPath.Count - 1);

        Debug.Log($"SaveManager: Player pos {playerPos} from new playerPath of {playerPath.Count} length");

        SnakeManager.Instance.playerPath = playerPath;
        GameManager.Instance.player.transform.position = playerPath.Get(playerPath.Count - 1);

        GameManager.Instance.ClearAllOrbs();

        for (int i = 0; i < loadedData.OrbPositions.Length; i++)
        {
            var opos = loadedData.OrbPositions[i];
            GameManager.Instance.SpawnOrb(opos);
        }

        */
        for (int i = 0; i < loadedData.birdsPurchased; i++)
        {
            GameManager.Instance.SpawnPrey();
        }

        SnakeManager.Instance.DestroySnake();

        for (int i = 0; i < loadedData.orbsCollected; i++)
        {
            GameManager.Instance.GivePlayerOrb();
        }

        ShopManager.Instance.birdPurchases = loadedData.birdsPurchased;
        ShopManager.Instance.collectorPurchases = loadedData.collectorPurchases;

        UIManager.Instance.UpdateUI();

        Debug.Log("SaveManager: Finished applying loaded data");
    }
}
