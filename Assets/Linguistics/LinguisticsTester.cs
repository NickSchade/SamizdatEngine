using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine.Linguistics;

public class LinguisticsTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
        int iter = 5;
        for (int j = 0; j < iter; j++)
        {
            List<string> words = Linguistics.GenerateWordsBasic(5);
            for (int i = 0; i < words.Count; i++)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
            Debug.Log(string.Join(",", words.ToArray()));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
