using System.Collections.Generic;
public class CreatedEffect
{
    public string Name {get;}
    public Dictionary<string, string> Params {get;}
    public ASTNode Action {get;}

    public CreatedEffect (string name, Dictionary<string, string> parameters, ASTNode action)
    {
        Name = name;
        Action = action;
        Params = parameters;
    }
}