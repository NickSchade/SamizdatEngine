using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SamizdatEngine.WayfarerSettlementsMVP
{
    public class AgentStatsUI : MonoBehaviour
    {
        public GameObject Icon, StatsPanelName, StatsPanelValues, pfText;
        private Dictionary<Stat, GameObject> statsName, statsValue;
        private GameObject iconText, abilityTextName, abilityTextDescription;
        private Agent agent;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Click()
        {
            Debug.Log("Clicked "+agent.Describe());
        }

        public void InitializeAgentStatsUI(Agent _agent)
        {
            agent = _agent;
            Icon.AddComponent<Button>();
            Icon.GetComponent<Button>().onClick.AddListener(() => Click());
            Icon.GetComponent<Image>().color = agent.color;
            statsName = new Dictionary<Stat, GameObject>();
            statsValue = new Dictionary<Stat, GameObject>();
            iconText = Instantiate(pfText, Icon.transform);
            List<Stat> theseStats = Agent.GetAllStats();
            foreach (Stat s in theseStats)
            {
                statsName[s] = Instantiate(pfText, StatsPanelName.transform);
                statsValue[s] = Instantiate(pfText, StatsPanelValues.transform);
                statsName[s].GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                statsValue[s].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            }
            abilityTextName = Instantiate(pfText, StatsPanelName.transform);
            abilityTextName.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            abilityTextDescription = Instantiate(pfText, StatsPanelValues.transform);
            abilityTextDescription.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            UpdateAgentStatsUI();
            
        }
        public void UpdateAgentStatsUI()
        {
            iconText.GetComponent<Text>().text = "Icon";
            Text t = abilityTextName.GetComponent<Text>();
            t.text = "Ability: ";
            t.fontStyle = FontStyle.Italic;
            Text t2 = abilityTextDescription.GetComponent<Text>();
            t2.text = "-";
            t2.fontStyle = FontStyle.Italic;
            List<Stat> theseStats = Agent.GetAllStats();
            foreach (Stat s in theseStats)
            {
                float f = 0f;
                if (agent.stats.ContainsKey(s))
                {
                    f = agent.stats[s];
                }
                statsName[s].GetComponent<Text>().text = s.ToString() + ": ";
                statsValue[s].GetComponent<Text>().text = Mathf.Round(f).ToString();
            }
        }

    }

}
