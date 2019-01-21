using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{
    public enum bbTerrainType { OCEAN, LAND, MOUNTAIN };
    public enum bbTerrainFeature { ARABLE, MINEABLE, NONE };

    public class bbLand
    {

        float TERRAIN_LEVEL = 0.25f;
        float TERRAIN_RARITY = 1 / 5f;
        float TERRAIN_UNCERTAINTY = 0.1f;

        public bbPos pos;
        public float val;
        public bbTerrainType terrainType;
        public bbTerrainFeature terrainFeature;
        public bbLand(bbPos _pos)
        {
            pos = _pos;
            terrainFeature = bbTerrainFeature.NONE;
        }
        public void setFromValue(float _val)
        {
            val = _val;
            terrainType = bbTerrainType.LAND;
            float t = Random.Range(0.0f, 1.0f);
            if (val < TERRAIN_LEVEL)
            {
                terrainType = bbTerrainType.OCEAN;
            }
            else if (val > (1 - TERRAIN_LEVEL))
            {
                terrainType = bbTerrainType.MOUNTAIN;
            }
            else if (val > 0.5f)
            {
                float thresh = Mathf.Pow(Mathf.InverseLerp((1 - TERRAIN_LEVEL), 0.5f, val), TERRAIN_RARITY) + TERRAIN_UNCERTAINTY;
                if (t > thresh)
                {
                    terrainFeature = bbTerrainFeature.MINEABLE;
                }

            }
            else if (val < 0.5f)
            {
                float thresh = Mathf.Pow(Mathf.InverseLerp(TERRAIN_LEVEL, 0.5f, val), TERRAIN_RARITY) + TERRAIN_UNCERTAINTY;
                if (t > thresh)
                {
                    terrainFeature = bbTerrainFeature.ARABLE;
                }
            }
        }
        public void setRandom()
        {
            float _val = Random.Range(0.0f, 1.0f);
            setFromValue(_val);
        }
    }

}
