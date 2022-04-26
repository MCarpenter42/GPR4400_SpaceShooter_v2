using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class just makes the camera on the main menu spin.

// Not strictly necessary, but with the skybox I'm using
// I think it's a nice effect!

public class MainMenuCam : CoreFunc
{
    #region [ PARAMETERS ]

    private float yRot;
    [SerializeField] float yRotRate;

	#endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Update()
    {
        RotateCam();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    private void RotateCam()
    {
        yRot = yRotRate * Time.unscaledDeltaTime;
        transform.Rotate(new Vector3(0.0f, yRot, 0.0f), Space.Self);
    }
}
