using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Points any object it is applied to towards the player camera.

// Used for the energy effect in the power cores.

public class PointAtPlayer : CoreFunc
{
    #region [ PARAMETERS ]

    private GameObject playerCam;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void FixedUpdate()
    {
        Vector3 dir = playerCam.transform.position - gameObject.transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
