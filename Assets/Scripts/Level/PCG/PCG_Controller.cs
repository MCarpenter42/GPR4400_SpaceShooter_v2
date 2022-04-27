using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Controller : CoreFunc
{
    #region [ PARAMETERS ]

    [SerializeField] GameObject startRoomPrefab;
    private PCG_Start startRoom;

    [SerializeField] GameObject proceduralRoom;
    [HideInInspector] public ObjectField rooms;
    [HideInInspector] public List<GameObject> spawnedRooms = new List<GameObject>();
    private List<int[]> roomPositions = new List<int[]>;

    [SerializeField] GameObject cornerPrefab;
    [HideInInspector] public ObjectField corners;
    private List<GameObject> cornerList = new List<GameObject>();

    [SerializeField] List<GameObject> wallPrefabs;

    [SerializeField] int maxIterations;
    private int iterationCounter = 0;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        if (maxIterations < 2)
        {
            maxIterations = 2;
        }
        GenerateObjectFields();
        SpawnRooms();
        SpawnCorners();
        SpawnWalls();
    }

    void Start()
    {
        UpdateCorners();
        RemovePrefabs();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    private void GenerateObjectFields()
    {
        int n = 3 + maxIterations * 2;

        if (corners == null)
        {
            corners = new ObjectField(n, 6.0f, transform.position);
        }

        if (rooms == null)
        {
            rooms = new ObjectField(n - 1, 6.0f, transform.position);
        }
    }

    private void SpawnRooms()
    {
        GameObject startRoomObj = Instantiate(startRoomPrefab, gameObject.transform, true);
        spawnedRooms.Add(startRoomObj);

        startRoom = startRoomObj.GetComponent<PCG_Start>();
        startRoom.Init();
        SetCornerPoints(startRoom);

        int n = rooms.fieldWidth / 2;
        rooms.SetCheck(true, n, n, n);
        rooms.SetCheck(true, n, n - 1, n);
        rooms.SetCheck(true, n, n, n - 1);
        rooms.SetCheck(true, n, n - 1, n - 1);
        rooms.SetCheck(true, n - 1, n, n);
        rooms.SetCheck(true, n - 1, n - 1, n);
        rooms.SetCheck(true, n - 1, n, n - 1);
        rooms.SetCheck(true, n - 1, n - 1, n - 1);

        List<GameObject> recentRooms = new List<GameObject>();
        for (int i = 0; i < maxIterations; i++)
        {
            List<Transform> spawnPoints = new List<Transform>();
            if (i == 0)
            {
                spawnPoints = startRoom.SelectSpawnPoints();
            }
            else
            {
                foreach (GameObject room in recentRooms)
                {
                    spawnPoints.AddRange(room.GetComponent<PCG_Room>().SelectSpawnPoints());
                }
            }

            recentRooms.Clear();

            foreach (Transform point in spawnPoints)
            {
                GameObject roomObj = Instantiate(proceduralRoom, gameObject.transform, false);
                roomObj.transform.position = point.position;
                spawnedRooms.Add(roomObj);
                recentRooms.Add(roomObj);

                PCG_Room room = roomObj.GetComponent<PCG_Room>();
                room.Init(this);
                SetCornerPoints(room);

                rooms.SetCheck(true, point.position);
            }

            iterationCounter += 1;
        }
    }

    private void SetCornerPoints(PCG_Start room)
    {
        foreach (Transform point in room.cornerPoints)
        {
            Vector3 pos = point.position;
            corners.SetCheck(true, pos);
        }
    }
    
    private void SetCornerPoints(PCG_Room room)
    {
        foreach (Transform point in room.cornerPoints)
        {
            Vector3 pos = point.position;
            corners.SetCheck(true, pos);
        }
    }

    private void SpawnCorners()
    {
        for (int x = 0; x < corners.fieldWidth; x++)
        {
            for (int y = 0; y < corners.fieldWidth; y++)
            {
                for (int z = 0; z < corners.fieldWidth; z++)
                {
                    if (corners.checks[x, y, z])
                    {
                        GameObject corner = Instantiate(cornerPrefab, gameObject.transform, false);
                        corners.SetObject(corner, x, y, z);
                        cornerList.Add(corner);
                    }
                }
            }
        }
    }

    private void SpawnWalls()
    {
        for (int x = 0; x < rooms.fieldWidth; x++)
        {
            for (int y = 0; y < rooms.fieldWidth; y++)
            {
                for (int z = 0; z < rooms.fieldWidth; z++)
                {
                    for (int i = -3; i < 4; i++)
                    {
                        if (i != 0 && WallCheck(i, x, y, z))
                        {
                            SpawnWall(i, x, y, z);
                        }
                    }
                }
            }
        }
    }

    private bool WallCheck(int dir, int arrayX, int arrayY, int arrayZ)
    {
        if (dir > 3)
        {
            dir = 3;
        }
        else if (dir < -3)
        {
            dir = -3;
        }
        else if (dir == 0)
        {
            dir = 1;
        }

        int[] dirArray = new int[] { 0, 0, 0 };
        if (dir > 0)
        {
            dirArray[Mathf.Abs(dir) - 1] = 1;
        }
        else
        {
            dirArray[Mathf.Abs(dir) - 1] = -1;
        }

        if (rooms.CheckObject(arrayX, arrayY, arrayZ))
        {
            if (rooms.CheckObject(arrayX + dirArray[0], arrayY + dirArray[1], arrayZ + dirArray[2]))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private void SpawnWall(int i, int x, int y, int z)
    {
        Vector3 wallPos = rooms.VectorFromArrayPosition(x, y, z);
        float offset = rooms.objectSpacing / 2.0f;
        int a = Mathf.Abs(i) - 1;
        if (i > 0)
        {
            wallPos[a] += offset;
        }
        else
        {
            wallPos[a] -= offset;
        }

        Vector3 wallRot = Vector3.zero;
        switch (Mathf.Abs(i))
        {
            case 2:
                wallRot[2] = 90.0f;
                break;
                
            case 3:
                wallRot[1] = 90.0f;
                break;

            default:
                break;
        }

        int n = wallPrefabs.Count - 1;
        int r = RandomInt(0, n);
        GameObject wall = Instantiate(wallPrefabs[r], gameObject.transform, true);
        wall.transform.position = wallPos;
        wall.transform.eulerAngles = wallRot;
    }

    private void UpdateCorners()
    {
        foreach (GameObject corner in cornerList)
        {
            corner.GetComponent<PCG_Corner>().Init(this);
        }
    }

    private void RemovePrefabs()
    {
        foreach (GameObject prefabRoom in spawnedRooms)
        {
            Destroy(prefabRoom);
        }
    }
}
