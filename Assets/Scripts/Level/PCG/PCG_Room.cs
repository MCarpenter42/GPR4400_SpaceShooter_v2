using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Room : CoreFunc
{
    #region [ PARAMETERS ]

    [Header("Anchor Points")]
    [SerializeField] GameObject interior;
    [SerializeField] GameObject positiveX;
    [SerializeField] GameObject positiveY;
    [SerializeField] GameObject positiveZ;
    [SerializeField] GameObject negativeX;
    [SerializeField] GameObject negativeY;
    [SerializeField] GameObject negativeZ;

    [Header("Corner Points")]
    [SerializeField] GameObject cornersParent;
    [HideInInspector] public List<Transform> cornerPoints;

    [HideInInspector] public int iteration = 0;
    [HideInInspector] public int directionForward = 0;
    [HideInInspector] public int directionBackward = 0;

    private PCG_Controller pcgController;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Init(PCG_Controller pcgController)
    {
        GetComponents();
        this.pcgController = pcgController;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void GetComponents()
    {
        for (int i = 0; i < cornersParent.transform.childCount; i++)
        {
            Transform point = cornersParent.transform.GetChild(i).gameObject.transform;
            cornerPoints.Add(point);
        }
    }

    public List<Transform> SelectSpawnPoints()
    {
        List<int> obstructed = new List<int>();
        for (int i = -3; i < 4; i++)
        {
            if (i != 0)
            {
                int[] arrayPos = pcgController.rooms.ArrayPositionFromVector(transform.position);
                int a = Mathf.Abs(i) - 1;
                if (i > 0)
                {
                    arrayPos[a] += 1;
                }
                else if (i < 0)
                {
                    arrayPos[a] -= 1;
                }
                if (pcgController.rooms.CheckObject(arrayPos[0], arrayPos[1], arrayPos[2]))
                {
                    obstructed.Add(i);
                }
                else if (CountAdjacent(arrayPos[0], arrayPos[1], arrayPos[2]) > 1)
                {
                    obstructed.Add(i);
                }
                Debug.Log(CountAdjacent(arrayPos[0], arrayPos[1], arrayPos[2]));
            }
        }

        List<int> dirs = new List<int> { 1, 2, 3, -1, -2, -3 };
        List<int> avDirs = new List<int> { 1, 2, 3, -1, -2, -3 };
        foreach (int i in obstructed)
        {
            if (avDirs.Contains(i))
            {
                int n = avDirs.FindIndex(0, x => x == i);
                avDirs.RemoveAt(n);
            }
        }

        List<int> dirsRand = new List<int>();
        int count = avDirs.Count;
        for (int i = 0; i < count; i++)
        {
            int r = RandomInt(0, avDirs.Count - 1);
            dirsRand.Add(avDirs[r]);
            avDirs.RemoveAt(r);
        }

        /*string debug = null;
        foreach (int i in dirsRand)
        {
            debug += i + " | ";
        }
        Debug.Log(debug);*/

        for (int i = 0; i < dirsRand.Count; i++)
        {

            switch (dirsRand[i])
            {
                case 1:
                    dirsRand[i] = 0;
                    break;
                    
                case 2:
                    dirsRand[i] = 1;
                    break;
                    
                case 3:
                    dirsRand[i] = 2;
                    break;
                    
                case -1:
                    dirsRand[i] = 3;
                    break;
                    
                case -2:
                    dirsRand[i] = 4;
                    break;
                    
                case -3:
                    dirsRand[i] = 5;
                    break;
                    
                default:
                    break;
            }
        }

        List<Transform> selectedPoints = new List<Transform>();
        for (int i = 0; i < dirsRand.Count; i++)
        {
            List<Transform> dirTransforms = new List<Transform> { positiveX.transform, positiveY.transform, positiveZ.transform, negativeX.transform, negativeY.transform, negativeZ.transform };
            float threshold = 1.0f;
            switch (i)
            {
                case 0:
                    threshold = 0.05f;
                    break;
                    
                case 1:
                    threshold = 0.8f;
                    break;
                    
                case 2:
                    threshold = 0.9f;
                    break;
                    
                case 3:
                    threshold = 0.95f;
                    break;
                    
                case 4:
                    threshold = 0.98f;
                    break;

                default:
                    break;
            }
            float rF = Random.value;

            if (rF >= threshold)
            {
                Transform point = dirTransforms[dirsRand[i]];
                selectedPoints.Add(point);
            }
        }

        return selectedPoints;
    }

    public int CountAdjacent(int arrayX, int arrayY, int arrayZ)
    {
        int adjacent =  0;
        for (int i = -3; i < 4; i++)
        {
            if (i != 0)
            {
                int[] arrayPos = new int[] { arrayX, arrayY, arrayZ };
                int a = Mathf.Abs(i) - 1;
                if (i > 0)
                {
                    arrayPos[a] += 1;
                }
                else if (i < 0)
                {
                    arrayPos[a] -= 1;
                }
                if (pcgController.rooms.CheckObject(arrayPos[0], arrayPos[1], arrayPos[2]))
                {
                    adjacent++;
                }
            }
        }
        return adjacent;
    }
    
    public int CountAdjacent(Vector3 pos)
    {
        int adjacent =  0;
        for (int i = -3; i < 4; i++)
        {
            if (i != 0)
            {
                int[] arrayPos = pcgController.rooms.ArrayPositionFromVector(pos);
                int a = Mathf.Abs(i) - 1;
                if (i > 0)
                {
                    arrayPos[a] += 1;
                }
                else if (i < 0)
                {
                    arrayPos[a] -= 1;
                }
                if (pcgController.rooms.CheckObject(arrayPos[0], arrayPos[1], arrayPos[2]))
                {
                    adjacent++;
                }
            }
        }
        return adjacent;
    }

}
