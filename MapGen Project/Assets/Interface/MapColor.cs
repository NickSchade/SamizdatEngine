using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Drawing;

public static class MapColor
{
    static public Color GetColorDiscrete(float value, Dictionary<float,Color> cdict)
    {
        List<float> flevels = new List<float>();
        foreach (KeyValuePair<float,Color> kvp in cdict)
        {
            flevels.Add(kvp.Key);
        }
        flevels.Sort();
        Color rColor = cdict[flevels[0]];
        foreach (float flevel in flevels)
        {
            rColor = cdict[flevel];
            if (value < flevel)
            {
                break;
            }
        }
        return rColor;
    }
    static public Color GetColorContinuous(float value, Dictionary<float, Color> cdict)
    {
        List<float> flevels = new List<float>();
        foreach (KeyValuePair<float, Color> kvp in cdict)
        {
            flevels.Add(kvp.Key);
        }
        flevels.Sort();
        Color rColor = cdict[flevels[0]];
        float flerp;
        for (int i = 0; i < flevels.Count - 1; i++)
        {
            if (value >= flevels[i] && value <= flevels[i + 1])
            {
                flerp = MapUtil.dNormalize(value, flevels[i], flevels[i + 1]);
                rColor = GetColorLerp(flerp, cdict[flevels[i]], cdict[flevels[i + 1]]);
            }
        }
        return rColor;
    }
    static public Color GetColorLerp(float value, Color cLower, Color cUpper)
    {
        float r = Mathf.Lerp(cLower.r, cUpper.r, value);
        float g = Mathf.Lerp(cLower.g, cUpper.g, value);
        float b = Mathf.Lerp(cLower.b, cUpper.b, value);
        Color C = new Color(r, g, b);
        return C;
    }
    static public Color GetColorCutoff(float value, float cutoff, Color cLower, Color cUpper)
    {
        Dictionary<float, Color> CutoffDict = new Dictionary<float, Color>();
        CutoffDict[cutoff] = cLower;
        CutoffDict[1f] = cUpper;
        Color rColor = GetColorDiscrete(value, CutoffDict);
        return rColor;
    }
    
    static public Dictionary<float,Color> PaletteBasicSpectrum(float c)
    {
        Dictionary<float, Color> d = new Dictionary<float, Color>();
        d[0f] = Color.blue;
        d[0.2f] = Color.cyan;
        d[0.4f] = Color.green;
        d[0.6f] = Color.yellow;
        d[0.8f] = Color.red;
        d[1f] = Color.magenta;
        return d;
    }
    static public Dictionary<float,Color> PaletteBasicNatural(float c)
    {
        Dictionary<float, Color> d = new Dictionary<float, Color>();
        d[0f] = Color.blue;
        float sealerp = Mathf.Lerp(0f, c, 0.8f);
        d[sealerp] = Color.blue;
        d[c] = GetColorLerp(0.5f, Color.blue, Color.cyan);
        float shore = c + 0.00001f;
        d[shore] = Color.green;
        float landlerp = Mathf.Lerp(shore, 1f, 0.5f);
        d[landlerp] = Color.yellow;

        float mountain = Mathf.Lerp(shore, 1f, 0.85f);
        d[mountain] = Color.red;
        d[mountain + 0.00001f] = Color.red;
        d[1f] = Color.red;
        return d;
    }
}


