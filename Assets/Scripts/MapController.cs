using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    [Header("Размер карты (в гексах)")]
    public int width = 512;
    public int height = 512;

    [Header("Визуализация гексов")]
    public int hexSpriteSize = 64;

    [Header("Настройки шума")]
    public float scale = 20f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Пороги типов местности")]
    [Range(0, 1)] public float waterThreshold = 0.3f;
    [Range(0, 1)] public float plainsThreshold = 0.55f;
    [Range(0, 1)] public float forestThreshold = 0.75f;
    [Range(0, 1)] public float hillsThreshold = 0.88f;

    [Header("Компоненты")]
    public Tilemap groundTilemap;
    public Grid grid;
    public TerrainPalette colorPalette;
    public TileClickHandler tileClickHandler;

    private TerrainEnum[,] tiles;
    private Dictionary<TerrainEnum, Tile> cachedTiles;

    private float noiseOffsetX;
    private float noiseOffsetY;

    private void OnValidate()
    {
        scale = Mathf.Max(scale, 0.001f);

        plainsThreshold = Mathf.Max(plainsThreshold, waterThreshold);
        forestThreshold = Mathf.Max(forestThreshold, plainsThreshold);
        hillsThreshold = Mathf.Max(hillsThreshold, forestThreshold);
    }

    private void Start()
    {
        tileClickHandler.TileClicked += OnTileClicked;
        noiseOffsetX = Random.Range(-10000f, 10000f);
        noiseOffsetY = Random.Range(-10000f, 10000f);

        StartCoroutine(GenerateMapCoroutine());
    }

    private IEnumerator GenerateMapCoroutine()
    {
        tiles = new TerrainEnum[width, height];

        InitTileCache();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float heightValue = GetPerlinHeight(x, y);
                tiles[x, y] = HeightToTerrain(heightValue);
            }

            if (x % 32 == 0)
                yield return null;
        }

        yield return StartCoroutine(RenderToTilemapCoroutine());
    }

    private void InitTileCache()
    {
        cachedTiles = new Dictionary<TerrainEnum, Tile>();

        foreach (TerrainEnum terrain in System.Enum.GetValues(typeof(TerrainEnum)))
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();

            tile.sprite = CreateHexSprite(
                colorPalette.GetColor(terrain),
                hexSpriteSize);

            tile.name = $"Tile_{terrain}";

            cachedTiles.Add(terrain, tile);
        }
    }

    private Sprite CreateHexSprite(Color color, int size)
    {
        Texture2D texture = new(
            size,
            size,
            TextureFormat.RGBA32,
            false)
        {
            filterMode = FilterMode.Point
        };

        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - center;

                pixels[y * size + x] =
                    IsPointInHexagon(p, radius)
                    ? color
                    : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size);
    }

    private bool IsPointInHexagon(Vector2 p, float r)
    {
        float x = Mathf.Abs(p.x);
        float y = Mathf.Abs(p.y);

        if (x > r || y > r)
            return false;

        return (x * 0.5f + y * 0.8660254f) <= r;
    }

    private float GetPerlinHeight(float x, float y)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float heightValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX =
                (x + noiseOffsetX) / scale * frequency;

            float sampleY =
                (y + noiseOffsetY) / scale * frequency;

            float perlin =
                Mathf.PerlinNoise(sampleX, sampleY);

            heightValue += perlin * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float maxPossible;

        if (Mathf.Approximately(persistence, 1f))
        {
            maxPossible = octaves;
        }
        else
        {
            maxPossible =
                (1f - Mathf.Pow(persistence, octaves))
                / (1f - persistence);
        }

        return Mathf.Clamp01(heightValue / maxPossible);
    }

    private TerrainEnum HeightToTerrain(float value)
    {
        if (value < waterThreshold)
            return TerrainEnum.Water;

        if (value < plainsThreshold)
            return TerrainEnum.Plains;

        if (value < forestThreshold)
            return TerrainEnum.Forest;

        if (value < hillsThreshold)
            return TerrainEnum.Hills;

        return TerrainEnum.Mountains;
    }

    private IEnumerator RenderToTilemapCoroutine()
    {
        groundTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                groundTilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    cachedTiles[tiles[x, y]]);
            }

            if (x % 16 == 0)
                yield return null;
        }
    }

    private void OnTileClicked(Vector2Int tilePosition)
    {
        TerrainEnum selectedTile = tiles[tilePosition.x, tilePosition.y];

        Debug.Log($"Pressed: {selectedTile}, Coords: {tilePosition.x} , {tilePosition.y}");
    }
}