using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class SpellType : Spell
{
    public SpellType(SpellCaster owner, JObject spells) : base(owner)
    {
        this.owner = owner;
        SetAttributes(spells); 
    }    

    public SpellType(SpellCaster owner) : base(owner)
    {
        this.owner = owner;
    }    
}
