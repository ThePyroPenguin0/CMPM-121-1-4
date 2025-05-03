using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    // New properties for asgn2
    public string name;
    public string description;
    public int icon;
    public Damage damage;
    public int mana_cost;
    public float cooldown;
    public Projectile projectile;
    public Projectile secondary_projectile;
    public float? damage_multiplier;
    public float? mana_multiplier;
    public float? speed_multiplier;
    public float? delay;
    public float? angle;
    public float? mana_adder;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public string GetName()
    {
        return name;
    }

    public int GetManaCost()
    {
        return mana_cost;
    }

    public int GetDamage()
    {
        return damage.amount;
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, projectile.trajectory, where, target - where, projectile.speed, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), damage.type));
        }

    }

}

public class Projectile
{
    public string trajectory = "none defined";
    public float speed = 0f;
    public float lifetime = 0f;
    public int sprite = 0;
}
