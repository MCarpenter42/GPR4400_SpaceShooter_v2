using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Just allows certain values to be set for an
// individual panel from the inspector.

public class MenuPanel : UI
{
    #region [ PARAMETERS ]

    [SerializeField] public bool doColourCycle;
    [SerializeField] public colourCycleTypes cycleType;
    [SerializeField] public colours clr1;
    [SerializeField] public colours clr2;

	#endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

}
