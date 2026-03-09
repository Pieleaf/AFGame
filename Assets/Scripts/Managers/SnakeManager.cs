using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SnakeManager : MonoBehaviour
{
    public static SnakeManager Instance;

    public float removePercentOnCollision = 50; // Remove limbs when player hits their own tail

    public float followDistance = 0; // Dist from player
    public float limbSpacing = 1;     // Dist between each limb

    public int ignoreFirstLimbs = 2;
    public int snakeHeadTaperLength = 4;
    public float snakeHeadTaperMinScale = 0.1f;

    public float snakeExtraSpeed = 1f;

    public int maxPathLength = 1000000;

    PlayerController player;

    [ReadOnly]
    public bool active = false;
    [ReadOnly]
    public List<Orb> snake;
    [ReadOnly]
    public PathBuffer playerPath;

    public int collectedOrbs => snake.Count;

    void Awake()
    {
        Instance = this;
        snake = new List<Orb>();

        playerPath = new PathBuffer(maxPathLength);
    }

    private void Start()
    {
        player = GameManager.Instance.player;
        playerPath.Add(player.transform.position);
        UIManager.Instance.UpdateUI();

        Debug.Log($"SnakeManager Start() with player: {player}");
    }

    public void AddLimb(Orb orb)
    {
        if (orb == null || snake.Contains(orb))
            return;

        snake.Add(orb);

        orb.ChangeState(OrbState.InSnakeHidden);
        orb.transform.parent = transform;

        if (snake.Count <= ignoreFirstLimbs)
            orb.isHarmlessLimb = true;
        else
            orb.isHarmlessLimb = false;

        active = true;
        UIManager.Instance.UpdateUI();
    }

    void Update()
    {
        UpdatePlayerPath();
        UpdateSnake();

        if (player.state == WitchState.Tumbling)
            return;

        var playerSpeed = player.speed;
        var snakeSpeed = (snakeExtraSpeed + Mathf.Abs(playerSpeed)) * Time.deltaTime;

        foreach (Orb orb in snake)
        {
            if (orb.state != OrbState.InSnake)
                continue;

            orb.transform.position = Vector3.MoveTowards(
                orb.transform.position, playerPath[orb.pathIndex], snakeSpeed);
        }
    }

    private void UpdatePlayerPath()
    {
        if (playerPath == null || playerPath.IsNull())
            playerPath = new PathBuffer(maxPathLength);

        if (!player) return;

        var playPos = player.transform.position;

        if (playerPath.Count > 0)
        {
            var lastPos = playerPath[playerPath.Count - 1];

            if (RoughDist(playPos, lastPos) < 0.25)
            {
                return;
            }
        }
        playerPath.Add(playPos);
    }

    private void UpdateSnake()
    {
        if (!active || playerPath == null || playerPath.Count == 0)
            return;

        // Update position for each limb based on player path buffer:

        for (int i = 0; i < snake.Count; i++)
        {
            Orb limb = snake[i];

            //if (limb.state == OrbState.InSnakeHidden)
              //  continue;

            // Find position based on index with playerpath:

            float neededDistance = 0f;
            int nextLimbPathIndex = 0;

            if (i == 0) // First limb
            {
                neededDistance = limb.targetRadius * limb.shrinkage + followDistance;
                nextLimbPathIndex = playerPath.Count - 1; // Most recent addition; player position
            }
            else // Other limbs
            {
                Orb nextLimb = snake[i - 1];

                if (nextLimb.state != OrbState.InSnake)
                {
                    limb.ChangeState(OrbState.InSnakeHidden);
                    continue;
                }

                Assert.IsTrue(nextLimb.pathIndex >= 0 && nextLimb.pathIndex < playerPath.Count,
                    $"Invalid pathIndex ({nextLimb.pathIndex}) for orb {i - 1}: {nextLimb}");

                neededDistance = (limb.targetRadius * limb.shrinkage 
                    + nextLimb.targetRadius * limb.shrinkage 
                    + limbSpacing * limb.shrinkage);
                nextLimbPathIndex = nextLimb.pathIndex;
            }

            // Apply position to limb:

            int newPathI = playerPath.MoveDistance(nextLimbPathIndex, neededDistance);

            if (newPathI < 0) // No spot found
            {
                limb.ChangeState(OrbState.InSnakeHidden);
                continue;
            }

            limb.pathIndex = newPathI;
            if (limb.state != OrbState.InSnake)
            {
                limb.transform.position = playerPath[limb.pathIndex];
                limb.ChangeState(OrbState.InSnake);
            }

            // Shrink limbs closer to player
            if (i <= snakeHeadTaperLength - 1)
            {
                var shrinkage = Mathf.Lerp(
                    snakeHeadTaperMinScale,
                    1f,
                    (float)i / (snakeHeadTaperLength - 1));
                limb.SetShrinkage(shrinkage);
            }
        }
    }

    // 2-4x speed of Vector3. Not really needed unless done 10K times per frame
    float RoughDist(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y) + Mathf.Abs(pos1.z - pos2.z);
    }

    internal void DestroySnake()
    {
        Debug.Log($"DestroySnake()");

        foreach (var limb in snake)
        {
            limb.Destroy();
        }
        snake.Clear();
        active = false;

        UIManager.Instance.UpdateUI();
    }

    internal void SnakeHit()
    {
        if (snake.Count == 0)
            return;

        RemoveLimbs(Mathf.CeilToInt(snake.Count * removePercentOnCollision / 100f));
    }

    public void RemoveLimbs(int n)
    {
        if (n >= snake.Count)
        {
            DestroySnake();
            return;
        }
        var targetCount = snake.Count - n;
        for (int i = snake.Count - 1; snake.Count > targetCount && i >= 0; i--)
        {
            Orb orb = snake[i];
            snake.RemoveAt(i);
            orb.Destroy();
        }

        UIManager.Instance.UpdateUI();
    }
}
