using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Corner : MonoBehaviour
{
    #region [ PARAMETERS ]
    [Header("Beams")]
    [SerializeField] GameObject beamPositiveX;
    [SerializeField] GameObject beamPositiveY;
    [SerializeField] GameObject beamPositiveZ;
    [SerializeField] GameObject beamNegativeX;
    [SerializeField] GameObject beamNegativeY;
    [SerializeField] GameObject beamNegativeZ;

    [Header("Connectors")]
    [SerializeField] GameObject connector3a;
    [SerializeField] GameObject connector3b;
    [SerializeField] GameObject connector4a;
    [SerializeField] GameObject connector4b;
    [SerializeField] GameObject connector6;

    GameObject[] beams = new GameObject[6];
    private bool[] beamDirs = new bool[] { false, false, false, false, false, false };
    private int beamActiveCount = 0;

    private PCG_Controller pcgController;

	#endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    public void Init(PCG_Controller pcgController)
    {
        GetComponents();
        this.pcgController = pcgController;
        CheckForAdjacent();
        ShowBeams();
        ShowConnector();
    }

    private void GetComponents()
    {
        beams[0] = beamPositiveX;
        beams[1] = beamPositiveY;
        beams[2] = beamPositiveZ;
        beams[3] = beamNegativeX;
        beams[4] = beamNegativeY;
        beams[5] = beamNegativeZ;

        connector3a.SetActive(false);
        connector3b.SetActive(false);
        connector4a.SetActive(false);
        connector4b.SetActive(false);
        connector6.SetActive(false);
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
            if (beamDirs[i])
            {
                beamActiveCount++;
            }
        }
    }

    private void ShowConnector()
    {
        switch (beamActiveCount)
        {
            case 3:
                Connector3();
                break;
                
            case 4:
                Connector4();
                break;
                
            case 5:
            case 6:
                connector6.SetActive(true);
                break;

            default:
                break;
        }
    }

    private void Connector3()
    {
        if (beamDirs[0])
        {
            if (beamDirs[1])
            {
                // 012
                // +X +Y +Z
                if (beamDirs[2])
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                }
                // 013
                // +X +Y -X
                else if (beamDirs[3])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                }
                // 014
                // +X +Y -Y
                else if (beamDirs[4])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                }
                // 015
                // +X +Y -Z
                else
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(-90.0f, 90.0f, 0.0f);
                }
            }
            else if (beamDirs[2])
            {
                // 023
                // +X +Z -X
                if (beamDirs[3])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                }
                // 024
                // +X +Z -Y
                else if (beamDirs[4])
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(-90.0f, 90.0f, 0.0f);
                }
                // 025
                // +X +Z -Z
                else
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(0.0f, 90.0f, -90.0f);
                }
            }
            else if (beamDirs[3])
            {
                // 034
                // +X -X -Y
                if (beamDirs[4])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(180.0f, 0.0f, 0.0f);
                }
                // 035
                // +X -X -Z
                else
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
                }
            }
            // 045
            // +X -Y -Z
            else
            {
                connector3a.SetActive(true);
                connector3a.transform.eulerAngles = new Vector3(180.0f, 90.0f, 0.0f);
            }
        }
        else if (beamDirs[1])
        {
            if (beamDirs[2])
            {
                // 123
                // +Y +Z -X
                if (beamDirs[3])
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                }
                // 124
                // +Y +Z -Y
                else if (beamDirs[4])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(90.0f, 90.0f, 0.0f);
                }
                // 125
                // +Y +Z -Z
                else
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                }
            }
            else if (beamDirs[3])
            {
                // 134
                // +Y -X -Y
                if (beamDirs[4])
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(180.0f, 0.0f, 0.0f);
                }
                // 135
                // +Y -X -Z
                else
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
                }
            }
            // 145
            // +Y -Y -Z
            else
            {
                connector3b.SetActive(true);
                connector3b.transform.eulerAngles = new Vector3(0.0f, 90.0f, -90.0f);
            }
        }
        else if (beamDirs[2])
        {
            if (beamDirs[3])
            {
                // 234
                // +Z -X -Y
                if (beamDirs[4])
                {
                    connector3a.SetActive(true);
                    connector3a.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                }
                // 235
                // +Z -X -Z
                else
                {
                    connector3b.SetActive(true);
                    connector3b.transform.eulerAngles = new Vector3(90.0f, 0.0f, 90.0f);
                }
            }
            // 245
            // +Z -Y -Z
            else
            {
                connector3b.SetActive(true);
                connector3b.transform.eulerAngles = new Vector3(180.0f, 90.0f, 0.0f);
            }
        }
        // 345
        // -X -Y -Z
        else
        {
            connector3a.SetActive(true);
            connector3a.transform.eulerAngles = new Vector3(0.0f, -90.0f, 90.0f);
        }
    }

    private void Connector4()
    {
        if (beamDirs[0])
        {
            if (beamDirs[1])
            {
                if (beamDirs[2])
                {
                    // 0123
                    // +X +Y +Z -X
                    if (beamDirs[3])
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                    }
                    // 0124
                    // +X +Y +Z -Y
                    else if (beamDirs[4])
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                    }
                    // 0125
                    // +X +Y +Z -Z
                    else
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                    }
                }
                else if (beamDirs[3])
                {
                    // 0134
                    // +X +Y -X -Y
                    if (beamDirs[4])
                    {
                        connector4b.SetActive(true);
                        connector4b.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                    }
                    // 0135
                    // +X +Y -X -Z
                    else
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
                    }
                }
                // 0345
                // +X +Y -Y -Z
                else
                {
                    connector4a.SetActive(true);
                    connector4a.transform.eulerAngles = new Vector3(-90.0f, 90.0f, 0.0f);
                }
            }
            else if (beamDirs[2])
            {
                // 0234
                // +X +Z -X -Z
                if (beamDirs[3])
                {
                    connector4b.SetActive(true);
                    connector4b.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                }
                // 0235
                // +X +Z -X -Y
                else
                {
                    connector4a.SetActive(true);
                    connector4a.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                }
            }
            else
            {
                // 0345
                // +X -X -Y -Z
                connector4a.SetActive(true);
                connector4a.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
            }
        }
        else if (beamDirs[1])
        {
            if (beamDirs[2])
            {
                if (beamDirs[3])
                {
                    // 1234
                    // +Y +Z -X -Y
                    if (beamDirs[4])
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                    }
                    // 1235
                    // +Y +Z -X -Z
                    else
                    {
                        connector4a.SetActive(true);
                        connector4a.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
                    }
                }
                // 1245
                // +Y +Z -Y -Z
                else
                {
                    connector4b.SetActive(true);
                    connector4b.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                }
            }
            // 1345
            // +Y -X -Y -Z
            else
            {
                connector4a.SetActive(true);
                connector4a.transform.eulerAngles = new Vector3(0.0f, -90.0f, 90.0f);
            }
        }
        // 2345
        // + Z -X -Y -Z
        else
        {
            connector4a.SetActive(true);
            connector4a.transform.eulerAngles = new Vector3(0.0f, -90.0f, 90.0f);
        }
    }
}
