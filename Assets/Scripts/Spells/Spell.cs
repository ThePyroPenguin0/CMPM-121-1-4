using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    private string name;
    private string description;
    private int icon;
    private int damage; 
    private JObject damageAmount;
    private int manaCost;
    private int cooldown;
    private Dictionary<string, string> projectile;

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
        return manaCost;
    }

    public int GetDamage()
    {
        return damage;
    }

    public int GetCooldown()
    {
        return cooldown;
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public void SetName(string theName)
    {
        name = theName;
    }

    public void SetManaCost(int theManaCost)
    {
        manaCost = theManaCost;
    }

    public void SetDamage(JObject theDamage)
    {
        damageAmount = theDamage;
    }

    public void SetCooldown(int theCooldown)
    {
        cooldown = theCooldown;
    }

    public void SetIcon(int theIcon)
    {
        icon = theIcon; 
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

    }

    public virtual void SetAttributes(JObject attributes){
        SetName(attributes.Value<string>("name"));
        SetManaCost(attributes.Value<int>("mana_cost"));
        SetDamage(attributes.Value<JObject>("damage"));
        SetCooldown(attributes.Value<int>("cooldown"));
        SetIcon(attributes.Value<int>("icon"));
    }

}
