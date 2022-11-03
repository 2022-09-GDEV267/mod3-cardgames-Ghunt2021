using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Pyramid : MonoBehaviour
{

    static public Pyramid P;

    [Header("Set in Inspector")]
    public TextAsset deckXML;

    public TextAsset PyramidLayoutXML; //Replace with layoutXML if needed

    public float xOffset = 3;

    public float yOffset = -2.5f;

    public Vector3 layoutCenter;

    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);

    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);

    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);

    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);


    [Header("Set Dynamically")]
    public Deck deck;

    //public Layout layout;

    public LayoutPyramid pLayout;

    public List<CardPyramid> drawPile;

    public Transform layoutAnchor;

    public CardPyramid target;

    public List<CardPyramid> tableau;

    public List<CardPyramid> discardPile;

    public List<CardPyramid> targetPile;


    void Awake()
    {
        P = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);

        Deck.Shuffle(ref deck.cards); //This shuffles the deck by reference //a


        //Comment out this section
        //Card c;
        //
        //for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        //{                    //b
        //
        //	c = deck.cards[cNum];
        //
        //	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //
        //}

        pLayout = GetComponent<LayoutPyramid>();  // Get the Layout component

        pLayout.ReadLayout(PyramidLayoutXML.text); // Pass LayoutXML to it   //Replace with layoutXML if needed

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);

        LayoutGame();
    }

    CardPyramid Draw()
    {

        CardPyramid pd = drawPile[0]; // Pull the 0th CardProspector

        drawPile.RemoveAt(0);            // Then remove it from List<> drawPile

        return (pd);                      // And return it

    }

    // LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"

    void LayoutGame()
    {

        // Create an empty GameObject to serve as an anchor for the tableau // a

        if (layoutAnchor == null)
        {

            GameObject tGO = new GameObject("_LayoutAnchor");

            // ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy

            layoutAnchor = tGO.transform;              // Grab its Transform

            layoutAnchor.transform.position = layoutCenter;   // Position it

        }

        CardPyramid cp;

        // Follow the layout

        foreach (PyramidSlotDef tSD in pLayout.pyramidSlotDefs)
        {

            // ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD

            cp = Draw(); // Pull a card from the top (beginning) of the draw Pile

            cp.faceUp = tSD.faceUp;  // Set its faceUp to the value in SlotDef

            cp.transform.parent = layoutAnchor; // Make its parent layoutAnchor

            // This replaces the previous parent: deck.deckAnchor, which

            //  appears as _Deck in the Hierarchy when the scene is playing.

            cp.transform.localPosition = new Vector3(

                pLayout.multiplier.x * tSD.x,

                pLayout.multiplier.y * tSD.y,

                -tSD.layerID);

            // ^ Set the localPosition of the card based on slotDef

            cp.layoutID = tSD.id;

            cp.pyramidSlotDef = tSD;

            // CardProspectors in the tableau have the state CardState.tableau

            cp.state = eCardState.tableau;

            cp.SetSortingLayerName(tSD.layerName); // Set the sorting layers



            tableau.Add(cp); // Add this CardPyramid to the List<> tableau    
        }

        // Set which cards are hiding others

        foreach (CardPyramid tCP in tableau)
        {

            foreach (int hid in tCP.pyramidSlotDef.hiddenBy)
            {

                cp = FindCardByLayoutID(hid);

                tCP.hiddenBy.Add(cp);

            }

        }

        // Set up the initial target card

        MoveToTarget(Draw());

        // Set up the Draw pile

        UpdateDrawPile();
    }

    // Convert from the layoutID int to the CardProspector with that ID

    CardPyramid FindCardByLayoutID(int layoutID)
    {

        foreach (CardPyramid tCP in tableau)
        {

            // Search through all cards in the tableau List<>

            if (tCP.layoutID == layoutID)
            {

                // If the card has the same ID, return it

                return (tCP);

            }

        }

        // If it's not found, return null

        return (null);

    }

    // This turns cards in the Mine face-up or face-down

    void SetTableauFaces()
    {

        foreach (CardPyramid pd in tableau)
        {

            bool faceUp = true; // Assume the card will be face-up

            foreach (CardPyramid cover in pd.hiddenBy)
            {

                // If either of the covering cards are in the tableau

                if (cover.state == eCardState.tableau)
                {

                    faceUp = false; // then this card is face-down

                }

            }

            pd.faceUp = faceUp; // Set the value on the card

        }

    }

    // Moves the current target to the discardPile

    void MoveToDiscard(CardPyramid cd)
    {

        // Set the state of the card to discard

        cd.state = eCardState.discard;

        discardPile.Add(cd); // Add it to the discardPile List<>

        cd.transform.parent = layoutAnchor; // Update its transform parent





        // Position this card on the discardPile

        cd.transform.localPosition = new Vector3(

            pLayout.multiplier.x * pLayout.discardPile.x,

            pLayout.multiplier.y * pLayout.discardPile.y,

            -pLayout.discardPile.layerID + 0.5f);

        cd.faceUp = true;

        // Place it on top of the pile for depth sorting

        cd.SetSortingLayerName(pLayout.discardPile.layerName);

        cd.SetSortOrder(-100 + discardPile.Count);

    }



    // Make cd the new target card

    void MoveToTarget(CardPyramid cd)
    {

        // If there is currently a target card, move it to discardPile

        // if (target != null) MoveToDiscard(target);

        target = cd; // cd is the new target

        cd.state = eCardState.target;

        cd.transform.parent = layoutAnchor;


        // Move to the target position

        cd.transform.localPosition = new Vector3(

            pLayout.multiplier.x * pLayout.targetCard.x,

            pLayout.multiplier.y * pLayout.targetCard.y,

            -pLayout.targetCard.layerID);





        cd.faceUp = true; // Make it face-up

        // Set the depth sorting

        cd.SetSortingLayerName(pLayout.targetCard.layerName);

        cd.SetSortOrder(100 + targetPile.Count);

    }

    //void MoveToTargetPOS(CardPyramid cd)
    //{
    //    // Set the state of the card to discard

    //    cd.state = eCardState.target;

    //    targetPile.Add(cd); // Add it to the targetPile List<>

    //    cd.transform.parent = layoutAnchor; // Update its transform parent

    //    // Position this card on the discardPile

    //    cd.transform.localPosition = new Vector3(

    //        pLayout.multiplier.x * pLayout.targetCard.x,

    //        pLayout.multiplier.y * pLayout.targetCard.y,

    //        -pLayout.targetCard.layerID + 0.5f);

    //    cd.faceUp = true;

    //    // Place it on top of the pile for depth sorting

    //    cd.SetSortingLayerName(pLayout.targetCard.layerName);

    //    cd.SetSortOrder(-100 + targetPile.Count);
    //}



    // Arranges all the cards of the drawPile to show how many are left

    void UpdateDrawPile()
    {

        CardPyramid cd;

        // Go through all the cards of the drawPile

        for (int i = 0; i < drawPile.Count; i++)
        {

            cd = drawPile[i];

            cd.transform.parent = layoutAnchor;




            // Position it correctly with the layout.drawPile.stagger

            Vector2 dpStagger = pLayout.drawPile.stagger;

            cd.transform.localPosition = new Vector3(

                pLayout.multiplier.x * (pLayout.drawPile.x + i * dpStagger.x),

                pLayout.multiplier.y * (pLayout.drawPile.y + i * dpStagger.y),

                -pLayout.drawPile.layerID + 0.1f * i);




            cd.faceUp = false; // Make them all face-down

            cd.state = eCardState.drawpile;

            // Set depth sorting

            cd.SetSortingLayerName(pLayout.drawPile.layerName);

            cd.SetSortOrder(-10 * i);

        }

    }

    public void CardClicked(CardPyramid cd)
    {

        // The reaction is determined by the state of the clicked card

        switch (cd.state)
        {

            case eCardState.target:

                // Clicking the target card does nothing

                break;



            case eCardState.drawpile:

                // Clicking any card in the drawPile will draw the next card

                MoveToTarget(target); // Moves the target to the discardPile

                MoveToTarget(Draw());  // Moves the next drawn card to the target

                UpdateDrawPile();     // Restacks the drawPile

                break;





            case eCardState.tableau:

                // Clicking a card in the tableau will check if it's a valid play
                bool validMatch = true;

                if (!cd.faceUp)
                {

                    // If the card is face-down, it's not valid

                    validMatch = false;

                }

                if (!RankAddsUpTo13(cd, target))
                {

                    // If it's not an adjacent rank, it's not valid

                    validMatch = false;

                }

                if (!validMatch) return; // return if not valid



                // If we got here, then: Yay! It's a valid card.

                tableau.Remove(cd); // Remove it from the tableau List

                MoveToTarget(cd);  // Make it the target card

                SetTableauFaces();  // Update tableau card face-ups

                

                break;

        }

        CheckForGameOver();

    }

    // Test whether the game is over

    void CheckForGameOver()
    {

        // If the tableau is empty, the game is over

        if (tableau.Count == 0)
        {

            // Call GameOver() with a win

            GameOver(true);

            return;

        }




        // If there are still cards in the draw pile, the game's not over

        if (drawPile.Count > 0)
        {

            return;

        }




        // Check for remaining valid plays

        foreach (CardPyramid cd in tableau)
        {

            if (RankAddsUpTo13(cd, target))
            {

                // If there is a valid play, the game's not over

                return;

            }

        }




        // Since there are no valid plays, the game is over

        // Call GameOver with a loss

        GameOver(false);

    }



    // Called when the game is over. Simple for now, but expandable

    void GameOver(bool won)
    {

        if (won)
        {

            print ("Game Over. You won! :)");

        }
        else
        {

            print ("Game Over. You Lost. :(");

        }

        // Reload the scene, resetting the game

        SceneManager.LoadScene("__Pyramid");

    }

    // Return true if the two cards are adjacent in rank (A & K wrap around)

    public bool RankAddsUpTo13(CardPyramid c0, CardPyramid c1)
    {

        // If either card is face-down, it's not adjacent.

        if (!c0.faceUp || !c1.faceUp) return (false);



        // If they are equal to 13, they are discarded

        if (Mathf.Abs(c0.rank + c1.rank) == 13)
        {
            print("A match! Good job me!");
            return (true);
        }

        // If one is a King, then remove it.

        if (c0.rank == 13)
        {
            return (true);
        }
        //if (c0.rank == 13 && c1.rank == 1) return (true);



        // Otherwise, return false

        return (false);

    }

    List<CardPyramid> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {

        List<CardPyramid> lCP = new List<CardPyramid>();

        CardPyramid tCP;

        foreach (Card tCD in lCD)
        {

            tCP = tCD as CardPyramid;                                     // a

            lCP.Add(tCP);

        }

        return (lCP);

    }
}
