using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class handles the UI elements used for the game ending, whether
// in a success or fail state.

public class GameEnd : UI
{
    #region [ PARAMETERS ]

    private GameObject overlay;
    private GameObject panel;
    private GameObject displayWin;
    private GameObject displayLose;

    private Player player;

	#endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        GetComponents();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Start()
    {
        overlay.SetActive(false);
        panel.SetActive(false);
        displayWin.SetActive(false);
        displayLose.SetActive(false);
        PanelColourCycle();
    }
	
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    // Retrieves references for the relevant components
    private void GetComponents()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            if (child.CompareTag("MenuPanel"))
            {
                panel = child;
            }
            else if (child.CompareTag("Win"))
            {
                displayWin = child;
            }
            else if (child.CompareTag("Lose"))
            {
                displayLose = child;
            }
            else
            {
                overlay = child;
            }
        }
    }

    // Initialiser for the panel's colour cycle.
    private void PanelColourCycle()
    {
        MenuPanel panelHandler = panel.GetComponent<MenuPanel>();
        if (panelHandler.doColourCycle)
        {
            DoColourCycle(panel.GetComponent<Image>(), (int)panelHandler.cycleType, true, (int)panelHandler.clr1, (int)panelHandler.clr2);
        }
    }

    // Handles what happens on the game ending.
    public void EndGame(bool win)
    {
        Debug.Log("Game over");
        StartCoroutine(End(win));
    }

    public void EndToMenu()
    {
        gameState.gameEnded = false;
        GoToScene(0);
    }

    public void EndToExit()
    {
        gameState.gameEnded = false;
        Exit();
    }

    private IEnumerator End(bool win)
    {
        if (win)
        {
            yield return new WaitForSeconds(3.0f);
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        Pause();

        overlay.SetActive(true);
        panel.SetActive(true);
        if (win)
        {
            displayWin.SetActive(true);
            displayLose.SetActive(false);
            playerHandler.LevelSFX(1);
        }
        else
        {
            displayWin.SetActive(false);
            displayLose.SetActive(true);
            playerHandler.LevelSFX(2);
        }

        gameState.gameEnded = true;
    }

}
