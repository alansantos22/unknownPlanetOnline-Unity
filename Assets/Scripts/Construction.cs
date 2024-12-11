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
            BuildingType.City => 3,    // Reduzido
            BuildingType.Village => 2, // Reduzido
            BuildingType.Farm => 1,    // Reduzido
            BuildingType.Mine => 1,    // Reduzido
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
            BuildingType.City => new[] { BiomeType.Plains, BiomeType.Cerrado, BiomeType.Tundra },
            BuildingType.Village => new[] { BiomeType.Plains, BiomeType.Cerrado, BiomeType.Tundra },
            BuildingType.Farm => new[] { BiomeType.Plains, BiomeType.Cerrado },
            BuildingType.Mine => new[] { BiomeType.Mountain },
            _ => new BiomeType[] { }
        };
    }
}