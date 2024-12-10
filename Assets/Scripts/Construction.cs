using UnityEngine;

namespace UnknownPlanet
{
    public enum BiomeType
    {
        Ocean,
        Desert,
        Plains,
        Forest,
        DenseForest,
        Cerrado,
        Mountain,
        Tundra,
        Snow
    }

    [System.Serializable]
    public struct BuildingTypeData
    {
        public BuildingType type;
        public int exclusionRange; // Radius in hex tiles where similar buildings are not allowed
        public Sprite buildingSprite; // Add sprite field
        public Color defaultColor; // Add default color field
        public BiomeType[] allowedBiomes; // Biomas onde pode construir
    }

    [System.Serializable]
    public class Construction
    {
        public string name;
        public BuildingType type;
        public Vector2Int coordinates;
        public GameObject visualPrefab;
    }

    public enum BuildingType
    {
        City,
        Village,
        Farm,
        Mine
    }

    public static class BuildingTypeExtensions
    {
        public static BuildingTypeData GetData(this BuildingType type)
        {
            return new BuildingTypeData
            {
                type = type,
                exclusionRange = GetExclusionRange(type),
                defaultColor = GetDefaultColor(type),
                allowedBiomes = GetAllowedBiomes(type)
            };
        }

        private static int GetExclusionRange(BuildingType type) => type switch
        {
            BuildingType.City => 5,      // Reduzido de 10 para 5
            BuildingType.Village => 3,   // Reduzido de 5 para 3
            BuildingType.Farm => 1,      // Reduzido de 2 para 1
            BuildingType.Mine => 2,      // Reduzido de 3 para 2
            _ => 0
        };

        private static Color GetDefaultColor(BuildingType type) => type switch
        {
            BuildingType.City => Color.red,
            BuildingType.Village => Color.yellow,
            BuildingType.Farm => Color.green,
            BuildingType.Mine => Color.grey,
            _ => Color.white
        };

        private static BiomeType[] GetAllowedBiomes(BuildingType type) => type switch
        {
            BuildingType.City => new[] { BiomeType.Plains, BiomeType.Cerrado },
            BuildingType.Village => new[] { BiomeType.Plains, BiomeType.Forest, BiomeType.DenseForest, BiomeType.Cerrado },
            BuildingType.Farm => new[] { BiomeType.Plains, BiomeType.Cerrado },
            BuildingType.Mine => new[] { BiomeType.Mountain },
            _ => new BiomeType[] { }
        };
    }
}