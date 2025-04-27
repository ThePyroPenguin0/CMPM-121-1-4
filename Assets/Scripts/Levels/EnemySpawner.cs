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
        GameObject rookieSelector = Instantiate(button, level_selector.transform);
        rookieSelector.transform.localPosition = new Vector3(0, 130);
        rookieSelector.GetComponent<MenuSelectorController>().spawner = this;
        rookieSelector.GetComponent<MenuSelectorController>().SetLevel("Rookie Level");
        rookieSelector.GetComponent<MenuSelectorController>().level = "Easy";

        GameObject mediumSelector = Instantiate(button, level_selector.transform);
        mediumSelector.transform.localPosition = new Vector3(0, 30);
        mediumSelector.GetComponent<MenuSelectorController>().spawner = this;
        mediumSelector.GetComponent<MenuSelectorController>().SetLevel("Medium Level");
        mediumSelector.GetComponent<MenuSelectorController>().level = "Medium";

        GameObject advSelector = Instantiate(button, level_selector.transform);
        advSelector.transform.localPosition = new Vector3(0, -70);
        advSelector.GetComponent<MenuSelectorController>().spawner = this;
        advSelector.GetComponent<MenuSelectorController>().SetLevel("Advanced Level");
        advSelector.GetComponent<MenuSelectorController>().level = "Endless";
        
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");

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
    }

    // Added code - Jocelyn 
    void CreateLevel(string theLevel, Vector3 position)
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = position;

        MenuSelectorController theMenu = selector.GetComponent<MenuSelectorController>();
        theMenu.spawner = this;
        theMenu.SetLevel(theLevel);

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
            yield return new WaitForSeconds(0.2f);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (var spawn in currentLevel.spawns)
        {
            foreach (int group in spawn.sequence)
            {
                for (int i = 0; i < group; i++)
                {
                    StartCoroutine(SpawnZombie(spawn.enemy));
                }
                yield return new WaitForSeconds(spawn.delay);
            }
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        currentWave++;
    }

    IEnumerator SpawnZombie(string type)
    {
        Enemy enemyData = enemy_types[type];

        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(RPN.EvaluateRPN(type, new Dictionary<string, int>() { ["base"] = enemyData.hp, ["wave"] = currentWave }), Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemyData.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
