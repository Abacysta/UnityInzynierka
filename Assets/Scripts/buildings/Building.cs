using System.ComponentModel.Design;
using UnityEngine;

public abstract class Building : ScriptableObject
{
    public string buildingName;
    public Sprite buildingIcon;
    public int buildingLevel=0;
    public string description;
    public Technology requiredTechnology;
    public int populationRequirement;

    public abstract void ApplyEffect();
    public abstract void Upgrade();
}
