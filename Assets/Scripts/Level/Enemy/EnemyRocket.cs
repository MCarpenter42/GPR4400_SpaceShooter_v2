using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EnemyRocket : CoreFunc
{
    #region [ PARAMETERS ]

    [HideInInspector] public GameObject player;

    [SerializeField] GameObject pointer;
    [SerializeField] GameObject visuals;

    private bool move = true;
    [SerializeField] float speed = 6.0f;
    [SerializeField] float explosionRadius = 0.75f;
    [SerializeField] int explosionDamage = 1;

    private SphereCollider cldr;

    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip launch;
    [SerializeField] AudioClip explosion;

    private bool seek = true;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        sfx.PlayOneShot(launch);
    }

    void Update()
    {
        if (move)
        {
            if (seek)
            {
                transform.LookAt(player.transform.position);
                float distance = (player.transform.position - transform.position).magnitude;
                if (distance < 2.5f)
                {
                    seek = false;
                }
            }
            transform.localPosition += transform.forward * speed * Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // Debug.Log("Hit! | " + Time.time);
        move = false;

        GameObject hitObj = col.gameObject;

        if (hitObj.CompareTag("Player"))
        {
            hitObj.GetComponent<Player>().HitByRocket(1);
        }
        else if (hitObj.CompareTag("Enemy"))
        {
            hitObj.GetComponent<EnemyDrone>().HitByRocket(3);
        }

        float volumeMulti = 0.04f;
        float distanceScale = 3.0f * volumeMulti / (player.transform.position - transform.position).magnitude;
        sfx.volume = volumeMulti + distanceScale;
        sfx.pitch = 1.4f;
        sfx.PlayOneShot(explosion);
        visuals.SetActive(false);
        Destroy(gameObject, 0.5f);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void DisableCollider()
    {
        cldr = gameObject.GetComponent<SphereCollider>();
        cldr.enabled = false;
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.1f);
        cldr.enabled = true;
    }
}
