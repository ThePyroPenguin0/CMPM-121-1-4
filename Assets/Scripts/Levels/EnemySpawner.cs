using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI; 

public class EnemySpawner : MonoBehaviour
{
    public UnityEngine.UI.Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    private Dictionary<string, Enemy> enemy_types = new Dictionary<string, Enemy>();
    private Dictionary<string, Level> level_types = new Dictionary<string, Level>();
    private Level currentLevel;
    public int currentWave = 1;
    private Dictionary<string, JObject> spells = new Dictionary<string, JObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int buttonPosition = 90;
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

            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, buttonPosition);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(le.name);
            buttonPosition -= 90;
        }

        // read the spells.json file here similar to above code 
        var spellstext = Resources.Load<TextAsset>("spells");
 
        JObject jo3 = JObject.Parse(spellstext.text);
        foreach(var spell in jo3){
            spells.Add(spell.Key, (JObject)spell.Value);
            Debug.Log("The spell ----- " + spell + ". ");
        }
        
        var levelEntries = level_types.Values.ToList();
        CreateLevel(levelEntries[0].name, new Vector3(0, 90));
        CreateLevel(levelEntries[1].name, new Vector3(0, 0));
        CreateLevel(levelEntries[2].name, new Vector3(0, -90)); // I know I can do this with a for loop more dynamically, but I don't want to deal with spacing at this moment - Gabriel
       
    }

    // Added code - Jocelyn
    // Fixed code - Gabriel
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

    public void StartLevel(string level_name)
    {
        currentLevel = level_types[level_name];
        level_selector.gameObject.SetActive(false);
        Debug.Log("=========== Spells ============== " + spells);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel(spells);
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        currentWave++;
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        //Debug.Log($"Spawnwave called.");
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        foreach (var spawn in currentLevel.spawns)
        {
            Debug.Log($"Enemy: {spawn.enemy}, Delay: {spawn.delay}");
        }
        foreach (var spawn in currentLevel.spawns)
        {
            int spawnCount = RPN.EvaluateRPN(spawn.count, new Dictionary<string, int>() { ["wave"] = currentWave });
            int spawned = 0;
            if (spawn.sequence != null)
            {
                Debug.Log($"Sequence provided for {spawn.enemy}");
                while (spawned < spawnCount)
                {
                    foreach (int group in spawn.sequence)
                    {
                        for (int i = 0; i < group && spawned < spawnCount; i++)
                        {
                            StartCoroutine(SpawnZombie(spawn.enemy));
                            spawned++;
                        }
                        Debug.Log($"Spawns left for {spawn.enemy}: {spawnCount - spawned}");
                        yield return new WaitForSeconds(RPN.EvaluateRPN(spawn.delay, new Dictionary<string, int>()));
                    }
                }
            }
            else
            {
                Debug.Log($"No sequence provided for {spawn.enemy}");
                while (spawned < RPN.EvaluateRPN(spawn.count, new Dictionary<string, int>() { ["wave"] = currentWave }))
                {
                    Debug.Log($"Spawn delay for {spawn.enemy}: {spawn.delay}");
                    StartCoroutine(SpawnZombie(spawn.enemy));
                    yield return new WaitForSeconds(RPN.EvaluateRPN(spawn.delay, new Dictionary<string, int>()));
                    spawned++;
                    Debug.Log($"Spawns left for {spawn.enemy}: {RPN.EvaluateRPN(spawn.count, new Dictionary<string, int>() { ["wave"] = currentWave }) - spawned}");
                }

            }
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnZombie(string type)
    {
        Spawn spawn = currentLevel.spawns.FirstOrDefault(s => s.enemy == type);
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
        yield return null;
        //yield return new WaitForSeconds(spawn.delay);
    }
}
