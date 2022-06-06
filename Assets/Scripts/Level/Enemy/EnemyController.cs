using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EnemyController : CoreFunc
{
    #region [ PARAMETERS ]

    List<EnemyDrone> drones;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Start()
    {
        drones = FindDrones();
        {
            foreach (EnemyDrone drone in drones)
            {
                if (drone.CheckState(EnemyAIState.Patrolling))
                {
                    drone.ChangeState(EnemyAIState.Patrolling);
                }
                else
                {
                    drone.ChangeState(EnemyAIState.Idle);
                }
            }
        }
    }

    void Update()
    {
        StateCheck();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    private List<EnemyDrone> FindDrones()
    {
        EnemyDrone[] droneArray = FindObjectsOfType<EnemyDrone>();
        List<EnemyDrone> drones = new List<EnemyDrone>();
        foreach(EnemyDrone drone in droneArray)
        {
            drones.Add(drone);
        }
        return drones;
    }

    private void StateCheck()
    {
        foreach (EnemyDrone drone in drones)
        {
            if (drone.canChangeState)
            {
                foreach (EnemyAIState newState in Enum.GetValues(typeof(EnemyAIState)))
                {
                    bool stateChanged = false;
                    if (drone.CheckState(newState) && !stateChanged)
                    {
                        drone.ChangeState(newState);
                        stateChanged = true;
                    }
                }
            }
        }
    }

    public void OnPause()
    {
        foreach (EnemyDrone drone in drones)
        {
            drone.hum.Pause();
        }
    }

    public void OnResume()
    {
        foreach (EnemyDrone drone in drones)
        {
            drone.hum.Play();
        }
    }
}
