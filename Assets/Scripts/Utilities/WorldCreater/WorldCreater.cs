using System;
using UnityEngine;

namespace Utilities.WorldCreater
{
    public class WorldCreater
    {
        public readonly string Seed;
        private readonly float _heightSeed;
        private readonly float _precipitationSeed;
        private readonly float[] HEIGHT_AMP_PARAM = { 60, 36, 4 };
        private readonly float[] HEIGHT_FREQ_PARAM = { 0.01f, 0.02f, 0.1f};
        private readonly float[] PRECIPITATION_AMP_PARAM = { 60, 36, 4 };
        private readonly float[] PRECIPITATION_FREQ_PARAM = { 0.02f, 0.1f, 1f};
        private const float OCEAN_HEIGHT = 50;
        private const float MOUNTAIN_HEIGHT = 60;
        private const float DESERT_PRECIPITATION = 40;
        private const float FOREST_PRECIPITATION = 60;
        public WorldCreater(string seed)
        {
            Seed = seed;
            _heightSeed = GenerateFloatsFromString(seed, 3)[0];
            _precipitationSeed = GenerateFloatsFromString(seed, 3)[1];
        }

        public float[] GenerateFloatsFromString(string input, int count)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                float[] floats = new float[count];
                for (int i = 0; i < count; i++)
                {
                    uint num = BitConverter.ToUInt32(hash, i * 4 % hash.Length);
                    floats[i] = num / (float)uint.MaxValue;
                }
                return floats;
            }
        }

        private WorldTileType ChooseTile(float height, float precipitation)
        {
            if (height < OCEAN_HEIGHT)
                return WorldTileType.Ocean;
            else if (height >= MOUNTAIN_HEIGHT)
                return WorldTileType.Mountain;
            else if (precipitation < DESERT_PRECIPITATION)
                return WorldTileType.Desert;
            else if (precipitation < FOREST_PRECIPITATION)
                return WorldTileType.Grass;
            else
                return WorldTileType.Forest;
        }

        public float GetNoiseAtPosition(Vector2 position, float seed, float[] ampParam, float[] freqParam)
        {
            float nx = position.x;
            float ny = position.y;
            float noise = 0;
            for (int i = 0; i < ampParam.Length; i++)
            {
                noise += Mathf.PerlinNoise(nx * freqParam[i] + seed, ny * freqParam[i] + seed) * ampParam[i];
            }
            return noise;
        }

        public WorldTileType GetTile(Vector2 position)
        {
            float height = GetNoiseAtPosition(position, _heightSeed, HEIGHT_AMP_PARAM, HEIGHT_FREQ_PARAM);
            float precipitation = GetNoiseAtPosition(position, _precipitationSeed, PRECIPITATION_AMP_PARAM, PRECIPITATION_FREQ_PARAM);
            return ChooseTile(height, precipitation);
        }
    }
}