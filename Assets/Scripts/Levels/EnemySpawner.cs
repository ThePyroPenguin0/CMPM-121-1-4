using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    private Dictionary<string, Enemy> enemy_types = new Dictionary<string, Enemy>();
    private Dictionary<string, Level> level_types = new Dictionary<string, Level>();
    private Level currentLevel;
    private int currentWave = 1;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        var enemytext = Resources.Load<TextAsset>("enemies");

        JToken jo = JToken.Parse(enemytext.text);
        foreach (var enemy in jo)
        {
            Enemy en = enemy.ToObject<Enemy>();
            enemy_types[en.name] = en;
        }

        var leveltext = Resources.Load<TextAsset>("levels");

        JToken jo2 = JToken.Parse(leveltext.text);
        foreach (var level in jo2)
        {
            Level le = level.ToObject<Level>();
            level_types[le.name] = le;
        }

        var levelEntries = level_types.Values.ToList();
        CreateLevel(levelEntries[0].name, new Vector3(0, 90));
        CreateLevel(levelEntries[1].name, new Vector3(0, 0));
        CreateLevel(levelEntries[2].name, new Vector3(0, -90)); // I know I can do this with a for loop more dynamically, but I don't want to deal with spacing at this moment - Gabriel
    }

    // Added code - Jocelyn 
    void CreateLevel(string theLevel, Vector3 position)
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = position;

        MenuSelectorController theMenu = selector.GetComponent<MenuSelectorController>();
        theMenu.spawner = this;
        theMenu.SetLevel(theLevel);
        currentLevel = level_types[theLevel];

        Button theButton = selector.GetComponent<Button>();
        theButton.onClick.AddListener(() => theMenu.StartLevel());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        // ^ Cope and seethe
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1.01f);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (var spawn in currentLevel.spawns)
        {


            // Check if the sequence field exists and is not null
            if (spawn.sequence != null && spawn.sequence.Count > 0)
            {
                foreach (int group in spawn.sequence)
                {
                    Debug.Log($"Spawning {spawn.enemy}, group of {group}");
                    for (int i = 0; i < group; i++)
                    {
                        StartCoroutine(SpawnZombie(spawn.enemy));
                    }
                    yield return new WaitForSeconds(spawn.delay);
                }
            }
            else
            {
                Debug.Log($"Enemy: {spawn.enemy}, Count: {spawn.count}");
                //Debug.Log($"Spawning {spawn.enemy}, no sequence defined. Defaulting to {RPN.EvaluateRPN(spawn.count, new Dictionary<string, int>() { ["wave"] = currentWave })} spawns.");
                for (int i = 0; i < RPN.EvaluateRPN(spawn.count, new Dictionary<string, int>() { ["wave"] = currentWave }); i++)
                {
                    StartCoroutine(SpawnZombie(spawn.enemy));
                }
                yield return new WaitForSeconds(spawn.delay);
            }
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        /*
        if(PlayerController.Instance.player.GetComponent<PlayerController>().hp != 0){
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
        }
        
        if(GameManager.Instance.player.GetComponent<PlayerController>().hp.hp != 0){
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
        }
        */

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        currentWave++;
    }

    IEnumerator SpawnZombie(string type)
    {
        Level.Spawn spawn = currentLevel.spawns.FirstOrDefault(s => s.enemy == type);
        Enemy enemyData = enemy_types[type];

        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemyData.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(RPN.EvaluateRPN(spawn.hp, new Dictionary<string, int>() { ["base"] = enemy_types[type].hp, ["wave"] = currentWave }), Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemyData.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
