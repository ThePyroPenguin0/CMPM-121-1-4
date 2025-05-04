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
    public int N = 1; // Count of projectiles
    public float spray = 0f;
    public Damage damage;
    public int mana_cost;
    public float cooldown;
    public Projectile projectile = null;
    public Projectile secondary_projectile = null;
    public float? delay;
    public float? angle;

    public class Projectile
    {
        public string trajectory = "none defined";
        public float speed = 0f;
        public float lifetime = 0f;
        public int sprite = 0;
    }
    public class ModifierSpell
    {
        public string name = "No spell provided."; 
        public string description = "No description provided.";
        private float damage_multiplier = 1;
        private float mana_multiplier = 1;
        private float speed_multiplier = 1;
        private float cooldown_multiplier = 1;
        private string projectile_trajectory = null;
        private int mana_adder = 0;

        public void MultDMG(Spell spell)
        {
            spell.damage.amount = (int)(spell.damage.amount * damage_multiplier);
        }

        public void MultMana(Spell spell)
        {
            spell.mana_cost = (int)(spell.mana_cost * mana_multiplier);
        }
        public void MultSpeed(Spell spell)
        {
            spell.projectile.speed = (int)(spell.projectile.speed * speed_multiplier);
        }
        public void MultCooldown(Spell spell)
        {
            spell.cooldown = (int)(spell.cooldown * cooldown_multiplier);
        }
        public void AddMana(Spell spell)
        {
            spell.mana_cost += mana_adder;
        }
        public void setProjectileTrajectory(Spell spell)
        {
            spell.projectile.trajectory = projectile_trajectory;
        }
    }


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
        for (int i = 0; i < N; i++)
        {
            GameManager.Instance.projectileManager.CreateProjectile(i, projectile.trajectory, where, target - where, projectile.speed, OnHit);
        }

        yield return new WaitForEndOfFrame();
    }

    public void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), damage.type));
        }

    }

}