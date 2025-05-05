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

    public Dictionary<string, JObject> spells = new Dictionary<string, JObject>();

    TMP_Text numEnemiesKilled;

    public Dictionary<string, JObject> manaObjects = new Dictionary<string, JObject>();
    public Dictionary<string, JObject> enhancedObjects = new Dictionary<string, JObject>();
    public JObject spellAttributes;
    public JObject enhancedAttributes;

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
            spells.Add(spell.Key, (JObject)spell.Value);
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

            if (manaObjects == null) {
                // only needs to do this once to create mana objects and enhanced objects out of the json configuration
                for(int i = 0; i < spells.Count; i++){
                    //Debug.Log("How many spells: " + spells.Count + " SpellEntry: " + spells.ElementAt(i).Value);
                    if(spells.ElementAt(i).Value.ContainsKey("mana_cost")){
                        manaObjects.Add(spells.ElementAt(i).Key, spells.ElementAt(i).Value);
                    } else {
                        enhancedObjects.Add(spells.ElementAt(i).Key, spells.ElementAt(i).Value);
                    }
                }
            }            
        }

        TMP_Text [] allText = gameOverUI.GetComponentsInChildren<TMP_Text>(true);
        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "GameOverText");
        
    }

    public void StartLevel(Dictionary<string, JObject> spells)
    {   
        // To discuss using a fix mana json configuration for easy, medium or endless level (instead of randomely selected here)
        int randomEntry = UnityEngine.Random.Range(0, manaObjects.Count);
        Debug.Log("How many manaobjects: " + manaObjects.Count + " RandomEntry: " + randomEntry);
        JObject spellAttributes = manaObjects.ElementAt(randomEntry).Value;
        //if (spellAttributes["mana_cost"].Value<string>())
        //int manaCost = int.Parse(spellAttributes["mana_cost"].Value<string>());
        int manaCost = RPN.EvaluateRPN(spellAttributes["mana_cost"].Value<string>(), new Dictionary<string, int> { ["wave"] = theSpawner.currentWave , [ "power" ] = 100});

        Debug.Log("ManaCost ========= " + manaCost + "\nThe Spells ========= " + spellAttributes + "\nThe Spell Value ====== " + spellAttributes["mana_cost"].Value<string>());
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

    public void DecideRandSpell(){
        // Decide which spell and which enhance to be randomly used
        // Outcome: set the randomly used spell attributes to spellAttributes
        //          set the randomly used enhance attributes to enhancedAttributes
        // They will be used in RewardScreenManager.cs before showing the reward screen
        // which shows the randomly used spell and enhance.
        int randomSpell = UnityEngine.Random.Range(0, manaObjects.Count);
        int randomEnhance = UnityEngine.Random.Range(0, enhancedObjects.Count);
        Debug.Log("How many randomSpells: " + manaObjects.Count + " randomSpell: " + randomSpell);
        Debug.Log("How many randomEnhances: " + enhancedObjects.Count + " randomEnhance: " + randomEnhance);

        spellAttributes = manaObjects.ElementAt(randomSpell).Value;
        enhancedAttributes = enhancedObjects.ElementAt(randomEnhance).Value;
    }

    public void SetRandomSpell(int manaCost) {
        if (spellcaster != null && spellcaster.spell != null) {
            // remove the existing one to have a new one
            spellcaster.spell = null;
            spellcaster = null;
        }

        spellcaster = new SpellCaster(manaCost, (manaCost/15), Hittable.Team.PLAYER);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);     
    }
}
