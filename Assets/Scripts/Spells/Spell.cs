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
    public Projectile projectile;
    public Projectile secondary_projectile;
    public float? delay;
    public float? angle;

    public List<ValueMod> DamageModifiers = new List<ValueMod>();
    public List<ValueMod> ManaCostModifiers = new List<ValueMod>();
    public List<ValueMod> CooldownModifiers = new List<ValueMod>();
    

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

    public void OnHit(Hittable other, Vector3 impact)
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
    public List<ValueMod> SpeedModifiers = new List<ValueMod>();
}


public class ValueMod
{
    public delegate float Modifier(float value);
    private Modifier modifier;

    public ValueMod(Modifier modifier)
    {
        this.modifier = modifier;
    }

    public float Apply(float value)
    {
        return modifier(value);
    }

    public static float ApplyAll(float baseValue, List<ValueMod> mods)
    {
        float moddedValue = baseValue;
        foreach (ValueMod mod in mods)
        {
            moddedValue = mod.Apply(moddedValue);
        }
        return moddedValue;
    }
}

public class ModifierSpell : Spell
{
    private Spell baseSpell;

    public ModifierSpell(Spell baseSpell) : base(baseSpell.owner)
    {
        this.baseSpell = baseSpell;
    }

    public void AddDamageModifier(ValueMod modifier)
    {
        baseSpell.DamageModifiers.Add(modifier);
    }

    public void AddManaCostModifier(ValueMod modifier)
    {
        baseSpell.ManaCostModifiers.Add(modifier);
    }

    public void AddCooldownModifier(ValueMod modifier)
    {
        baseSpell.CooldownModifiers.Add(modifier);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        return baseSpell.Cast(where, target, team);
    }
}
