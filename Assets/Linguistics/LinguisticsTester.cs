using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine.Linguistics;

public class LinguisticsTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Starting");
        List<string> words = Linguistics.GenerateWordsBasic(20);
        foreach(string s in words)
        {
            Debug.Log(char.ToUpper(s[0])+s.Substring(1).ToLower());
        }
        Debug.Log("Finished with "+ words.Count + " words");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
