using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

// GameManager singleton class.

public class GameManager : CoreFunc
{
    private static GameManager instance = null;

    #region [ PARAMETERS ]

    private bool setupRun = false;

    public static bool isCursorLocked;

    public static float FPS;
    private List<float> frameTimes = new List<float>();

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameManager inst = FindObjectOfType<GameManager>();
                if (inst == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();

                    instance.Init();

                    // Prevents game manager from being destroyed on loading of a new scene
                    DontDestroyOnLoad(obj);

                    Debug.Log(obj.name);
                }
            }
            return instance;
        }
    }

    // Initialiser function, serves a similar purpose to a constructor
    private void Init()
    {
        Setup();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        Setup();
    }

    void Start()
    {
        OnStartDebugging();
    }

    void Update()
    {
        CalcFPS();
        OnUpdateDebugging();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void Setup()
    {
        playerHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        enemyHandler = FindObjectOfType<EnemyController>().GetComponent<EnemyController>();
    }

    private void CalcFPS()
    {
        if (frameTimes.Count >= 60)
        {
            frameTimes.RemoveAt(0);
        }
        frameTimes.Add(Time.deltaTime);
        float total = 0.0f;
        foreach (float f in frameTimes)
        {
            total += f;
        }
        FPS = (int)((float)frameTimes.Count / total);
    }

    private void OnStartDebugging()
    {

    }

    private void OnUpdateDebugging()
    {
        //Debug.Log(FPS);
    }
}
