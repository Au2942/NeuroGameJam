using System.Collections.Generic;

public interface ISpeaker
{
    public List<AnimatorClipsPair> DialoguePlayingAnimation {get; set;} 
    public List<AnimatorClipsPair> DialogueTypingAnimation {get; set;}
}