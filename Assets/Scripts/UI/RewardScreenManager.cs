using System;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    TMP_Text numEnemiesKilled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP_Text[] allText = rewardUI.GetComponentsInChildren<TMP_Text>(true);

        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "LevelCompletedText");

        if(numEnemiesKilled != null){
            Debug.Log("Found some text here = " + numEnemiesKilled.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            if(numEnemiesKilled != null){
                numEnemiesKilled.text = "Level Finished!!! You have killed " + GameManager.Instance.enemiesKilled + " enemies.";
            }
            rewardUI.SetActive(true);
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }
}
