using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
using System;

public enum DisplayView { Basic, Natural, Biome,
    Elevation, Landmass, Downhill,
    Rain, WaterFlux, Temperature, Flatness,
    WindMagnitude, WindDirection, WindVector,
    ResourceTotal, Fertility, Harbor, Regions};

public class MapDrawer : MonoBehaviour
{
    public GameObject goTile;
    public MapGen mapGen;
    public Dictionary<MapLoc, GameObject> Tiles;
    public DisplayView displayView;
    
    static int d = 6;
    static int iExpand = 2;
    float tileSpread = 1.1f;
    Dictionary<float, Color> cdictNatural;
    Dictionary<float, Color> cdictSpectrum;
    
    // Use this for initialization
    void Start()
    {
        Tiles = new Dictionary<MapLoc, GameObject>();
    }
    
    // BUTTON FUNCTIONS - DISPLAY
    public void btnDisplayWaterFlux()
    {
        DisplayNewView(DisplayView.WaterFlux);
    }
    public void btnDisplayTemperature()
    {
        DisplayNewView(DisplayView.Temperature);
    }
    public void btnDisplayRain()
    {
        DisplayNewView(DisplayView.Rain);
    }
    public void btnDisplayLandmass()
    {
        DisplayNewView(DisplayView.Landmass);
    }
    public void btnDisplayHeight()
    {
        DisplayNewView(DisplayView.Elevation);
    }
    public void btnDisplayWindMagnitude()
    {
        DisplayNewView(DisplayView.WindMagnitude);
    }
    public void btnDisplayWindDirection()
    {
        DisplayNewView(DisplayView.WindDirection);
    }
    public void btnDisplayWindVector()
    {
        DisplayNewView(DisplayView.WindVector);
    }
    public void btnDisplayBasic()
    {
        DisplayNewView(DisplayView.Basic);
    }
    public void btnDisplayNatural()
    {
        DisplayNewView(DisplayView.Natural);
    }
    public void btnDisplayDownhill()
    {
        DisplayNewView(DisplayView.Downhill);
    }
    public void btnDisplayBiome()
    {
        DisplayNewView(DisplayView.Biome);
    }
    public void btnDisplayResourceTotal()
    {
        DisplayNewView(DisplayView.ResourceTotal);
    }
    public void btnDisplayFertility()
    {
        DisplayNewView(DisplayView.Fertility);
    }
    public void btnDisplayRegions()
    {
        DisplayNewView(DisplayView.Regions);
    }
    public void btnDisplayFlatness()
    {
        DisplayNewView(DisplayView.Flatness);
    }
    //BUTTON FUNCTIONS - GENERATION
    public void btnGenerateMap()
    {
        displayView = DisplayView.Biome;
        int td = d - iExpand + 1;
        int Dim = (int)Mathf.Pow(2, td) + 1;
        mapGen = new MapGen(Dim, Dim, iExpand);
        cdictSpectrum = MapColor.PaletteBasicSpectrum(0f);
        mapGen.GenerateMap();
        AdjustCamera(mapGen.xDim, mapGen.yDim);
        MapScaler ms = new MapScaler();
        for (int i = 0; i < iExpand; i++)
        {
            ms.Smooth(ref mapGen);
        }
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }

    public void btnCreateElevation()
    {
        mapGen.CreateElevation();
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }
    public void btnApplyWaterBodyPreferences()
    {
        mapGen.ApplyPreferencesWaterBody();
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }
    public void btnElevationAdjustments()
    {
        mapGen.CreateElevationAdjustments();
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }
    public void btnCreateRain()
    {
        mapGen.CreateRain(RainMethod.Wind);
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }
    public void btnErosion()
    {
        mapGen.CreateFluxErosion(3,30);
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
    }
    public void btnExpand()
    {
        MapScaler ms = new MapScaler();
        ms.ExpandSquare(ref mapGen, 2, 2);
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayTilesInit();
        DisplayMap(displayView);
        AdjustCamera(mapGen.xDim, mapGen.yDim);
    }
    public void btnSmooth()
    {
        MapScaler ms = new MapScaler();
        ms.Smooth(ref mapGen);
        cdictNatural = MapColor.PaletteBasicNatural(mapGen.seaLevel);
        DisplayMap(displayView);
    }
    

    // DISPLAYVIEW FUNCTIONS
    void AdjustCamera(int xDim, int yDim)
    {
        float xl = xDim * (tileSpread + 1f);
        float yl = xDim * (tileSpread + 1f);
        Camera.main.transform.position = new Vector3(xl/4, 10, yl/4);
        Camera.main.orthographicSize = Mathf.Max(xl,yl)/3;
    }
    void DisplayMap(DisplayView displayView)
    {
        if (displayView == DisplayView.Elevation)
        {
            DisplayMapWorkhorse(ColorElevationSimple);
        }
        else if (displayView == DisplayView.Landmass)
        {
            DisplayMapWorkhorse(ColorLandmass);
        }
        else if (displayView == DisplayView.Basic)
        {
            DisplayMapWorkhorse(ColorBasic);
        }
        else if ( displayView == DisplayView.Rain)
        {
            DisplayMapWorkhorse(ColorRain);
        }
        else if (displayView == DisplayView.Temperature)
        {
            DisplayMapWorkhorse(ColorTemperature);
        }
        else if (displayView == DisplayView.WaterFlux)
        {
            DisplayMapWorkhorse(ColorWaterFlux);
        }
        else if (displayView == DisplayView.WindMagnitude)
        {
            DisplayMapWorkhorse(ColorWindMagnitude);
        }
        else if (displayView == DisplayView.Natural)
        {
            DisplayMapWorkhorse(ColorNatural);
        }
        else if (displayView == DisplayView.Downhill)
        {
            DisplayMapWorkhorse(ColorDownhill);
        }
        else if (displayView == DisplayView.WindDirection)
        {
            DisplayMapWorkhorse(ColorWindDirection);
        }
        else if (displayView == DisplayView.Biome)
        {
            DisplayMapWorkhorse(ColorBiome);
        }
        else if (displayView == DisplayView.WindVector)
        {
            DisplayMapWorkhorse(ColorWindVector);
        }
        else if (displayView == DisplayView.ResourceTotal)
        {
            //DisplayMapWorkhorse(ColorResourceTotal);
        }
        else if (displayView == DisplayView.Fertility)
        {
            DisplayMapWorkhorse(ColorFertility);
        }
        else if (displayView == DisplayView.Regions)
        {
            DisplayMapWorkhorse(ColorRegions);
        }
        else if (displayView == DisplayView.Flatness)
        {
            DisplayMapWorkhorse(ColorFlatness);
        }
    }
    void DisplayMapWorkhorse(ColorGo cf)
    {
        for (int x = 0; x < mapGen.xDim; x++)
        {
            for (int y = 0; y < mapGen.yDim; y++)
            {
                cf(new MapLoc(x, y));
            }
        }
    }
    void DisplayTilesInit()
    {
        for (int x = 0; x < mapGen.xDim; x++)
        {
            for (int y = 0; y < mapGen.yDim; y++)
            {
                Vector3 pos = new Vector3(tileSpread * x, 0, tileSpread * y);
                MapLoc l = new MapLoc(x, y);
                if (!Tiles.ContainsKey(l))
                {
                    Tiles[l] = Instantiate(goTile, pos, Quaternion.identity);
                }
            }
        }
    }
    public void DisplayNewView(DisplayView newView, int waitSeconds = 1)
    {
        displayView = newView;
        DisplayMap(displayView);
    }
    // COLOR FUNCTIONS
    delegate void ColorGo(MapLoc l);
    void ResetTile(GameObject go)
    {
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = "";
            Transform tmr = GetComponentInParent<Transform>();
            tmr.eulerAngles = new Vector3(90, 0, 0);
            tmr.localScale = new Vector3(1, 1, 1);
        }
        
    }
    void ColorBasic(MapLoc l)
    {
        float arableFrac = 2f; //2.5f
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Elevation[l.x, l.y];
        Color c = ColorCutoff(e, mapGen.seaLevel, Color.black, Color.green);
        float f = mapGen.WaterFlux[l.x, l.y];
        c = (f > mapGen.riverLevel / arableFrac && e > mapGen.seaLevel) ? Color.yellow : c; // a rough representation of arable land
        c = (f > mapGen.riverLevel && e > mapGen.seaLevel) ? Color.blue : c;
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorElevationSimple(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Elevation[l.x, l.y];
        Color c = ColorBasic(e, 0f, Color.black, Color.green);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = (10 * Math.Round(e, 1)).ToString();
            tm.color = Color.white;
        }
        
    }
    void ColorRain(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Rain[l.x, l.y];
        Color c = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? ColorBasic(e, 0f, Color.black, Color.blue) : ColorBasic(e, 0f, Color.white, Color.blue);

        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorTemperature(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Temperature[l.x, l.y];
        Color c = ColorBasic(e, 0f, Color.blue, Color.red);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorWindMagnitude(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.WindMagnitude[l.x, l.y];
        Color c = ColorBasic(e, 0f, Color.white, Color.black);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorWaterFlux(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.WaterFlux[l.x, l.y];
        Color c = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? ColorBasic(e, 0f, Color.black, Color.blue) : ColorBasic(e, 0f, Color.white, Color.blue);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorNatural(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Elevation[l.x, l.y];
        float r = mapGen.WaterFlux[l.x, l.y];
        Color c = MapColor.GetColorContinuous(e, cdictNatural);
        c = (r > mapGen.riverLevel && e > mapGen.seaLevel) ? MapColor.GetColorLerp(0.5f,Color.cyan, Color.blue) : c;
        //c = (r > mapGen.riverLevel && e > mapGen.seaLevel) ? Color.cyan : c;
        c = (mapGen.Temperature[l.x, l.y] < mapGen.iceLevel) ? Color.white : c;
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorSpectrum(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.WaterFlux[l.x, l.y];
        Color c = MapColor.GetColorContinuous(e, cdictSpectrum);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorDownhill(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        Color cb = Color.black;
        Color ct = Color.white;
        if (mapGen.Elevation[l.x, l.y] > mapGen.seaLevel)
        {
            cb = Color.white;
            ct = Color.black;
        }
        go.GetComponent<Renderer>().material.SetColor("_Color", cb);
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = '\u2192'.ToString();
            tm.color = ct;
            Vec v = mapGen.Downhill[l.x, l.y];
            float z = 0f;
            float z2 = 90f;
            float d = 0f;
            if (!v.Equals(null))
            {
                d = MapUtil.VectorToRadians(v.x, v.y);
                d = MapUtil.RadiansToDegrees(d);
                z = 90f;
                z2 = 0f;
            }
            Debug.Log("[" + l.x + "," + l.y + "] is " + z + "," + d);
            tm.GetComponentInParent<Transform>().eulerAngles = new Vector3(z, d, z2);
        }
        
    }
    void ColorWindDirection(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        Color cb = Color.black;
        Color ct = Color.white;
        if (mapGen.Elevation[l.x, l.y] > mapGen.seaLevel)
        {
            cb = Color.white;
            ct = Color.black;
        }
        go.GetComponent<Renderer>().material.SetColor("_Color", cb);
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = '\u2192'.ToString();
            tm.color = ct;
            Vec v = mapGen.WindVector[l.x, l.y];
            float z = 0f;
            float z2 = 90f;
            float d = 0f;
            if (!v.Equals(null))
            {
                d = MapUtil.VectorToRadians(v.x, v.y);
                d = MapUtil.RadiansToDegrees(d);
                z = 90f;
                z2 = 0f;
            }
            //Debug.Log("[" + l.x + "," + l.y + "] is ["+v.x+","+v.y+"]");
            tm.GetComponentInParent<Transform>().eulerAngles = new Vector3(z, d, z2);
        }
    }
    void ColorWindVector(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        Color cb = Color.black;
        Color ct = Color.white;
        if (mapGen.Elevation[l.x, l.y] > mapGen.seaLevel)
        {
            cb = Color.white;
            ct = Color.black;
        }
        go.GetComponent<Renderer>().material.SetColor("_Color", cb);
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = '\u2192'.ToString();
            tm.color = ct;
            float m = (float)Math.Pow(mapGen.WindMagnitude[l.x, l.y], 2);
            Vec v = mapGen.WindVector[l.x, l.y];
            float z = 0f;
            float z2 = 90f;
            float d = 0f;
            if (!v.Equals(null))
            {
                d = MapUtil.VectorToRadians(v.x, v.y);
                d = MapUtil.RadiansToDegrees(d);
                z = 90f;
                z2 = 0f;
            }
            //Debug.Log("[" + l.x + "," + l.y + "] is ["+v.x+","+v.y+"]");
            Transform tmr = tm.GetComponentInParent<Transform>();
            tmr.eulerAngles = new Vector3(z, d, z2);
            tmr.localScale = new Vector3(m, m, m);
            if (m > 1.0)
            {
                Debug.Log(l.x + "," + l.y + " is " + m);
            }
        }
    }
    void ColorLandmass(MapLoc l)
    {
        GameObject go = Tiles[l];
        ResetTile(go);
        float e = mapGen.Elevation[l.x, l.y];
        Color c = ColorCutoff(e, mapGen.seaLevel, Color.blue, Color.green);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorBiome(MapLoc l)
    {
        float r = mapGen.WaterFlux[l.x, l.y];
        float t = mapGen.Temperature[l.x, l.y];
        float e = mapGen.Elevation[l.x, l.y];
        Color orange = MapColor.GetColorLerp(0.5f, Color.red, Color.yellow);
        Color cr = MapColor.GetColorLerp(r, orange, Color.green);
        Color ct = MapColor.GetColorLerp(t, Color.black, Color.white);
        Color c = MapColor.GetColorLerp(0.5f, cr, ct);
        c = e < mapGen.seaLevel ? Color.blue : c;
        c = (mapGen.Temperature[l.x, l.y] < mapGen.iceLevel) ? Color.white : c;
        c = (mapGen.WaterFlux[l.x, l.y] > mapGen.riverLevel) && e > mapGen.seaLevel ? MapColor.GetColorLerp(0.5f, Color.blue, Color.cyan) : c;
        GameObject go = Tiles[l];
        ResetTile(go);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);

    }
    void ColorFertility(MapLoc l)
    {
        Color c = MapColor.GetColorLerp(mapGen.Fertility[l.x,l.y], Color.white, Color.green);
        float e = mapGen.WaterFlux[l.x, l.y];
        c = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? Color.black : c;

        GameObject go = Tiles[l];
        ResetTile(go);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorFlatness(MapLoc l)
    {
        Color c = MapColor.GetColorLerp(mapGen.Elevation[l.x,l.y], Color.white, Color.black);
        //float e = mapGen.WaterFlux[l.x, l.y];
        c = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? Color.blue : c;

        GameObject go = Tiles[l];
        ResetTile(go);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    void ColorRegions(MapLoc l)
    {
        Color c = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? Color.black : Color.white;
        GameObject go = Tiles[l];
        ResetTile(go);
        go.GetComponent<Renderer>().material.SetColor("_Color", c);
        TextMesh tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            tm.text = mapGen.Regions[l.x, l.y].ToString();
            tm.color = mapGen.Elevation[l.x, l.y] < mapGen.seaLevel ? Color.white : Color.black;
        }
    }

    delegate Color ColorFunc(float value, float coeff, Color cLower, Color cUpper);
    Color ColorBasic(float value, float coeff, Color cLower, Color cUpper)
    {
        return MapColor.GetColorLerp(value, cLower, cUpper);
    }
    Color ColorCutoff(float value, float coeff, Color cLower, Color cUpper)
    {
        return MapColor.GetColorCutoff(value, coeff, cLower, cUpper);
    }   
}
