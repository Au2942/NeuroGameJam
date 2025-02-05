using UnityEngine;

public abstract class StatusEffect
{
    public abstract StatusEffectData GetData();
    public abstract IStatusEffectable GetTarget();
    public abstract IStatusEffectSource GetSource();
    public abstract void Apply(IStatusEffectable target, IStatusEffectSource source = null);
    public abstract void OnUpdate(float deltaTime);
    public abstract bool TryAddStack(int stack);
    public abstract bool ShouldExpire();
    public abstract void Expire();
}

public class StatusEffectData
{
    [Header("General")]
    public string ID;
    public Sprite Icon;
    public string Name;
    public string Description;
    
    [Header("Stack")]
    public bool Stackable = false;
    public int Stack = 1;
    public int MaxStack = 1;
    [Header("Lifetime")]
    public bool ExpireAfterLifetime = true;
    public float ModifierLifetime;
    public bool ExpireNextUpdate = false;
}
