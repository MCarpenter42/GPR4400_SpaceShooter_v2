using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PCG_Controller : CoreFunc
{
    #region [ PARAMETERS ]

    [Header("Terrain Components")]
    [SerializeField] GameObject startRoomPrefab;
    private PCG_Start startRoom;

    [SerializeField] GameObject proceduralRoom;
    [HideInInspector] public ObjectField rooms;
    [HideInInspector] public List<GameObject> spawnedRooms = new List<GameObject>();
    private List<int[]> roomPositions = new List<int[]>();

    [SerializeField] GameObject cornerPrefab;
    [HideInInspector] public ObjectField corners;
    private List<GameObject> cornerList = new List<GameObject>();

    [SerializeField] List<GameObject> wallPrefabs;

    [Header("Generation Behaviour")]
    [SerializeField] int minRooms;
    [SerializeField] int maxIterations;
    private int iterationCounter = 0;
    private enum WallTypes { standardWalls, glassWalls, noWalls };
    [SerializeField] WallTypes wallType = WallTypes.standardWalls;
    [SerializeField] Material glassWallMaterial;


    [Header("Enemies")]
    [SerializeField] GameObject dronePrefab;
    [SerializeField] int[] roomCountThresholds = new int[] { };
    private int enemyCount = 0;
    [SerializeField] bool capEnemyCount = false;
    [SerializeField] int maxEnemyCount;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        if (maxIterations < 2)
        {
            maxIterations = 2;
        }
        if (capEnemyCount && maxEnemyCount < 1)
        {
            maxEnemyCount = 1;
        }
        GenerateObjectFields(10.0f);
        SpawnRooms();
        SpawnCorners();
        if (wallType != WallTypes.noWalls)
        {
            SpawnWalls();
        }
        SpawnEnemies();
    }

    void Start()
    {
        UpdateCorners();
        RemovePrefabs();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    private void GenerateObjectFields(float spacing)
    {
        int n = 3 + maxIterations * 2;

        if (corners == null)
        {
            corners = new ObjectField(n, spacing, transform.position);
        }

        if (rooms == null)
        {
            rooms = new ObjectField(n - 1, spacing, transform.position);
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
                roomPositions.Add(rooms.ArrayPositionFromVector(point.position));
            }

            iterationCounter += 1;
        }

        CheckRoomCount();
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

    private void CheckRoomCount()
    {
        if (roomPositions.Count < minRooms)
        {
            ClearAll();
            SpawnRooms();
        }
    }

    private void ClearAll()
    {
        for (int x = 0; x < corners.fieldWidth; x++)
        {
            for (int y = 0; y < corners.fieldWidth; y++)
            {
                for (int z = 0; z < corners.fieldWidth; z++)
                {
                    corners.SetCheck(false, x, y, z);
                    corners.SetObject(null, x, y, z);
                }
            }
        }

        for (int x = 0; x < rooms.fieldWidth; x++)
        {
            for (int y = 0; y < rooms.fieldWidth; y++)
            {
                for (int z = 0; z < rooms.fieldWidth; z++)
                {
                    rooms.SetCheck(false, x, y, z);
                }
            }
        }

        RemovePrefabs();
        spawnedRooms.Clear();
        roomPositions.Clear();
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

        /*foreach (int[] position in roomPositions)
        {
            for (int i = -3; i < 4; i++)
            {
                if (i != 0 && WallCheck(i, position))
                {
                    SpawnWall(i, position);
                }
            }
        }*/
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
    
    private bool WallCheck(int dir, int[] arrayPos)
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

        if (rooms.CheckObject(arrayPos))
        {
            if (rooms.CheckObject(arrayPos[0] + dirArray[0], arrayPos[1] + dirArray[1], arrayPos[2] + dirArray[2]))
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
        Vector3 wallRot = Vector3.zero;
        switch (i)
        {
            case 2:
                wallRot[2] = 90.0f;
                break;
                
            case 3:
                wallRot[1] = -90.0f;
                break;
                
            case -1:
                wallRot[1] = 180.0f;
                break;
                
            case -2:
                wallRot[2] = -90.0f;
                break;
                
            case -3:
                wallRot[1] = 90.0f;
                break;

            default:
                break;
        }
        Vector3 wallRotLocal = new Vector3(90.0f * RandomInt(-1, 2), 0.0f, 0.0f);

        int n = wallPrefabs.Count - 1;
        int r = RandomInt(0, n);
        GameObject wall = Instantiate(wallPrefabs[r], gameObject.transform, true);
        wall.transform.position = rooms.VectorFromArrayPosition(x, y, z);
        wall.transform.eulerAngles = wallRot;
        wall.transform.Rotate(wallRotLocal, Space.Self);
        if (wallType == WallTypes.glassWalls)
        {
            GetChildrenWithComponent<MeshRenderer>(wall)[0].GetComponent<MeshRenderer>().material = glassWallMaterial;
        }
    }
    
    private void SpawnWall(int i, int[] arrayPos)
    {
        Vector3 wallRot = Vector3.zero;
        switch (i)
        {
            case 2:
                wallRot[2] = 90.0f;
                break;
                
            case 3:
                wallRot[1] = -90.0f;
                break;
                
            case -1:
                wallRot[1] = 180.0f;
                break;
                
            case -2:
                wallRot[2] = -90.0f;
                break;
                
            case -3:
                wallRot[1] = 90.0f;
                break;

            default:
                break;
        }
        Vector3 wallRotLocal = new Vector3(90.0f * RandomInt(-1, 2), 0.0f, 0.0f);

        int n = wallPrefabs.Count - 1;
        int r = RandomInt(0, n);
        GameObject wall = Instantiate(wallPrefabs[r], gameObject.transform, true);
        wall.transform.position = rooms.VectorFromArrayPosition(arrayPos);
        wall.transform.eulerAngles = wallRot;
        wall.transform.Rotate(wallRotLocal, Space.Self);
        if (wallType == WallTypes.glassWalls)
        {
            GetChildrenWithComponent<MeshRenderer>(wall)[0].GetComponent<MeshRenderer>().material = glassWallMaterial;
        }
    }

    private void UpdateCorners()
    {
        foreach (GameObject corner in cornerList)
        {
            corner.GetComponent<PCG_Corner>().Init(this);
        }
    }

    private void SpawnEnemies()
    {
        int validOctants = 0;
        List<Vector3> startPoints = new List<Vector3>();

        int a = rooms.fieldWidth / 2 - 1;
        int b = rooms.fieldWidth / 2;
        int c = rooms.fieldWidth - 1;

        int[] xLim = new int[2];
        int[] yLim = new int[2];
        int[] zLim = new int[2];
        List<int[]> limits = new List<int[]> { xLim, yLim, zLim };
        int[] centre = new int[3];

        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 0:
                    {
                    xLim[0] = 0;
                    xLim[1] = a;
                    yLim[0] = 0;
                    yLim[1] = a;
                    zLim[0] = 0;
                    zLim[1] = a;
                    }
                    break;
                    
                case 1:
                    {
                    xLim[0] = 0;
                    xLim[1] = a;
                    yLim[0] = 0;
                    yLim[1] = a;
                    zLim[0] = b;
                    zLim[1] = c;
                    }
                    break;
                    
                case 2:
                    {
                    xLim[0] = 0;
                    xLim[1] = a;
                    yLim[0] = b;
                    yLim[1] = c;
                    zLim[0] = 0;
                    zLim[1] = a;
                    }
                    break;

                case 3:
                    {
                    xLim[0] = 0;
                    xLim[1] = a;
                    yLim[0] = b;
                    yLim[1] = c;
                    zLim[0] = b;
                    zLim[1] = c;
                    }
                    break;
                    
                case 4:
                    {
                    xLim[0] = b;
                    xLim[1] = c;
                    yLim[0] = 0;
                    yLim[1] = a;
                    zLim[0] = 0;
                    zLim[1] = a;
                    }
                    break;
                    
                case 5:
                    {
                    xLim[0] = b;
                    xLim[1] = c;
                    yLim[0] = 0;
                    yLim[1] = a;
                    zLim[0] = b;
                    zLim[1] = c;
                    }
                    break;
                    
                case 6:
                    {
                    xLim[0] = b;
                    xLim[1] = c;
                    yLim[0] = b;
                    yLim[1] = c;
                    zLim[0] = 0;
                    zLim[1] = a;
                    }
                    break;
                    
                case 7:
                    {
                    xLim[0] = b;
                    xLim[1] = c;
                    yLim[0] = b;
                    yLim[1] = c;
                    zLim[0] = b;
                    zLim[1] = c;
                    }
                    break;

            }
            for (int j = 0; j < 3; j++)
            {
                if (limits[j][1] < b)
                {
                    centre[j] = a;
                }
                else
                {
                    centre[j] = b;
                }
            }

            List<int[]> validPositions = new List<int[]>();
            for (int x = xLim[0]; x <= xLim[1]; x++)
            {
                for (int y = yLim[0]; y <= yLim[1]; y++)
                {
                    for (int z = zLim[0]; z <= zLim[1]; z++)
                    {
                        int[] pos = new int[] { x, y, z };
                        if (!(x == centre[0] && y == centre[1] && z == centre[2]) && rooms.CheckObject(pos))
                        {
                            validPositions.Add(pos);
                        }
                    }
                }
            }

            if (validPositions.Count > 8)
            {
                validOctants++;
                int r = RandomInt(0, validPositions.Count - 1);
                Vector3 point = rooms.VectorFromArrayPosition(validPositions[r]);
                startPoints.Add(point);
            }
        }

        Debug.Log(validOctants + " valid octants found. Spawning enemies.");
        enemyCount = validOctants;

        int n = startPoints.Count;
        if (capEnemyCount)
        {
            n = maxEnemyCount;
        }
        for (int i = 0; i < n; i++)
        {
            Vector3 startPoint = startPoints[i];
            GameObject enemy = Instantiate(dronePrefab, gameObject.transform);
            EnemyDrone drone = enemy.GetComponent<EnemyDrone>();
            drone.patrolPoints = GeneratePatrolPath(startPoint);
        }

        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().GetEnemyCount(enemyCount);
    }

    private List<Vector3> GeneratePatrolPath(Vector3 startPoint)
    {
        Vector3 point = startPoint;
        List<Vector3> points = new List<Vector3>();

        int pathLength = RandomInt(5, 8);
        for(int i = 0; i < pathLength; i++)
        {
            points.Add(point);
            if (i == pathLength - 1)
            {
                break;
            }

            List<Vector3> validPositions = new List<Vector3>();
            int[] arrayPos = rooms.ArrayPositionFromVector(point);

            for (int j = -3; j < 4; j++)
            {
                if (j != 0)
                {
                    int[] checkPos = new int[3];
                    checkPos[0] = arrayPos[0];
                    checkPos[1] = arrayPos[1];
                    checkPos[2] = arrayPos[2];
                    int a = Mathf.Abs(j);
                    int b = 0;
                    if (j > 0)
                    {
                        b = 1;
                    }
                    else if (j < 0)
                    {
                        b = -1;
                    }
                    checkPos[a - 1] += b;
                    if (rooms.CheckObject(checkPos))
                    {
                        bool posValid = true;
                        Vector3 vectPos = rooms.VectorFromArrayPosition(checkPos);
                        foreach (Vector3 pnt in points)
                        {
                            if (vectPos == pnt)
                            {
                                posValid = false;
                            }
                        }
                        if (posValid)
                        {
                            validPositions.Add(vectPos);
                        }
                    }
                }
            }
            if (validPositions.Count > 1)
            {
                int r = RandomInt(0, validPositions.Count - 1);
                point = validPositions[r];
            }
            else if (validPositions.Count == 1)
            {
                point = validPositions[0];
            }
            else if (validPositions.Count == 0)
            {
                break;
            }
        }
        return points;
    }

    private void RemovePrefabs()
    {
        foreach (GameObject prefabRoom in spawnedRooms)
        {
            Destroy(prefabRoom);
        }
    }

    /*private void TestPathGen()
    {
        int[] startPos = roomPositions[RandomInt(0, roomPositions.Count - 1)];
        Vector3 startPoint = rooms.VectorFromArrayPosition(startPos);
        List<Vector3> path = GeneratePatrolPath(startPoint);
        string pathString = "Points in path: " + path.Count + " || ";
        foreach (Vector3 point in path)
        {
            pathString += point[0] + " | " + point[1] + " | " + point[2] + " || ";
        }
        Debug.Log(pathString);
    }*/
}
