using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ObjectField
{
    public bool[,,] checks { get; private set; }
    public GameObject[,,] objects { get; private set; }

    public int fieldWidth { get; private set; }
    public float objectSpacing { get; private set; }
    public Vector3 fieldCentrepoint { get; private set; }
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public ObjectField(int fieldWidth, float objectSpacing)
    {
        if (fieldWidth < 2)
        {
            fieldWidth = 2;
        }
        this.fieldWidth = fieldWidth;
        this.objectSpacing = objectSpacing;
        this.fieldCentrepoint = Vector3.zero;
        checks = new bool[fieldWidth, fieldWidth, fieldWidth];
        objects = new GameObject[fieldWidth, fieldWidth, fieldWidth];
    }
    
    public ObjectField(int fieldWidth, float objectSpacing, Vector3 fieldCentrepoint)
    {
        if (fieldWidth < 2)
        {
            fieldWidth = 2;
        }
        this.fieldWidth = fieldWidth;
        this.objectSpacing = objectSpacing;
        this.fieldCentrepoint = fieldCentrepoint;
        checks = new bool[fieldWidth, fieldWidth, fieldWidth];
        objects = new GameObject[fieldWidth, fieldWidth, fieldWidth];
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetCheck(bool check, int arrayX, int arrayY, int arrayZ)
    {
        checks[arrayX, arrayY, arrayZ] = check;
    }

    public void SetCheck(bool check, Vector3 position)
    {
        int[] arrayPos = ArrayPositionFromVector(position);

        for (int i = 0; i < 3; i++)
        {
            if (arrayPos[i] < 0)
            {
                arrayPos[i] = 0;
            }
            else if (arrayPos[i] > fieldWidth - 1)
            {
                arrayPos[i] = fieldWidth - 1;
            }
        }

        checks[arrayPos[0], arrayPos[1], arrayPos[2]] = check;
    }

    public void SetObject(GameObject obj, int arrayX, int arrayY, int arrayZ)
    {
        objects[arrayX, arrayY, arrayZ] = obj;

        obj.transform.position = VectorFromArrayPosition(arrayX, arrayY, arrayZ);
    }
    
    public void SetObject(GameObject obj, Vector3 position)
    {
        int[] arrayPos = ArrayPositionFromVector(position);

        for (int i = 0; i < 3; i++)
        {
            if (arrayPos[i] < 0)
            {
                arrayPos[i] = 0;
            }
            else if (arrayPos[i] > fieldWidth - 1)
            {
                arrayPos[i] = fieldWidth - 1;
            }
        }

        objects[arrayPos[0], arrayPos[1], arrayPos[2]] = obj;

        obj.transform.position = VectorFromArrayPosition(arrayPos[0], arrayPos[1], arrayPos[2]);
    }

    public GameObject GetObject(int arrayX, int arrayY, int arrayZ)
    {
        return objects[arrayX, arrayY, arrayZ];
    }

    public bool CheckObject(int arrayX, int arrayY, int arrayZ)
    {
        if (arrayX < 0 || arrayX > checks.GetLength(0) - 1 || arrayY < 0 || arrayY > checks.GetLength(1) - 1 || arrayZ < 0 || arrayZ > checks.GetLength(2) - 1)
        {
            return false;
        }
        else
        {
            return checks[arrayX, arrayY, arrayZ];
        }
    }

    public bool CheckObject(Vector3 position)
    {
        int[] arrayPos = ArrayPositionFromVector(position);

        if (arrayPos[0] < 0 || arrayPos[0] > fieldWidth - 1 || arrayPos[1] < 0 || arrayPos[1] > fieldWidth - 1 || arrayPos[2] < 0 || arrayPos[2] > fieldWidth - 1)
        {
            return false;
        }
        else
        {
            return checks[arrayPos[0], arrayPos[1], arrayPos[2]];
        }
    }

    public int[] ArrayPositionFromVector(Vector3 position)
    {
        float halfWidth = ((float)(fieldWidth - 1) / 2.0f) * objectSpacing;
        Vector3 offset = fieldCentrepoint - new Vector3(halfWidth, halfWidth, halfWidth);

        position.x -= offset.x;
        position.y -= offset.y;
        position.z -= offset.z;

        position.x /= objectSpacing;
        position.y /= objectSpacing;
        position.z /= objectSpacing;

        if ((position.x % 1.0f) != 0.0f)
        {
            if ((position.x % 1.0f) >= 0.5f)
            {
                position.x = Mathf.Ceil(position.x);
            }
            else
            {
                position.x = Mathf.Floor(position.x);
            }
        }
        if ((position.y % 1.0f) != 0.0f)
        {
            if ((position.y % 1.0f) >= 0.5f)
            {
                position.y = Mathf.Ceil(position.y);
            }
            else
            {
                position.y = Mathf.Floor(position.y);
            }
        }
        if ((position.z % 1.0f) != 0.0f)
        {
            if ((position.z % 1.0f) >= 0.5f)
            {
                position.z = Mathf.Ceil(position.z);
            }
            else
            {
                position.z = Mathf.Floor(position.z);
            }
        }

        int arrayX = (int)position.x;
        int arrayY = (int)position.y;
        int arrayZ = (int)position.z;

        return new int[] { arrayX, arrayY, arrayZ };
    }

    public Vector3 VectorFromArrayPosition(int arrayX, int arrayY, int arrayZ)
    {
        float halfWidth = ((float)(fieldWidth - 1) / 2.0f) * objectSpacing;
        Vector3 pos = fieldCentrepoint;
        pos[0] += (float)arrayX * objectSpacing - halfWidth;
        pos[1] += (float)arrayY * objectSpacing - halfWidth;
        pos[2] += (float)arrayZ * objectSpacing - halfWidth;
        return pos;
    }
}
