using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    private JObject damageObject;
    private JObject secondary_damage_object;

    // New properties for asgn2
    public string name;
    public string description;
    private int icon;
    private int N = 1; // Count of projectiles
    private float spray = 0f;
    private Damage damage;
    private Damage secondary_damage;
    private int mana_cost;
    private float cooldown;
    public Projectile projectile;
    public Projectile secondary_projectile;
    private float? delay;
    private float? angle;

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

    public int GetSecondaryDamage(){
        return secondary_damage.amount;
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public int GetN(){
        return N; 
    }

    public string GetDescription(){
        return description;
    }

    public Projectile GetProjectile(){
        return projectile;
    }

    public Projectile GetSecondaryProjectile(){
        return secondary_projectile;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public void SetManaCost(int mana_cost)
    {
        this.mana_cost = mana_cost;
    }

    public void SetDamage(JObject damage)
    {
        this.damageObject = damage;
    }

    public void SetSecondaryDamage(JObject secondary_damage){
        this.secondary_damage_object = secondary_damage;
    }

    public void SetCooldown(float cooldown)
    {
        this.cooldown = cooldown;
    }

    public void SetIcon(int icon)
    {
        this.icon = icon; 
    }

    public void SetN(int N){
        this.N = N;
    }

    public void SetDescription(string description){
        this.description = description;
    }

    public void SetProjectile(Projectile projectile){
        this.projectile = projectile;
    }

    public void SetSecondProjectile(Projectile secondary_projectile){
        this.secondary_projectile = secondary_projectile;
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

    public virtual void SetAttributes(JObject attributes){
        if(attributes.ContainsKey("name")){
            SetName(attributes.Value<string>("name"));
        }
        
        if(attributes.ContainsKey("mana_cost")){
            SetManaCost(attributes.Value<int>("mana_cost"));
        }

        if(attributes.ContainsKey("damage")){
            SetDamage(attributes.Value<JObject>("damage"));
        }

        if(attributes.ContainsKey("secondary_damage")){
            SetDamage(attributes.Value<JObject>("secondary_damage"));
        }
        
        if(attributes.ContainsKey("cooldown")){
            SetCooldown(attributes.Value<int>("cooldown"));
        }
        
        if(attributes.ContainsKey("icon")){
            SetIcon(attributes.Value<int>("icon"));
        }

        if(attributes.ContainsKey("N")){
            SetIcon(attributes.Value<int>("N"));
        }
        
        if(attributes.ContainsKey("description")){
            SetIcon(attributes.Value<int>("description"));
        }

        if(attributes.ContainsKey("projectile")){
            SetProjectile(attributes.Value<Projectile>("projectile"));
        }
        
        if(attributes.ContainsKey("secondary_projectile")){
            SetSecondProjectile(attributes.Value<Projectile>("secondary_projectile")); 
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
