using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{
    // Generate a random spell from the assignment 2 desciption and the 2nd requirement. 
    public Spell Build(SpellCaster owner)
    {
        return new Spell(owner);
    }

   
    public SpellBuilder()
    {        
    }

}
