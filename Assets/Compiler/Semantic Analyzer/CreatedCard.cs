using System.Collections.Generic;
class CreatedCard: CardData
{
    public List<CardEffectCall> OnActivation {get;}

    public CreatedCard (List<CardEffectCall> onActivation)
    {
        OnActivation = onActivation;
    }
}