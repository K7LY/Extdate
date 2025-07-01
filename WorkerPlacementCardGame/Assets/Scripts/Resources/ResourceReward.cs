[System.Serializable]
public class ResourceReward
{
    public ResourceType resourceType;
    public int amount;
    
    public ResourceReward() { }
    
    public ResourceReward(ResourceType type, int amt)
    {
        resourceType = type;
        amount = amt;
    }
}

[System.Serializable]
public class ResourceRequirement
{
    public ResourceType resourceType;
    public int amount;
    
    public ResourceRequirement() { }
    
    public ResourceRequirement(ResourceType type, int amt)
    {
        resourceType = type;
        amount = amt;
    }
    
    public bool CanMeet(Player player)
    {
        return player.GetResource(resourceType) >= amount;
    }
}