using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; internal set; }

    public bool purchasesRemoveLimbs = true;

    public int[] birdCosts = new int[] { 10, 20, 40, 60, 100, 150, 200, 300, 400, 500, 600, 800, 1000};
    public int[] collectorUpgradeCosts = new int[] { 20, 100, 300, 600, 1000 };

    public int birdPurchases = 0;
    public int collectorPurchases = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        UIManager UIMan = UIManager.Instance;

        if (birdPurchases < birdCosts.Length)
            UIMan.ShopBirdCost.text = $"{birdCosts[birdPurchases]}";
        else
            UIMan.ShopBirdCost.text = $"X";

        if (collectorPurchases < collectorUpgradeCosts.Length)
            UIMan.ShopCollectionCost.text = $"{collectorUpgradeCosts[collectorPurchases]}";
        else
            UIMan.ShopCollectionCost.text = $"X";

        UIMan.ShopBirdButton.interactable = CanPurchaseBird();
        UIMan.ShopCollectionButton.interactable = CanUpgradeCollector();
    }

    private bool CanUpgradeCollector()
    {
        return (collectorPurchases < collectorUpgradeCosts.Length
            && collectorUpgradeCosts[collectorPurchases] <= SnakeManager.Instance.collectedOrbs);
    }

    private bool CanPurchaseBird()
    {
        return (birdPurchases < birdCosts.Length
            && birdCosts[birdPurchases] <= SnakeManager.Instance.collectedOrbs);
    }

    internal void PurchaseBird()
    {
        if (!CanPurchaseBird())
            return;

        var cost = birdCosts[birdPurchases];
        if (purchasesRemoveLimbs)
            SnakeManager.Instance.RemoveLimbs(cost);

        birdPurchases++;

        GameManager.Instance.SpawnPrey();
        SFXPlayer.Instance.PlayPurchase();
        UIManager.Instance.UpdateUI();
    }

    internal void UpgradeCollector()
    {
        if (!CanUpgradeCollector())
            return;

        var cost = collectorUpgradeCosts[collectorPurchases];
        if (purchasesRemoveLimbs)
            SnakeManager.Instance.RemoveLimbs(cost);

        collectorPurchases++;

        GameManager.Instance.player.collector.UpgradeRange();
        SFXPlayer.Instance.PlayPurchase();
        UIManager.Instance.UpdateUI();
    }
}
