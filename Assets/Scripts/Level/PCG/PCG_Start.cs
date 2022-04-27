using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Start : CoreFunc
{
    #region [ PARAMETERS ]

    [Header("SpawnPoints")]
    [SerializeField] GameObject interiorParent;
    private List<Transform> interior = new List<Transform>();
    [SerializeField] GameObject xPositiveParent;
    private List<Transform> xPositive = new List<Transform>();
    [SerializeField] GameObject xNegativeParent;
    private List<Transform> xNegative = new List<Transform>();
    [SerializeField] GameObject yPositiveParent;
    private List<Transform> yPositive = new List<Transform>();
    [SerializeField] GameObject yNegativeParent;
    private List<Transform> yNegative = new List<Transform>();
    [SerializeField] GameObject zPositiveParent;
    private List<Transform> zPositive = new List<Transform>();
    [SerializeField] GameObject zNegativeParent;
    private List<Transform> zNegative = new List<Transform>();

    private List<Transform>[] dirLists = new List<Transform>[6];

    [Header("Corner Points")]
    [SerializeField] GameObject cornersParent;
    [HideInInspector] public List<Transform> cornerPoints;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Init()
    {
        GetComponents();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void GetComponents()
    {
        for (int i = 0; i < interiorParent.transform.childCount; i++)
        {
            Transform point = interiorParent.transform.GetChild(i).gameObject.transform;
            interior.Add(point);
        }

        for (int i = 0; i < xPositiveParent.transform.childCount; i++)
        {
            Transform point = xPositiveParent.transform.GetChild(i).gameObject.transform;
            xPositive.Add(point);
        }
        dirLists[0] = xPositive;
        for (int i = 0; i < yPositiveParent.transform.childCount; i++)
        {
            Transform point = yPositiveParent.transform.GetChild(i).gameObject.transform;
            yPositive.Add(point);
        }
        dirLists[1] = yPositive;
        for (int i = 0; i < zPositiveParent.transform.childCount; i++)
        {
            Transform point = zPositiveParent.transform.GetChild(i).gameObject.transform;
            zPositive.Add(point);
        }
        dirLists[2] = zPositive;

        for (int i = 0; i < xNegativeParent.transform.childCount; i++)
        {
            Transform point = xNegativeParent.transform.GetChild(i).gameObject.transform;
            xNegative.Add(point);
        }
        dirLists[3] = xNegative;
        for (int i = 0; i < yNegativeParent.transform.childCount; i++)
        {
            Transform point = yNegativeParent.transform.GetChild(i).gameObject.transform;
            yNegative.Add(point);
        }
        dirLists[4] = yNegative;
        for (int i = 0; i < zNegativeParent.transform.childCount; i++)
        {
            Transform point = zNegativeParent.transform.GetChild(i).gameObject.transform;
            zNegative.Add(point);
        }
        dirLists[5] = zNegative;

        for (int i = 0; i < cornersParent.transform.childCount; i++)
        {
            Transform point = cornersParent.transform.GetChild(i).gameObject.transform;
            cornerPoints.Add(point);
        }
    }

    public List<Transform> SelectSpawnPoints()
    {
        List<int> dirs = new List<int> { 0, 1, 2, 3, 4, 5 };
        List<int> dirsRand = new List<int>();

        for (int i = 5; i >= 0; i--)
        {
            int r = 0;
            if (i > 0)
            {
                r = RandomInt(0, i);
            }
            dirsRand.Add(dirs[i]);
            dirs.RemoveAt(i);
        }

        List<Transform> selectedPoints = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            List<Transform> dirList = dirLists[dirsRand[i]];
            float threshold = 0.0f;
            if (i > 0)
            {
                float delta = (float)i / 5.0f;
                threshold = 0.35f + 0.6f * delta;
            }
            float rF = Random.value;

            if (rF >= threshold)
            {
                int rI = RandomInt(0, 3);
                Transform point = dirList[rI];
                selectedPoints.Add(point);
            }
        }

        return selectedPoints;
    }

}
