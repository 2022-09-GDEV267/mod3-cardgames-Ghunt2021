using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// An enum defines a variable type with a few prenamed values        // a

public enum ePyrCardState
{

    drawpile,

    tableau,

    target,

    discard

}

public class CardPyramid : Card
{ // Make sure CardPyramid extends Card

    [Header("Set Dynamically: CardProspector")]

    // This is how you use the enum eCardState

    public eCardState state = eCardState.drawpile;

    // The hiddenBy list stores which other cards will keep this one face down

    public List<CardPyramid> hiddenBy = new List<CardPyramid>();

    // The layoutID matches this card to the tableau XML if it's a tableau card

    public int layoutID;

    // The SlotDef class stores information pulled in from the LayoutXML <slot>

    public PyramidSlotDef pyramidSlotDef;

        // This allows the card to react to being clicked

    override public void OnMouseUpAsButton() {

      // Call the CardClicked method on the Prospector singleton

      Pyramid.P.CardClicked(this);

      // Also call the base class (Card.cs) version of this method

      base.OnMouseUpAsButton();                                                  // a

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
