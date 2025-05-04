using UnityEngine;
using System.Collections.Generic;


public class Enemy
{
    public string name;
    public int hp;
    public int sprite;
    public int speed;
    public int damage;
}

public class Spawn
{
    public string enemy;
    public string count;
    public List<int> sequence;
    public string delay = "2";
    public string location;
    public string hp = "base";
    public string damage = "base";
    public string speed = "base";
}

public class Level
{
    public string name;
    public int waves;
    public List<Spawn> spawns;
}