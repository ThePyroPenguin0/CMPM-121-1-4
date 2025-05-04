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
    public int? N; // Count of projectiles
    public Damage damage;
    public int mana_cost;
    public float cooldown;
    public Projectile projectile;
    public Projectile secondary_projectile;
    public float? delay;
    public float? angle;

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
        for(int i = 0; i < N; i++)
        {
            GameManager.Instance.projectileManager.CreateProjectile(i, projectile.trajectory, where, target - where, projectile.speed, OnHit);
        }
        
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

public class ModifierSpell
{
    public Spell baseSpell;

    public string name = "No name specified";
    public string description = "No description specified";
    public float damage_multiplier = 1f;
    public float mana_multiplier = 1f;
    public float speed_multiplier = 1f;
    public int mana_adder = 0;
    public float cooldown_multiplier = 1f;
    public int projectile_adder = 0;

    public void CastModifiedSpell() 
    {
        baseSpell.mana_cost = baseSpell.mana_cost + mana_adder;
        baseSpell.mana_cost = Mathf.RoundToInt(baseSpell.mana_cost * mana_multiplier);
        baseSpell.damage.amount = Mathf.RoundToInt(baseSpell.damage.amount * damage_multiplier);
        baseSpell.projectile.speed = Mathf.RoundToInt(baseSpell.projectile.speed * speed_multiplier);
        baseSpell.cooldown = baseSpell.cooldown * cooldown_multiplier;
        baseSpell.N += projectile_adder;

        baseSpell.Cast(Vector3.zero, Vector3.zero, Hittable.Team.PLAYER); // Debug values atm, had to throw in something fast
    }
}
