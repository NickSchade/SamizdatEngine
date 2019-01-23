using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine.WayfarerSettlementsMVP;

public class AgentGenManager : MonoBehaviour {

    public GameObject PanelAgentStats, pfAgentStats;
    private Dictionary<Agent, GameObject> agentStatsUI;
	// Use this for initialization
	void Start () {
        TestAgentStatsPanel();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    void TestAgentStatsPanel()
    {
        agentStatsUI = new Dictionary<Agent, GameObject>();
        List<Agent> agents = GenerateAgents(3);
        foreach (Agent a in agents)
        {
            GameObject go = Instantiate(pfAgentStats, PanelAgentStats.transform);
            go.GetComponent<AgentStatsUI>().InitializeAgentStatsUI(a);
            agentStatsUI[a] = go;
        }

    }
    List<Agent> GenerateAgents(int _n)
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < _n; i++)
        {
            agents.Add(new Agent(i.ToString(), 10f));
        }
        return agents;
    }

    public void ClickAgent()
    {
        Debug.Log("Clicked!");
    }
}
