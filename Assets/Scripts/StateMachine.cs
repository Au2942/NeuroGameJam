using UnityEngine;
using System.Collections.Generic;

//T : state identifier
//TContext : hold data that are shared between multiple states (eg. entity data, worker data etc.) 
public class StateMachine<T, TContext>
{
    private Dictionary<T, IState<TContext>> states = new Dictionary<T, IState<TContext>>();
    private IState<TContext> currentState;
    private TContext context;

    public StateMachine(TContext context)
    {
        this.context = context;
    }

    public void AddState(T key, IState<TContext> state)
    {
        states[key] = state;
    }

    public void ChangeState(T newState)
    {
        if (currentState != null)
        {
            currentState.Exit(context);
        }

        if(states.TryGetValue(newState, out IState<TContext> state))
        {
            currentState = state;
            currentState.Enter(context);
        }
        else
        {
            Debug.LogWarning(newState + " state not found.");
        }
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.Update(context);
        }
    }

    public T GetCurrentStateKey()
    {
        foreach (var pair in states)
        {
            if (pair.Value == currentState)
            {
                return pair.Key;
            }
        }
        return default;
    }


}


public interface IState<TContext>
{
    void Enter(TContext context);
    void Update(TContext context);
    void Exit(TContext context);
}