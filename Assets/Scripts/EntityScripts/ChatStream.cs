using UnityEngine;

public class ChatStream : StreamEntity
{
    protected override void NormalBehavior()
    {
        base.NormalBehavior();
        RollChanceToTalk();
    }
}