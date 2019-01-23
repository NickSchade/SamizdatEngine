using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SamizdatEngine.Linguistics
{
    public class Linguistics
    {
        public static List<string> AllConsonants()
        {
            return new List<string>
        {"B","C","D","F","G","H","J","K","L","M","N","P","R","S","T","SH","CH","TH"};
        }
        public static List<string> AllVowels()
        {
            return new List<string>
        {"A","AE","E","EE", "EA", "I", "IA", "AI", "IE", "EI","O","OO", "OA", "AO", "OE", "EO", "OI", "IO", "U", "UE", "UI", "UA", "AU", "OU"};
        }
        public static List<string> SelectSubset(List<string> list, int n)
        {
            List<string> subset = new List<string>();
            int len = list.Count;
            for (int i = 0; i < len; i++)
            {
                float r = Random.Range(0.0f, 1.0f);
                if (r < n / (len - i))
                {
                    subset.Add(list[i]);
                    if (subset.Count == n)
                    {
                        break;
                    }
                }
            }
            return subset;
        }
        public static List<string> GenerateWordsBasic(int nWords)
        {
            int nMorphemes = 50;
            List<string> morphemes = GenerateMorphemes(nMorphemes);
            List<string> words = new List<string>();
            for (int i = 0; i < nWords; i++)
            {
                string newWord = morphemes[Random.Range(0, morphemes.Count)] + morphemes[Random.Range(0, morphemes.Count)];
                words.Add(newWord);
            }
            return words;
        }
        public static List<string> GenerateMorphemes(int nMorphemes)
        {
            int nVowelSounds = 5;
            int nConsonantSounds = 10;
            float pConsonantStart = 0.5f;
            float pConsonantEnd = 0.5f;
            List<string> vowels = SelectSubset(AllVowels(), nVowelSounds);
            List<string> consonants = SelectSubset(AllConsonants(), nConsonantSounds);
            List<string> morphemes = new List<string>();
            for (int i = 0; i < nMorphemes; i++)
            {
                morphemes.Add(GenerateMorpheme(consonants, vowels, pConsonantStart, pConsonantEnd));
            }
            return morphemes;
        }
        public static string GenerateMorpheme(List<string> consonants, List<string> vowels, float pConsonantStart, float pConsonantEnd)
        {
            string morpheme = "";
            float rStart = Random.Range(0.0f, 1.0f);
            float rEnd = Random.Range(0.0f, 1.0f);
            if (rStart < pConsonantStart)
            {
                morpheme += consonants[Random.Range(0, consonants.Count)];
            }
            morpheme += vowels[Random.Range(0, vowels.Count)];
            if (rEnd < pConsonantEnd)
            {
                morpheme += consonants[Random.Range(0, consonants.Count)];
            }
            return morpheme;
        }
    }
}
