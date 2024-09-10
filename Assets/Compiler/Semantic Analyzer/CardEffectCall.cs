#nullable enable
using System.Collections.Generic;
class CardEffectCall
{
    // Effect
    public string Name {get;}
    public Dictionary<string, object>? Parameters {get;}

    // Selector
    public string? Source {get;}
    public bool? Single {get;}
    public BinaryNode? Predicate {get;}

    // PostAction (opcional)
    public List<CardEffectCall>? PostAction{get;}

    public CardEffectCall (string name, Dictionary<string, object>? parameters, string? source, bool? single, BinaryNode? predicate, List<CardEffectCall>? postAction)
    {
        Name = name;
        Parameters = parameters;
        Source = source;
        Single = single;
        Predicate = predicate;
        PostAction = postAction;
    }
}