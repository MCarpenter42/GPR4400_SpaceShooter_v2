using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Corner : MonoBehaviour
{
    #region [ PARAMETERS ]

    [SerializeField] GameObject beamPositiveX;
    [SerializeField] GameObject beamPositiveY;
    [SerializeField] GameObject beamPositiveZ;
    [SerializeField] GameObject beamNegativeX;
    [SerializeField] GameObject beamNegativeY;
    [SerializeField] GameObject beamNegativeZ;

    GameObject[] beams = new GameObject[6];
    private bool[] beamDirs = new bool[] { false, false, false, false, false, false };

    private PCG_Controller pcgController;

	#endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    public void Init(PCG_Controller pcgController)
    {
        GetComponents();
        this.pcgController = pcgController;
        CheckForAdjacent();
        ShowBeams();
    }

    private void GetComponents()
    {
        beams[0] = beamPositiveX;
        beams[1] = beamPositiveY;
        beams[2] = beamPositiveZ;
        beams[3] = beamNegativeX;
        beams[4] = beamNegativeY;
        beams[5] = beamNegativeZ;
    }

    private void CheckForAdjacent()
    {
        int[] arrayPos = pcgController.corners.ArrayPositionFromVector(transform.position);
        beamDirs[0] = pcgController.corners.CheckObject(arrayPos[0] + 1, arrayPos[1], arrayPos[2]);
        beamDirs[1] = pcgController.corners.CheckObject(arrayPos[0], arrayPos[1] + 1, arrayPos[2]);
        beamDirs[2] = pcgController.corners.CheckObject(arrayPos[0], arrayPos[1], arrayPos[2] + 1);
        beamDirs[3] = pcgController.corners.CheckObject(arrayPos[0] - 1, arrayPos[1], arrayPos[2]);
        beamDirs[4] = pcgController.corners.CheckObject(arrayPos[0], arrayPos[1] - 1, arrayPos[2]);
        beamDirs[5] = pcgController.corners.CheckObject(arrayPos[0], arrayPos[1], arrayPos[2] - 1);
    }

    private void ShowBeams()
    {
        for (int i = 0; i < 6; i++)
        {
            beams[i].SetActive(beamDirs[i]);
        }
    }
}
