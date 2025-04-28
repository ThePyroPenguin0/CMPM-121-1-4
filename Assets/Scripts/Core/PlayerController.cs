using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public GameObject gameOverUI;

    public EnemySpawner theSpawner;

    public int speed;

    public Unit unit;

    TMP_Text numEnemiesKilled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
        theSpawner = FindObjectOfType<EnemySpawner>();

        TMP_Text [] allText = gameOverUI.GetComponentsInChildren<TMP_Text>(true);
        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "GameOverText");
        if(numEnemiesKilled != null){
            Debug.Log("Found some text here = " + numEnemiesKilled.name);
        }
    }

    public void StartLevel()
    {
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());
        
        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void Die()
    {
        Debug.Log("You Lost: " + GameManager.Instance.state);
        if(GameManager.Instance.state == GameManager.GameState.INWAVE){
            GameManager.Instance.ClearAllEnemies();
            StartCoroutine(ShowGameOverScreen());
        }
        GameManager.Instance.state = GameManager.GameState.PREGAME;
    }

    IEnumerator ShowGameOverScreen(){
        if(numEnemiesKilled != null){
            numEnemiesKilled.text = "Level Finished!!! You have killed " + GameManager.Instance.enemiesKilled + " enemies.";
        }
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(15f); 
        //gameOverUI.SetActive(false);
    }

    public void ReturnToStartMenu(){
        GameManager.Instance.state = GameManager.GameState.PREGAME;

        gameOverUI.SetActive(false);

        if(theSpawner != null){
            theSpawner.level_selector.gameObject.SetActive(true);
        } 

        GameManager.Instance.ClearAllEnemies(); 
    }
}
