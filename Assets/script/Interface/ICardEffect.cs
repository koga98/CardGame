using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

public interface ICardEffect 
{
    Task Apply(ApplyEffectEventArgs e);
}
