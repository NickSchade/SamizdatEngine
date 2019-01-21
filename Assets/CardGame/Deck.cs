using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface Card
{
    void EffectWhenDrawn();
    void EffectWhenPlayed();
    void EffectWhenDiscarded();
    GameObject Display();
}


public class Deck {

    List<Card> deck;
    List<Card> hand;
    List<Card> library;
    List<Card> discard;
    int handMax = 0; // 0 means no max
    int handDraw = 0; // 0 means draw up to max
    int handStart = 5;

	public Deck(List<Card> _deck, int _handMax, int _handDraw, int _handStart)
    {
        handMax = _handMax;
        handDraw = _handDraw;
        handStart = _handStart;
        StartDeck(_deck);
    }
    void StartDeck(List<Card> _deck)
    {
        deck = _deck;
        library = deck;
        discard = new List<Card>();
        StartHand();
    }
    void StartHand()
    {
        hand = new List<Card>();
        DrawCards(handStart);
    }
    int CardsToDiscard()
    {
        int discardNumber = 0;
        if (handMax != 0 && hand.Count > handMax)
        {
            discardNumber = hand.Count - handMax;
            Debug.Log("MUST DISCARD " + discardNumber + " CARDS");
        }
        return discardNumber;
    }
    void DrawStep()
    {
        if (handMax == 0)
        {
            // no max
            DrawCards(handDraw);
        }
        else
        {
            DrawCards(handMax - hand.Count);
        }
    }
    void DrawCards(int n)
    {
        if ( n > 0)
        {
            for (int i = 0; i < n; i++)
            {
                DrawCard();
            }
        }
    }
    void DrawCard()
    {
        Card drawn = library[0];
        library.Remove(drawn);
        hand.Add(drawn);
        drawn.EffectWhenDrawn();
    }

    public static List<Card> Shuffle(List<Card> aList)
    {

        System.Random _random = new System.Random();

        Card myCard;

        int n = aList.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            myCard = aList[r];
            aList[r] = aList[i];
            aList[i] = myCard;
        }

        return aList;
    }

}
