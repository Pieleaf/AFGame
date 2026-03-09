using UnityEngine;

[CreateAssetMenu(fileName = "OrbBackup", menuName = "Scriptable Objects/OrbBackup")]
public class OrbBackup : ScriptableObject
{
    public VisualSettings[] colorSelection;
    public VisualSettings collisionColor;
}
