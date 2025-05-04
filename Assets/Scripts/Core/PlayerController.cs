using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public Dictionary<string, Spell> spellList;

    public Dictionary<string, JObject> manaObjects = new Dictionary<string, JObject>();

    TMP_Text numEnemiesKilled;
    private List<string> spells = new List<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
        theSpawner = FindObjectOfType<EnemySpawner>();

        // read the spells.json file here similar to above code 
        var spellstext = Resources.Load<TextAsset>("spells");

        JObject jo3 = JObject.Parse(spellstext.text);
        foreach (var spell in jo3)
        {
            spells.Add(spell.Key);
            Debug.Log("The spell loaded: " + spell.Key + ".");

            if (spell.Value["damage"] != null)
            {
                var damageJson = spell.Value["damage"];
                string amountExpression = damageJson["amount"].ToString();
                string typeString = damageJson["type"].ToString();
                int amount = RPN.EvaluateRPN(amountExpression, new Dictionary<string, int> { ["wave"] = theSpawner.currentWave , [ "power" ] = 100});
                Damage damage = new Damage(amount, Damage.TypeFromString(typeString));

                Debug.Log($"Damage Amount: {damage.amount}, Damage Type: {damage.type}");
            }
        }

        TMP_Text [] allText = gameOverUI.GetComponentsInChildren<TMP_Text>(true);
        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "GameOverText");
        
    }

    public void StartLevel(Dictionary<string, JObject> spells)
    {   
        if(manaObjects.Count > 0){
            manaObjects.Clear();
        }

        for(int i = 0; i < spells.Count; i++){
            if(spells.ElementAt(i).Value.ContainsKey("mana_cost")){
                manaObjects.Add(spells.ElementAt(i).Key, spells.ElementAt(i).Value);
            }
        }

        int randomEntry = UnityEngine.Random.Range(0, manaObjects.Count);
        JObject spellAttributes = manaObjects.ElementAt(randomEntry).Value;
        int manaCost = int.Parse(spellAttributes["mana_cost"].Value<string>());
        spellcaster = new SpellCaster(manaCost, (manaCost/15), Hittable.Team.PLAYER);
        // spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);

        StartCoroutine(spellcaster.ManaRegeneration());
        // SpellBuilder.Build(spellcaster);
        
        hp = new Hittable(RPN.EvaluateRPN("95 wave 5 * +", new Dictionary<string, int>() { ["wave"] = theSpawner.currentWave }), Hittable.Team.PLAYER, gameObject);
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
