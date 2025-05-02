using UnityEngine;
using System.Collections.Generic;

public class MagicSpell
{
    public string name = "none";
    public string description = "none";
    public int icon = 0;
    public string N = "0";
    public string damage = "0";
    public string mana_cost = "0";
    public string cooldown = "0";
    public Projectile projectile = new Projectile("none", "lifetime", "0", 0);
    public Projectile secondary_projectile = new Projectile("none", "lifetime", "0", 0);
}

public class Projectile
{
    public string trajectory = "none";
    public string lifetime = "999999";
    public string speed = "0";
    public int sprite = 0;

    public Projectile(string trajectory, string lifetime, string speed, int sprite)
    {
        this.trajectory = trajectory;
        this.lifetime = lifetime;
        speed = "0";
        sprite = 0;
    }
}