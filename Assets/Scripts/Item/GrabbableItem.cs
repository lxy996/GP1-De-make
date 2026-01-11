using UnityEngine;

public enum GrabbableType { Ball, Rock, Bottle, Book, Bomb, Slime, Furniture, Key, Potion, Relic }
public class GrabbableItem : MonoBehaviour
{
    public GrabbableType type;
    public bool enemyCanPick = true;
    public bool playerCanPick = true;

    public bool consumable = false;    
    public enum PickupRequirement { None, RequiresRelic }
    public PickupRequirement requirement = PickupRequirement.None;
    public string requirementId;
}
