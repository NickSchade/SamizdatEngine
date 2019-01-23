using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



namespace SamizdatEngine.WayfarerSettlementsMVP
{
    public enum Stat { SCOUT, BUILD, EXTRACT, TRADE };
    public enum Ability { };
    public class Agent
    {
        public static List<Stat> GetAllStats()
        {
            return new List<Stat> { Stat.SCOUT, Stat.BUILD, Stat.EXTRACT, Stat.TRADE };
        }
        public Dictionary<Stat, float> stats;
        public Ability ability;
        public string name;
        public Color color;
        public Agent(string _name, float _power)
        {
            stats = InitializeStatsRandomDiscrete(_power);
            name = _name;
            color = new Dictionary<string, Color>{ { "0" ,Color.red}, { "1", Color.blue}, { "2", Color.green}  }[name];
            Debug.Log(Describe());
        }
        public string Describe()
        {
            string d = name + " - ";
            List<string> statstring = new List<string>();
            foreach (Stat s in GetAllStats())
            {
                statstring.Add("[" + s.ToString() + ":" + (Mathf.Round(stats[s])).ToString() + "]");
            }
            d += string.Join(",", statstring.ToArray());
            return d;
        }
        public Dictionary<Stat, float> InitializeStatsRandomContinuous(float _power)
        {
            Dictionary<Stat, float> initStats = new Dictionary<Stat, float>();
            List<Stat> allStats = GetAllStats().OrderBy(x => Random.value).ToList();
            for (int i = 0; i < allStats.Count; i++)
            {
                float level = i < allStats.Count - 1 ? Random.Range(0f, _power) : _power;
                _power -= level;
                initStats[allStats[i]] = level;
            }
            return initStats;
        }
        public Dictionary<Stat, float> InitializeStatsRandomDiscrete(float _power)
        {
            List<Stat> setStats = GetAllStats().OrderBy(x => Random.value).ToList(); ;
            float[] statStrength = new float[setStats.Count];
            
            for (int i = 0; i < setStats.Count; i++)
            {
                _power = i < setStats.Count - 1 ? _power / 2f : _power;
                statStrength[i] = _power;
            }
            Dictionary<Stat, float> initStats = new Dictionary<Stat, float>();
            for (int i = 0; i < setStats.Count; i++)
            {
                initStats[setStats[i]] = statStrength[i];
            }
            return initStats;
        }
    }
}

