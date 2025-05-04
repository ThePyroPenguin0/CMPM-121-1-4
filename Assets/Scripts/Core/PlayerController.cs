using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static Spell;

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
    private Dictionary<string, Spell> baseSpells = new Dictionary<string, Spell>();
    private Dictionary<string, ModifierSpell> modifierSpells = new Dictionary<string, ModifierSpell>();
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
        JToken jo3 = JObject.Parse(spellstext.text);

        foreach (var spell in jo3)
        {
            if (spell["damage_multiplier"] != null || spell["mana_multiplier"] != null || spell["speed_multiplier"] != null || spell["cooldown_multiplier"] != null || spell["projectile_trajectory"] != null || spell["mana_adder"] != null)
            {
                ModifierSpell modSpell = spell.ToObject<ModifierSpell>();
                modifierSpells[modSpell.name] = modSpell;
            }
            else
            {
                Spell sp = spell.ToObject<Spell>();
                baseSpells[sp.name] = sp;
            }
        }

        hp = new Hittable(RPN.EvaluateRPN("95 wave 5 * +", new Dictionary<string, int>() { ["wave"] = theSpawner.currentWave }), Hittable.Team.PLAYER, gameObject);
        TMP_Text[] allText = gameOverUI.GetComponentsInChildren<TMP_Text>(true);
        numEnemiesKilled = Array.Find(allText, x => x.gameObject.name == "GameOverText");

    }

    public void StartLevel()
    {
        spellcaster = new SpellCaster(RPN.EvaluateRPN("90 wave 10 * +", new Dictionary<string, int>() { ["wave"] = theSpawner.currentWave }), RPN.EvaluateRPN("10 wave +", new Dictionary<string, int>() { ["wave"] = theSpawner.currentWave }), Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());
        
        hp.SetMaxHP(RPN.EvaluateRPN("95 wave 5 * +", new Dictionary<string, int>() { ["wave"] = theSpawner.currentWave }));
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
        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        Debug.Log("You Lost: " + GameManager.Instance.state);
        if (GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            GameManager.Instance.ClearAllEnemies();
            StartCoroutine(ShowGameOverScreen());
        }
        GameManager.Instance.state = GameManager.GameState.PREGAME;
    }

    IEnumerator ShowGameOverScreen()
    {
        if (numEnemiesKilled != null)
        {
            numEnemiesKilled.text = "Level Finished!!! You have killed " + GameManager.Instance.enemiesKilled + " enemies.";
        }
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(15f);
        //gameOverUI.SetActive(false);
    }

    public void ReturnToStartMenu()
    {
        GameManager.Instance.state = GameManager.GameState.PREGAME;

        gameOverUI.SetActive(false);

        if (theSpawner != null)
        {
            theSpawner.level_selector.gameObject.SetActive(true);
        }

        GameManager.Instance.ClearAllEnemies();
    }
}
