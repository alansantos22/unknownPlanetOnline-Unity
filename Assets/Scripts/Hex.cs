using UnityEngine;
using UnityEngine.EventSystems;
using UnknownPlanet;
using System.Collections.Generic; // Add this line

namespace UnknownPlanet
{
    public class Hex : MonoBehaviour
    {
        public Vector2Int coordinates;
        public BiomeType biomeType { get; private set; }
        private float noiseValue;
        private ConstructionManager constructionManager;
        private static Hex selectedHex;

        void Start()
        {
            if (GetComponent<Collider2D>() == null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }

            constructionManager = FindObjectOfType<ConstructionManager>();
        }

        public void Initialize(float height, float latitude)
        {
            this.noiseValue = height;
            biomeType = DetermineBiome(height, latitude);
            Debug.Log($"Hex ({coordinates.x}, {coordinates.y}): Bioma={biomeType}, Altura={height:F2}, Latitude={latitude:F2}");
        }

        private BiomeType DetermineBiome(float height, float latitude)
        {
            if (height < 0.3f) return BiomeType.Ocean;

            float temperature = 1.0f - latitude; // 0 = frio (polos), 1 = quente (equador)

            if (height > 0.8f || latitude > 0.8f)
                return BiomeType.Snow;
            if (height > 0.6f)
                return BiomeType.Mountain;
            if (temperature > 0.7f)
                return BiomeType.Desert;
            if (temperature > 0.3f && height > 0.4f)
                return BiomeType.Forest;

            return BiomeType.Plains;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D);
                if (hitCollider != null && hitCollider.gameObject == gameObject)
                {
                    OnTouchDown();
                }
            }
        }

        private void OnTouchDown()
        {
            Debug.Log($"Hex selecionado em ({coordinates.x}, {coordinates.y}):\n" +
                     $"- Bioma: {biomeType}\n" +
                     $"- Altura: {noiseValue:F2}");

            if (constructionManager == null)
            {
                constructionManager = FindObjectOfType<ConstructionManager>();
                if (constructionManager == null) return;
            }

            if (selectedHex != null && selectedHex != this)
            {
                selectedHex.Deselect();
            }

            selectedHex = this;
            // constructionManager.OnHexSelected(this, coordinates);
        }

        private void Deselect()
        {
            selectedHex = null;
        }

        public BiomeType GetBiome()
        {
            Debug.Log($"Hex ({coordinates.x}, {coordinates.y}): {biomeType}");
            return biomeType;
        }
    }
}