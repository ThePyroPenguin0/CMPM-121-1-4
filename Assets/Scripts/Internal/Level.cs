using UnityEngine;
using System.Collections.Generic;

public class Level
{
    public string name;
    public int waves;
    public List<Spawn> spawns;

    public class Spawn
    {
        public string enemy;
        public string count;
        public string hp;   
        public float delay;
        public List<int> sequence;
        public string location;
    }
}