using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Stores data for pickup objects, as well as applying
// simple "animation" where appropriate.

public class Pickup : CoreFunc
{
    #region [ PARAMETERS ]

    public enum pickupType { energyRestore, healthRestore };
    [SerializeField] public pickupType type;
    [SerializeField] public int power = 1;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Update()
    {
        if ((int)type == 0)
        {
            float yRot = 12.0f * Time.deltaTime;
            transform.Rotate(new Vector3(0.0f, yRot, 0.0f), Space.Self);
        }
    }
}
