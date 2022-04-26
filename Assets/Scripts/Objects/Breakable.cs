using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to handle composite breakable objects.

// Something breaks the explosion force when in the .exe rather
// than the Unity editor and I have no idea what.

public class Breakable : CoreFunc
{
    #region [ PARAMETERS ]

    private GameObject groupParent;
    private MeshCollider meshCollider;
    private List<GameObject> barriers = new List<GameObject>();
    private List<Rigidbody> pieces = new List<Rigidbody>();

    [SerializeField] float breakForce;

    [SerializeField] AudioSource explosionSFX;
    [SerializeField] AudioClip explode;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        GetComponents();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // Gets the various components that make up the object.
    private void GetComponents()
    {
        groupParent = gameObject.transform.parent.gameObject;

        meshCollider = gameObject.GetComponent<MeshCollider>();

        barriers = GetChildrenWithTag(groupParent, "Barrier");

        List<GameObject> pieceObjs = GetChildrenWithComponent<Rigidbody>(groupParent);
        foreach (GameObject piece in pieceObjs)
        {
            pieces.Add(piece.GetComponent<Rigidbody>());
        }
    }

    // Handles the breaking process - triggered by the
    // player's shooting raycast.
    public void Break(Vector3 hitPoint)
    {
        foreach (GameObject barrier in barriers)
        {
            barrier.SetActive(false);
        }

        meshCollider.enabled = false;

        foreach (Rigidbody piece in pieces)
        {
            piece.AddExplosionForce(breakForce, hitPoint, transform.localScale.magnitude * 6.0f);
        }

        explosionSFX.PlayOneShot(explode);
    }
}
