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
        public static List<Stat> AllStats = new List<Stat> { Stat.SCOUT, Stat.BUILD, Stat.EXTRACT, Stat.TRADE };
        Dictionary<Stat, float> stats;
        Ability ability;
        public Agent(float power)
        {
            stats = new Dictionary<Stat, float>();
            List<Stat> allStats = AllStats.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < allStats.Count; i++)
            {
                float level = i < allStats.Count - 1 ? Random.Range(0, power) : power;
                power -= level;
                stats[allStats[i]] = level;
            }

        }
    }
}

