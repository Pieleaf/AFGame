
using System;
using System.Drawing;
using System.Net.Mail;
using UnityEngine;

[Serializable]
public class SaveData
{
    /*public PathBuffer playerPath;
    public Vector3[] OrbPositions;
    public Vector3[] PreyPositions;*/

    public int orbsCollected;
    public int birdsPurchased;
    public int collectorPurchases;

    public override string ToString()
    {
        return $"SaveData(\n" +
           // $"playerPath[{playerPath.Count}], \n" +
           // $"OrbPositions[{OrbPositions.Length}], \n" +
           // $"PreyPositions[{PreyPositions.Length}], \n" +
            $"orbsCollected: {orbsCollected}, \n" +
            $"birdsPurchased: {birdsPurchased}, \n" +
            $"collectorPurchases: {collectorPurchases} )";
    }
}
