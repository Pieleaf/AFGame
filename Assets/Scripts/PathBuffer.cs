using System;
using UnityEngine;

[Serializable]
public class PathBuffer
{
    private PathPos[] buffer;
    private int addingIndex = 0;
    private int count = 0;

    public int Count => count;
    public int Capacity => buffer.Length;

    public PathBuffer(int capacity)
    {
        buffer = new PathPos[capacity];
    }

    public void Add(Vector3 point)
    {
        float distToLast = 0;

        if (addingIndex > 0)
            distToLast = Vector3.Distance(point, Get(addingIndex - 1)); // Cache distance

        buffer[addingIndex] = new PathPos(point, distToLast);
        addingIndex = (addingIndex + 1) % buffer.Length;

        if (count == buffer.Length - 1)
            Debug.LogWarning($"{this} hit capacity {buffer.Length}");

        if (count < buffer.Length)
            count++;
    }

    private int IndexOf(int i)
    {
        if (i < 0 || i >= count)
            throw new IndexOutOfRangeException($"Index ({i}) out of range: [0, {count}]");

        return (addingIndex - count + i + buffer.Length) % buffer.Length;
    }
    public Vector3 Get(int i)
    {
        // 0 = oldest, Count-1 = newest
        return buffer[IndexOf(i)].pos;
    }
    private PathPos GetEntry(int i)
    {
        // 0 = oldest, Count-1 = newest
        return buffer[IndexOf(i)];
    }

    public Vector3 this[int i]
    {
        get
        {
            return Get(i);
        }
    }

    public Vector3[] ToArray()
    {
        Vector3[] result = new Vector3[count];
        for (int i = 0; i < count; i++)
            result[i] = Get(i);
        return result;
    }

    public int MoveDistance(int startIndex, float distance, bool goForward = false)
    {
        // Moves along path buffer from startIndex by distance, returns index of closest position

        //Debug.Log($"MoveDistance({startIndex}, {distance}, {goForward})");
        if (startIndex < 0)
        {
            //Debug.Log($"MoveDistance({startIndex}, {distance}, {goForward}) -> -1");
            return -1;
        }

        float traveledDistance = 0f;

        int foundPos = -1;

        // Buffer is chronological, so higher index is newer; positive is forward:
        if (goForward)
        {
            for (int i = startIndex + 1; i < count; i++)
            {
                var distFromLast = GetEntry(i).distFromLast;
                traveledDistance += distFromLast;

                if (traveledDistance >= distance)
                {
                    foundPos = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = startIndex - 1; i >= 0; i--)
            {
                var distToNext = GetEntry(i+1).distFromLast;
                traveledDistance += distToNext;

                if (traveledDistance >= distance)
                {
                    foundPos = i;
                    break;
                }
            }
        }

        //if (traveledDistance >= distance)
          //  Debug.Log($"MoveDistance({startIndex}, {distance}, {goForward}) -> {foundPos}");

        return foundPos;
    }

    internal bool IsNull()
    {
        return buffer == null;
    }
}

public struct PathPos
{
    public Vector3 pos;
    public float distFromLast;

    public PathPos(Vector3 pos, float distToLast)
    {
        this.pos = pos;
        this.distFromLast = distToLast;
    }
}