using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainPalette", menuName = "Strategy/Terrain Palette")]
public class TerrainPalette : ScriptableObject
{
    #region Fields

    public List<TerrainType> entities;

    #endregion

    #region Private methods

    private void OnValidate()
    {
        SyncEntriesWithEnum();
    }

    private void SyncEntriesWithEnum()
    {
        var allTypes = System.Enum.GetValues(typeof(TerrainEnum)).Cast<TerrainEnum>().ToList();

        entities.RemoveAll(e => ! allTypes.Contains(e.terrain));

        foreach (var terrainType in allTypes)
        {
            if (!entities.Any(e => e.terrain == terrainType))
            {
                entities.Add(new TerrainType { terrain = terrainType, color = Color.magenta });
            }
        }

        entities = entities.OrderBy(e => (int)e.terrain).ToList();
    }

    #endregion

    #region Public methods

    public Color GetColor(TerrainEnum terrainEnum)
    {
        var entry = entities.Find(e => e.terrain == terrainEnum);
        if (entry.terrain == terrainEnum)
            return entry.color;
        return Color.magenta;
    }

    #endregion
}
