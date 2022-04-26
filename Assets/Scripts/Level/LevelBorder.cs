using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Literally the entire purpose of this script is to have the level border objects
// only render when the player is nearby to them, to preserve visuals as much as
// possible, but still provide feedback for level boundaries.

public class LevelBorder : CoreFunc
{
    [SerializeField] axes axis;
    public enum axes { x, y, z }

    private GameObject player;
    private MeshRenderer rndr;
    private Color matClr;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rndr = gameObject.GetComponent<MeshRenderer>();
        matClr = rndr.material.color;
    }

    void Update()
    {
        float dist = Mathf.Abs(transform.position[(int)axis] - player.transform.position[(int)axis]);
        if (dist > 10)
        {
            rndr.enabled = false;
        }
        else if (dist <= 10 && dist > 2)
        {
            rndr.enabled = true;
            Color rndrClr = matClr;
            rndrClr[3] = 0.2f - ((dist - 2.0f) / 40.0f);
            rndr.material.color = rndrClr;
        }
        else
        {
            rndr.enabled = true;
            Color rndrClr = matClr;
            rndrClr[3] = 0.2f;
            rndr.material.color = rndrClr;
        }
    }
}
