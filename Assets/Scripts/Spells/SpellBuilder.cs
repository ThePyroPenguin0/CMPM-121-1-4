using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class SpellBuilder 
{
    // Generate a random spell from the assignment 2 desciption and the 2nd requirement. 
    public static Spell Build(SpellCaster owner, Dictionary<string, JObject> spells)
    {
        int randomEntry = Random.Range(0, spells.Count);
        var kvp = spells.ElementAt(randomEntry);
        //SpellType theSpell = new SpellType(owner, kvp.Value);
        return new SpellType(owner, kvp.Value);
    }

    public Spell Build(SpellCaster owner)
    {
        return new SpellType(owner);
    }

    public SpellBuilder(){
    }

}
