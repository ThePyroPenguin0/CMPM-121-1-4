using UnityEngine;

public class GameOverScreenManager : MonoBehaviour
{
    public GameObject gameOverUI;  // Reference to the Game Over UI object
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check if the game state is GAMEOVER and display the Game Over UI
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            gameOverUI.SetActive(false);
        }
    }

    // This method will be called when the "Return to Home" button is clicked
    public void ReturnToHomePage()
    {
        // Reset the game state to PREGAME
        GameManager.Instance.state = GameManager.GameState.PREGAME;

        // Hide the Game Over UI
        gameOverUI.SetActive(false);
    }
}
