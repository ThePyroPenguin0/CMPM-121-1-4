using System;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    //public GameObject spellUI; 
    public Button acceptButton; 
    TMP_Text numEnemiesKilled;
    TMP_Text randSpell; 
    PlayerController playerController; 
    public SpellCaster spellcaster;

    public JObject spellAttributes;
    public JObject enhancedAttributes;

    public EnemySpawner theSpawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP_Text[] allText = rewardUI.GetComponentsInChildren<TMP_Text>(true);

        RewardScreenManager acceptOrDecline = rewardUI.GetComponent<RewardScreenManager>();

        Button theButton = rewardUI.GetComponent<Button>();

        playerController = FindObjectOfType<PlayerController>();

        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "LevelCompletedText");

        randSpell = Array.Find(allText, x => x.gameObject.name == "SpellText");

        theButton.onClick.AddListener(() => acceptOrDecline.SetRandSpell());

        if(numEnemiesKilled != null){
            Debug.Log("Found some text here = " + numEnemiesKilled.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            playerController.DecideRandSpell();
            randSpell.text = playerController.enhancedAttributes["name"] + " " + playerController.spellAttributes["name"] + "\n" + 
                                playerController.enhancedAttributes["name"] + ": " + playerController.enhancedAttributes["description"] + "\n" + 
                                playerController.spellAttributes["name"] + ": " + playerController.spellAttributes["description"];  

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

    public void SetRandSpell()
    {    
        theSpawner = FindObjectOfType<EnemySpawner>();   
        int manaCost = RPN.EvaluateRPN(playerController.spellAttributes["mana_cost"].Value<string>(), new Dictionary<string, int> { ["wave"] = theSpawner.currentWave , [ "power" ] = 100});
        playerController.SetRandomSpell(manaCost);
    }
}
