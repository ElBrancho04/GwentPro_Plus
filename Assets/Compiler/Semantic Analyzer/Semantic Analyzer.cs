using System.Collections.Generic;
#nullable enable
using UnityEngine;
class SemanticAnalyzer
{
    public List<CreatedCard> Cards {get; private set;}
    public List<Error> Errors {get; private set;}
    public Context context {get;}

    public SemanticAnalyzer (Context context)
    {
        this.context = context;
        Cards = new List<CreatedCard>();
        Errors = new List<Error>();
    }

    private void ReportError(string message, ASTNode node)
    {
        Errors.Add(new Error(node.Line, node.Pos, message));
    }

    public string Analyze (string Source)
    {
        Cards = new List<CreatedCard>();
        Errors = new List<Error>();
        string errors = "";

        List<Token> tokens = new Lexer(Source).Tokenize();
        foreach (var item in tokens)
        {
            Debug.Log(item);
        }
        Parser parser = new Parser (tokens);
        ASTNode program = parser.Parse();
        if (parser.Errors.Count > 0)
        {
            errors = "No se pudo compilar el texto, tienes los siguientes errores gramaticales:";
            foreach (var item in parser.Errors)
            {
                //Debug.Log(item);
                errors += "\n" + item;
            }
            return errors;
        }

        foreach (var item in ((MultiChildNode)program).Children)
        {
            if (item != null && (string)item.Value == "effect")
            {
                int[] componentsCant = {0,0,0};
                object[] componentsValues = new object[3];
                foreach (var component in ((MultiChildNode)item).Children)
                {
                    if (component != null)
                    {
                        if ((string)component.Value == "Name")
                        {
                            componentsCant[0]++;
                            componentsValues[0] = (string)((UnaryNode)component).Child.Value;
                            foreach (var x in context.CreatedEffects)
                            {
                                if ((string)componentsValues[0] == x.Name)
                                {
                                    ReportError("Este nombre de effect ya existe", component);
                                    componentsCant[0]++;
                                    continue;
                                }
                            }
                        }
                        else if ((string)component.Value == "Params")
                        {
                            componentsCant[1]++;
                            componentsValues[1] = new Dictionary<string, string>();
                            foreach (var param in (((MultiChildNode)component).Children))
                            {
                                if (((Dictionary<string,string>)componentsValues[1]).ContainsKey((string)((BinaryNode)param).Right.Value))
                                {
                                    ReportError("Este nombre de parámetro ya existe", ((BinaryNode)param).Right);
                                    componentsCant[1]++;
                                    continue;
                                }
                                ((Dictionary<string,string>)componentsValues[1])[(string)((BinaryNode)param).Right.Value] = (string)((BinaryNode)param).Left.Value;
                            }
                        }
                        else
                        {
                            componentsCant[2]++;
                            componentsValues[2] = component;
                        }
                    }
                }
                if (componentsCant[0] == 1 && componentsCant[1] < 2 && componentsCant[2] == 1)
                {
                    context.CreatedEffects.Add(new CreatedEffect((string)componentsValues[0], (Dictionary<string,string>)componentsValues[1], (ASTNode)componentsValues[2]));
                }
                else
                ReportError("Error en la cantidad de componentes del effect", item);
            }
        }
        foreach (var item in ((MultiChildNode)program).Children)
        {
            if ((string)item.Value == "card")
            {
                int[] componentsCant = {0,0,0,0,0,0};
                object[] componentsValues = new object[6];
                foreach (var component in ((MultiChildNode)item).Children)
                {
                    if (component != null)
                    {
                        if ((string)component.Value == "Type")
                        {
                            componentsCant[0]++;
                            componentsValues[0] = (string)((UnaryNode)component).Child.Value;
                        }
                        else if ((string)component.Value == "Name")
                        {
                            componentsCant[1]++;
                            componentsValues[1] = (string)((UnaryNode)component).Child.Value;
                            foreach (var cardName in context.cardNames)
                            {
                                if ((string)componentsValues[1] == cardName)
                                {
                                    ReportError("Este nombre de carta ya existe", component);
                                    componentsCant[1]++;
                                    break;
                                }
                            }
                        }
                        else if ((string)component.Value == "Faction")
                        {
                            componentsCant[2]++;
                            componentsValues[2] = (string)((UnaryNode)component).Child.Value;
                        }
                        else if ((string)component.Value == "Power")
                        {
                            componentsCant[3]++;
                            componentsValues[3] = (int)((UnaryNode)component).Child.Value;
                        }
                        else if ((string)component.Value == "Range")
                        {
                            componentsCant[4]++;
                            List<string> range = new List<string>();
                            foreach (var rangeType in ((MultiChildNode)component).Children)
                            {
                                if (rangeType != null)
                                range.Add((string)rangeType.Value);
                            }
                            componentsValues[4] = range;
                        }
                        else if ((string)component.Value == "OnActivation")
                        {
                            componentsCant[5]++;
                            List<CardEffectCall> onActEffects = new List<CardEffectCall>();
                            foreach (var cardEffectCall in ((MultiChildNode)component).Children)
                            {
                                if (AnalyzeEffectCall(cardEffectCall) == null)
                                {
                                    componentsCant[5]++;
                                }
                                else
                                {
                                    onActEffects.Add(AnalyzeEffectCall(cardEffectCall));
                                }
                            }
                            componentsValues[5] = onActEffects;
                        }
                    }
                }
                bool declarationValid = true;
                for (int i = 0; i < 6; i++)
                {
                    if (componentsCant[i] != 1)
                    {
                        declarationValid = false;
                        break;
                    }
                }
                if (!declarationValid)
                {
                    ReportError("Error en la declaración de card, componentes faltantes o repetidos", item);
                    continue;
                }
                CreatedCard card = new CreatedCard((List<CardEffectCall>)(componentsValues[5]));
                card.type = (string)componentsValues[0];
                card.cardName = (string)componentsValues[1];
                card.faction = (string)componentsValues[2];
                card.attackPower = (int)componentsValues[3];
                card.range = (List<string>)componentsValues[4];
                card.id = -1;
                card.cardType = CardType.Unit;
                card.cardDescription = card.type + "\n" + card.faction;
                card.isGold = false;
                card.player = 1;
                card.playEfect = false;
                foreach (var rnge in card.range)
                {
                    switch (rnge)
                    {
                        case "Melee":
                            card.melee = true;
                            break;
                        case "Ranged":
                            card.ranged = true;
                            break;
                        default:
                            card.siege = true;
                            break;
                    }
                }
                Cards.Add(card);
                context.cardNames.Add(card.name);
            }
        }

        CardEffectCall? AnalyzeEffectCall(ASTNode effectCall, bool necessarySelector = true)
        {
            int[] componentsCant = {0,0,0,0,0,0};
            object[] componentsValues = new object[6];
            componentsValues[5] = new List<CardEffectCall>();
            foreach (var item in ((MultiChildNode)effectCall).Children)
            {
                if ((string)item.Value == "Name")
                {
                    string name = (string)((UnaryNode)item).Child.Value;
                    foreach (var effect in context.CreatedEffects)
                    {

                        if (effect.Name == name)
                        {
                            return new CardEffectCall(name, null, null, null, null, null);
                        }                        
                    }
                    ReportError("El nombre de efecto no existe", item);
                    return null;
                }
                else if ((string)item.Value == "Effect")
                {
                    componentsCant[1]++;
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    foreach (var param in ((MultiChildNode)item).Children)
                    {
                        if ((string)param.Value == "Name")
                        {
                            if (!(((UnaryNode)param).Child.Value is string))
                            {
                                ReportError("Los nombres de efectos deben ser tipo string", param);
                                return null;
                            }
                            componentsCant[0]++;
                            componentsValues[0] = (string)((UnaryNode)param).Child.Value;
                        }
                        else
                        {
                            if (parameters.ContainsKey((string)param.Value) && parameters[(string)param.Value] != null)
                            {
                                ReportError("El parametro ya había sido asignado", param);
                                return null;
                            }
                            parameters[(string)param.Value] = ((UnaryNode)param).Child.Value;                         
                        }
                    }
                    if (componentsCant[0] != 1)
                    {
                        ReportError("Faltó el nombre del efecto llamado o se insertaron varios", item);
                        return null;
                    }
                    bool exist = false;
                    Dictionary<string, string> effectParams = new Dictionary<string, string>(); 
                    foreach (var effect in context.CreatedEffects)
                    {
                        if (effect.Name == (string)(componentsValues[0]))
                        {
                            exist = true;
                            effectParams = effect.Params;
                        };
                    }
                    if (!exist)
                    {
                        ReportError("No existe efecto alguno con este nombre", item);
                        return null;
                    }

                    Dictionary<string, bool> parametersValidation = new Dictionary<string, bool>();
                    if (effectParams != null)
                    foreach (var key in parameters.Keys)
                    {
                        foreach (var param in effectParams.Keys)
                        {
                            if (param == key)
                            {
                                if (parameters[key] is string)
                                {
                                    if (effectParams[param] == "String")
                                    parametersValidation[key] = true;
                                    else 
                                    ReportError($"El parametro {key} debe ser de tipo String", item);
                                }
                                else if (parameters[key] is int)
                                {
                                    if (effectParams[param] == "Number")
                                    parametersValidation[key] = true;
                                    else 
                                    ReportError($"El parametro {key} debe ser de tipo Number", item);
                                }
                                else
                                {
                                    if (effectParams[param] == "Bool")
                                    parametersValidation[key] = true;
                                    else 
                                    ReportError($"El parametro {key} debe ser de tipo Bool", item);
                                }
                            }
                        }
                    }
                    bool allCheck = true;
                    foreach (var key in parametersValidation.Keys)
                    {
                        if (parametersValidation[key] == false)
                        allCheck = false;
                    }
                    if (allCheck)
                    {
                        componentsValues[1] = parameters;
                    }
                    else
                    componentsCant[1]++;       
                }
                else if ((string)item.Value == "Selector")
                {
                    foreach (var param in ((MultiChildNode)item).Children)
                    {
                        if ((string)param.Value == "Source")
                        {
                            componentsCant[2]++;
                            componentsValues[2] = ((UnaryNode)param).Child.Value;
                        }
                        else if ((string)param.Value == "Single")
                        {
                            componentsCant[3]++;
                            componentsValues[3] = bool.Parse(((string)((UnaryNode)param).Child.Value).ToLower());
                        }
                        else
                        {
                            componentsCant[4]++;
                            if (((UnaryNode)param).Child != null)
                            componentsValues[4] = ((UnaryNode)param).Child;
                        }
                    }
                }
                else
                {
                    if (AnalyzeEffectCall(item, false) == null)
                    {
                        ReportError("Declaracion de PostActionInválida", item);
                        return null;
                    }
                    componentsCant[5]++;
                    ((List<CardEffectCall>)componentsValues[5]).Add(AnalyzeEffectCall(item, false)); 
                }
            }
            for (int i = 0; i < 5; i++)
            {
                if (componentsCant[i] > 1)
                {
                    ReportError("Declaración repetida de componentes", effectCall);
                    return null;
                }

            }
            for (int i = 2; i < 5; i++)
            {
                if (componentsCant[i] == 0 && necessarySelector)
                {
                    ReportError("Faltan componentes por declarar en el Selector", effectCall);
                    return null;
                }
            }
            return new CardEffectCall((string)(componentsValues[0]), (Dictionary<string, object>)(componentsValues[1]), (string)(componentsValues[2]), (bool)(componentsValues[3]), (BinaryNode)(componentsValues[4]), (List<CardEffectCall>)(componentsValues[5]));
        }

        if (Errors.Count > 0)
        {
            errors = "ERRORES:\n";
            foreach (var item in Errors)
            {
                string err = item.ToString();
                errors += err += "\n";
            }
        }
        return errors;
    }
}