using UnityEngine;

public abstract class PlayerStatusEffect : StatusEffect
{
}

public class PlayerStatusEffectData : StatusEffectData
{
    public PlayerData playerData;
}

public abstract class PlayerStatusEffect<DataType> : PlayerStatusEffect
    where DataType : PlayerStatusEffectData, new()
{

    public DataType Data;
    public PlayerManager Target;
    public IStatusEffectSource Source;
    public override void Apply(IStatusEffectable target, IStatusEffectSource source = null)
    {
        if (target is PlayerManager playerTarget)
        {
            Apply(playerTarget, source);
        }
    }

    public override StatusEffectData GetData()
    {
        return Data;
    }
    public override IStatusEffectable GetTarget()
    {
        return Target;
    }


    public override IStatusEffectSource GetSource()
    {
        return Source;
    }


    public virtual void Apply(PlayerManager target, IStatusEffectSource source = null)
    {
        Target = target;
        Source = source;

    }

    public override void OnUpdate(float deltaTime)
    {
        if (Data.ExpireAfterLifetime)
        {
            Data.ModifierLifetime -= deltaTime;
            if (Data.ModifierLifetime <= 0)
            {
                Data.ExpireNextUpdate = true;
            }
        }
    }

    public override bool TryAddStack(int stack)
    {
        if (Data.Stack < Data.MaxStack)
        {
            Data.Stack = Mathf.Min(Data.Stack + stack, Data.MaxStack);
            return true;
        }
        return false;
    }

    public override bool ShouldExpire()
    {
        return Data.ExpireNextUpdate;
    }
    public override void Expire()
    {
        if(Data.Stack > 1)
        {
            Data.Stack--;
        }
        else
        {

            Target.RemoveStatusEffect(this);
        }
    } 


}