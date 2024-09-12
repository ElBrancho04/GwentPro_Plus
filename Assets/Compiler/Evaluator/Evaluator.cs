using System;
using System.Collections;
using System.Collections.Generic;
#nullable enable

class Evaluator
{
    private Stack<Dictionary<string, object>> scopes = new Stack<Dictionary<string, object>>();
    public List<Error> Errors { get; private set; }
    private readonly Context context;
    private readonly CardEffectCall efctCall;
    private readonly CreatedCard card;

    public Evaluator(CreatedCard card, Context context, CardEffectCall efctCall)
    {
        this.card = card;
        this.context = context;
        this.efctCall = efctCall;
        Errors = new List<Error>();
        scopes = new Stack<Dictionary<string, object>>();
    }

    public void Evaluate(List<CardData>? parentTargets = null)
    {
        object? action = null;
        foreach (var item in context.CreatedEffects)
        {
            if (item.Name == efctCall.Name)
            {
                action = item.Action;
                break;
            }
        }
        List<CardData> targets = new List<CardData>();

        if (efctCall.Source != null)
        {
            if (efctCall.Single == null || efctCall.Predicate == null)
            {
                ReportError($"La carta {card.cardName} que llamó a este no posee un Selector válido", (ASTNode)action);
                return;
            }
            var source = new List<CardData>();
            switch (efctCall.Source)
            {
                case "board":
                    source = Context.Board;
                    break;
                case "hand":
                    source = Context.TriggerPlayer == 1 ? Context.HandOfPlayer1 : Context.HandOfPlayer2;
                    break;
                case "otherHand":
                    source = Context.TriggerPlayer == 2 ? Context.HandOfPlayer1 : Context.HandOfPlayer2;
                    break;
                case "deck":
                    source = Context.TriggerPlayer == 1 ? Context.DeckOfPlayer1 : Context.DeckOfPlayer2;
                    break;
                case "otherDeck":
                    source = Context.TriggerPlayer == 2 ? Context.DeckOfPlayer1 : Context.DeckOfPlayer2;
                    break;
                case "field":
                    source = Context.TriggerPlayer == 1 ? Context.FieldOfPlayer1 : Context.FieldOfPlayer2;
                    break;
                default:
                    source = Context.TriggerPlayer == 2 ? Context.FieldOfPlayer1 : Context.FieldOfPlayer2;
                    break;
            }
            source = new List<CardData>(source);
            source = Find(source, efctCall.Predicate);
            if ((bool)efctCall.Single)
            {
                var aux = new List<CardData>(source);
                source.Clear();
                source.Add(aux[0]);
            }
            targets = new List<CardData>(source);
        }
        else if (parentTargets != null)
        targets = new List<CardData>(parentTargets);

        EnterScope();
        if (efctCall.Parameters != null)
        foreach (var item in efctCall.Parameters.Keys)
        {
            AssignNewVariable(item, efctCall.Parameters[item], (ASTNode)action);
        }
        AssignNewVariable((string)Left(Left((ASTNode)action)).Value, targets, (ASTNode)action);
        AssignNewVariable((string)Right(Left((ASTNode)action)).Value, context, (ASTNode)action);
        EvaluateBlock(Right((ASTNode)action));

        if (efctCall.PostAction != null)
        foreach (var item in efctCall.PostAction)
        {
            Evaluator evaluator = new Evaluator(card, context, item);
            evaluator.Evaluate(targets);
            Errors.AddRange(evaluator.Errors);
        }
        return;
    }

    private void ReportError(string message, ASTNode node)
    {
        Errors.Add(new Error(node.Line, node.Pos, message));
    }
    private void EnterScope()
    {
        scopes.Push(new Dictionary<string, object>());
    }
    private void ExitScope()
    {
        scopes.Pop();
    }
    public ASTNode Left(ASTNode node)
    {
        return ((BinaryNode)node).Left;
    }
    public ASTNode Right(ASTNode node)
    {
        return ((BinaryNode)node).Right;
    }
    public object? GetVal(ASTNode node)
    {
        return EvaluateExpression(node);
    }
    private object? LookupVariable(string name, ASTNode node)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope[name];
            }
        }

        ReportError($"La variable '{name}' no está definida.", node);
        return null;
    }

    private void AssignNewVariable(string name, object value, ASTNode node)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name))
            {
                ReportError($"La variable ya está definida.", node);
                return;
            }
        }
        scopes.Peek()[name] = value;
    }

    private void UpdateVariable(string name, object value, ASTNode node)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name))
            {
                scope[name] = value;
                return;
            }
        }

        ReportError($"La variable '{name}' no está definida.", node);
    }

    private void EvaluateBlock(ASTNode block)
    {
        if (block.NodeType == NodeType.Assigment || block.NodeType == NodeType.FunctionCall)
        {
            EvaluateStatement(block);
            return;
        }
        foreach (var stmnt in ((MultiChildNode)block).Children)
        {
            EvaluateStatement(stmnt);
        }
    }

    public void EvaluateStatement(ASTNode node)
    {
        switch ((string)node.Value)
        {
            case "if":
                EvaluateIfStatement(node);
                break;

            case "while":
                EvaluateWhileStatement(node);
                break;

            case "for":
                EvaluateForStatement(node);
                break;

            default:
                if (node is BinaryNode)
                {
                    if (node.NodeType == NodeType.Assigment)
                        EvaluateBinaryAssignment(node);
                    else
                        EvaluateFunctionCallStatement(node);
                }
                else if (node is UnaryNode)
                    EvaluateUnaryAssignment(node);
                else
                    ReportError("Tipo de statement desconocido.", node);
                break;
        }
    }

    private void EvaluateIfStatement(ASTNode node)
    {
        var condition = GetVal(Left(node));

        if (condition is bool)
        {
            if ((bool)condition)
            {
                EnterScope();
                EvaluateBlock(Right(node));
                ExitScope();
            }
        }
        else
        {
            ReportError("La condición del 'if' debe ser una expresión booleana.", node);
        }
    }

    private void EvaluateWhileStatement(ASTNode node)
    {
        var condition = GetVal(Left(node));

        if (condition is bool)
        {
            while ((bool)condition)
            {
                EnterScope();
                EvaluateBlock(Right(node));
                ExitScope();
            }
        }
        else
        {
            ReportError("La condición del 'while' debe ser una expresión booleana.", node);
        }
    }

    private void EvaluateForStatement(ASTNode node)
    {
        var iteratorVarNode = ((MultiChildNode)node).Children[0];
        var collection = GetVal(((MultiChildNode)node).Children[1]);

        if (iteratorVarNode.NodeType != NodeType.Identifier || collection is not List<CardData>)
        {
            ReportError("El 'for' necesita un iterador válido y una colección que sea List<CardData>.", node);
            return;
        }

        var iteratorVar = (string)iteratorVarNode.Value;
        if (((MultiChildNode)node).Children[2] == null)
        {
            ReportError("El cuerpo del for es inválido", node);
            return;
        }
        foreach (var item in (List<CardData>)collection)
        {
            EnterScope();
            AssignNewVariable(iteratorVar, item, Left(node));
            EvaluateBlock(((MultiChildNode)node).Children[2]);
            ExitScope();
        }
    }

    private void EvaluateBinaryAssignment(ASTNode node)
    {
        var leftNode = Left(node);
        if (leftNode.NodeType != NodeType.Identifier)
        {
            if (Left(node).NodeType == NodeType.PropertyAcces && Right(Left(node)).Value == "Power")
            {
                EvaluateCardPowerAssignment(node);
                return;
            }
            ReportError("Se esperaba una variable a la izquierda de la operación de asignación.", node);
            return;
        }

        var variableName = (string)leftNode.Value;
        var rightValue = GetVal(Right(node));

        if (rightValue == null)
        {
            ReportError("El valor a la derecha de la asignación no es válido.", node);
            return;
        }

        switch ((string)node.Value)
        {
            case "=":
                if (LookupVariable(variableName, leftNode).GetType().Equals(rightValue.GetType()))
                    UpdateVariable(variableName, rightValue, node);
                else
                    ReportError("El tipo de la variable es distinto del de la expresión", node);
                break;

            case "+=":
                if (rightValue is int && LookupVariable(variableName, leftNode) is int)
                    UpdateVariable(variableName, (int)LookupVariable(variableName, leftNode) + (int)rightValue, node);
                else
                    ReportError("El operador '+=' solo se puede aplicar a variables de tipo entero.", node);
                break;

            case "-=":
                if (rightValue is int && LookupVariable(variableName, leftNode) is int)
                    UpdateVariable(variableName, (int)LookupVariable(variableName, leftNode) - (int)rightValue, node);
                else
                    ReportError("El operador '-=' solo se puede aplicar a variables de tipo entero.", node);
                break;

            default:
                ReportError("Operador de asignación desconocido.", node);
                break;
        }
    }

    private void EvaluateUnaryAssignment(ASTNode node)
    {
        var leftNode = ((UnaryNode)node).Child;
        if (leftNode.NodeType != NodeType.Identifier)
        {
            if (Left(node).NodeType == NodeType.PropertyAcces && Right(Left(node)).Value == "Power")
            {
                EvaluateCardPowerAssignment(node);
                return;
            }
            ReportError("Se esperaba una variable para la operación unaria.", node);
            return;
        }

        var variableName = (string)leftNode.Value;
        var variable = LookupVariable(variableName, leftNode);

        if (variable is int)
        {
            switch ((string)node.Value)
            {
                case "++":
                    UpdateVariable(variableName, (int)variable + 1, node);
                    break;
                case "--":
                    UpdateVariable(variableName, (int)variable - 1, node);
                    break;
                default:
                    ReportError("Operador unario desconocido.", node);
                    break;
            }
        }
        else
        {
            ReportError("El operador unario solo se puede aplicar a variables de tipo entero.", node);
        }
    }

    private void EvaluateCardPowerAssignment(ASTNode node)
    {
        if (GetVal(Left(Left(node))) is not CardData)
        {
            ReportError("El objeto no tiene la propiedad Power", node);
            return;
        }
        var powerLeft = GetVal(Left(node));
        var right = node is BinaryNode ? GetVal(Right(node)) : null;

        if (node is BinaryNode && right is not int)
        {
            ReportError("Al Power solo se le puede asignar enteros", node);
            return;
        }
        switch ((string)node.Value)
        {
            case "++":
                powerLeft = (int)powerLeft + 1;
                break;
            case "--":
                powerLeft = (int)powerLeft - 1;
                break;
            case "+=":
                powerLeft = (int)powerLeft + (int)right;
                break;
            case "-=":
                powerLeft = (int)powerLeft - (int)right;
                break;
            default:
                powerLeft = (int)right;
                break;
        }
    }

    private void EvaluateFunctionCallStatement(ASTNode node)
    {
        var left = GetVal(Left(node));
        string name = (string)Left(Right(node)).Value;
        object? param = Right(Right(node));
        if (left is not List<CardData> && name != "Push" && name != "SendBottom" && name != "Pop" && name != "Remove" && name != "Shuffle")
        {
            ReportError("La parte izquierda es inválida o la función es inválida como statement", node);
            return;
        }
        if (name == "Push" || name == "SendBottom" || name == "Remove")
        {
            if (param is not CardData)
            {
                ReportError("El parámetro es inválido", node);
                return;
            }
        }
        else if (param != null)
        {
            ReportError("Esta función no recibe parámetros", node);
            return;
        }
        switch (name)
        {
            case "Push":
                ((List<CardData>)left).Insert(0, (CardData)param);
                break;
            case "SendBottom":
                ((List<CardData>)left).Add((CardData)param);
                break;
            case "Pop":
                ((List<CardData>)left).RemoveAt(0);
                break;
            case "Remove":
                ((List<CardData>)left).Remove((CardData)param);
                break;
            default:
                List<CardData> cards = new List<CardData>((List<CardData>)left);
                ((List<CardData>)left).Clear();
                for (int i = 0; i < cards.Count; i++)
                {
                    Random random = new Random();
                    var value = random.Next(cards.Count);
                    ((List<CardData>)left).Add(cards[value]);
                    cards.RemoveAt(value);
                }
                break;
        }
    }

    public object? EvaluateExpression(ASTNode node)
    {
        switch (node.NodeType)
        {
            case NodeType.BinaryOp:
                return EvaluateBinaryOperation(node);
            case NodeType.UnaryOp:
                return EvaluateUnaryOperation(node);
            case NodeType.Literal:
                return EvaluateLiteral(node);
            case NodeType.Identifier:
                return EvaluateIdentifier(node);
            case NodeType.PropertyAcces:
                return EvaluatePropertyAccess(node);
            case NodeType.FunctionCall:
                return EvaluateFunctionCall(node);
            default:
                ReportError("Nodo de expresión desconocido.", node);
                return null;
        }
    }

    private object? EvaluateBinaryOperation(ASTNode node)
    {
        var left = GetVal(Left(node));
        var right = GetVal(Right(node));

        if (left == null || right == null)
        {
            ReportError("Operación fallida debido a operandos nulos.", node);
            return null;
        }

        switch ((string)node.Value)
        {
            case "+":
            case "-":
            case "*":
            case "/":
                if (left is int && right is int)
                    return EvaluateArithmeticOperation((string)node.Value, (int)left, (int)right);
                else
                {
                    ReportError("Los operandos de la operación aritmética deben ser de tipo entero.", node);
                    return null;
                }
            case "==":
            case "!=":
                if (!(left.GetType() == right.GetType()))
                {
                    ReportError("Los operandos no son del mismo tipo", node);
                    return null;
                }
                return EvaluateComparisonOperation((string)node.Value, left, right);
            case "<":
            case ">":
            case "<=":
            case ">=":
                if (left is int && right is int)
                    return EvaluateComparisonOperation((string)node.Value, (int)left, (int)right);
                else
                {
                    ReportError("Los operandos de la operación de comparación deben ser de tipo entero.", node);
                    return null;
                }
            case "&&":
            case "||":
                if (left is bool && right is bool)
                    return EvaluateLogicalOperation((string)node.Value, (bool)left, (bool)right);
                else
                {
                    ReportError("Los operandos de la operación lógica deben ser de tipo booleano.", node);
                    return null;
                }
            case "@":
                if (left is string && right is string)
                    return (string)left + (string)right;
                else
                {
                    ReportError("Los operandos de la concatenación con '@' deben ser de tipo string.", node);
                    return null;
                }
            case "@@":
                if (left is string && right is string)
                    return (string)left + " " + (string)right;
                else
                {
                    ReportError("Los operandos de la concatenación con '@@' deben ser de tipo string.", node);
                    return null;
                }
            default:
                ReportError("Operador binario desconocido.", node);
                return null;
        }
    }

    private object? EvaluateUnaryOperation(ASTNode node)
    {
        var operand = EvaluateExpression(((UnaryNode)node).Child);

        if (operand == null)
        {
            ReportError("Operando nulo en la operación unaria.", node);
            return null;
        }

        switch ((string)node.Value)
        {
            case "++":
                if (((UnaryNode)node).Child.NodeType == NodeType.Identifier && operand is int)
                {
                    int newValue = (int)operand + 1;
                    UpdateVariable((string)((UnaryNode)node).Child.Value, newValue, node);
                    return newValue;
                }
                else
                {
                    ReportError("El operador '++' solo se puede aplicar a variables de tipo entero.", node);
                    return null;
                }
            case "--":
                if (((UnaryNode)node).Child.NodeType == NodeType.Identifier && operand is int)
                {
                    int newValue = (int)operand - 1;
                    UpdateVariable((string)((UnaryNode)node).Child.Value, newValue, node);
                    return newValue;
                }
                else
                {
                    ReportError("El operador '--' solo se puede aplicar a variables de tipo entero.", node);
                    return null;
                }
            case "!":
                if (operand is bool)
                    return !(bool)operand;
                else
                {
                    ReportError("El operador '!' solo se puede aplicar a valores booleanos.", node);
                    return null;
                }
            case "-":
                if (operand is int)
                    return -(int)operand;
                else
                {
                    ReportError("El operador '-' solo se puede aplicar a valores enteros.", node);
                    return null;
                }
            default:
                ReportError("Operador unario desconocido.", node);
                return null;
        }
    }

    private object EvaluateLiteral(ASTNode node)
    {
        return node.Value;
    }

    private object? EvaluateIdentifier(ASTNode node)
    {
        return LookupVariable((string)node.Value, node);
    }

    private object? EvaluatePropertyAccess(ASTNode node)
    {
        var left = GetVal(Left(node));
        string right = (string)Right(node).Value;

        if (left is CardData)
        {
            switch (right)
            {
                case "Owner":
                    return ((CardData)left).player;
                case "Type":
                    return ((CardData)left).type;
                case "Name":
                    return ((CardData)left).cardName;
                case "Faction":
                    return ((CardData)left).faction;
                case "Power":
                    return ((CardData)left).attackPower;
                case "Range":
                    return ((CardData)left).range;
                default:
                    ReportError("Operación de acceso a propiedad inválida", node);
                    return null;
            }
        }
        else if (left is Context)
        {
            left = (Context)context;
            var x = Context.TriggerPlayer;
            switch (right)
            {
                case "TriggerPlayer":
                    return Context.TriggerPlayer;
                case "Board":
                    return Context.Board;
                case "Hand":
                    return x == 1 ? Context.HandOfPlayer1 : Context.HandOfPlayer2;
                case "Deck":
                    return x == 1 ? Context.DeckOfPlayer1 : Context.DeckOfPlayer2;
                case "Field":
                    return x == 1 ? Context.FieldOfPlayer1 : Context.FieldOfPlayer2;
                case "Graveyard":
                    return x == 1 ? Context.GraveyardOfPlayer1 : Context.GraveyardOfPlayer2;
                default:
                    ReportError("Operación de acceso a propiedad inválida", node);
                    return null;
            }
        }
        else
        {
            ReportError("Operación de acceso a propiedad inválida", node);
            return null;
        }
    }

    private object? EvaluateFunctionCall(ASTNode node)
    {
        var left = GetVal(Left(node));
        string name = (string)Left(Right(node)).Value;
        if (Right(Right(node)) == null)
        {
            if (name != "Pop" || left is not List<CardData>)
            {
                ReportError("Operación de acceso a función inválida", node);
                return null;
            }
            
            var result = ((List<CardData>)left)[0];
            ((List<CardData>)left).RemoveAt(0);
            return result;
        }
        var param = Right(Right(node)).NodeType != NodeType.Predicate ? GetVal(Right(Right(node))) : Right(Right(node));

        if (left is Context)
        {
            if (param is not int)
            {
                ReportError("Operación de acceso a función inválida", node);
                return null;
            }

            switch (name)
            {
                case "HandOfPlayer":
                    return (int)param == 1 ? Context.HandOfPlayer1 : Context.HandOfPlayer2;
                case "FieldOfPlayer":
                    return (int)param == 1 ? Context.FieldOfPlayer1 : Context.FieldOfPlayer2;
                case "GraveyardOfPlayer":
                    return (int)param == 1 ? Context.GraveyardOfPlayer1 : Context.GraveyardOfPlayer2;
                case "DeckOfPlayer":
                    return (int)param == 1 ? Context.DeckOfPlayer1 : Context.DeckOfPlayer2;
                default:
                    ReportError("Operación de acceso a función inválida", node);
                    return null;
            }
        }
        else if (left is List<CardData> && name == "Find")
        {
            if (((ASTNode)param).NodeType != NodeType.Predicate)
            {
                ReportError("Operación de acceso a función inválida", node);
                return null;
            }

            return Find((List<CardData>)left, (ASTNode)param);
        }
        else
        {
            ReportError("Operación de acceso a función inválida", node);
            return null;
        }
    }

    private int? EvaluateArithmeticOperation(string op, int left, int right)
    {
        switch (op)
        {
            case "+": return left + right;
            case "-": return left - right;
            case "*": return left * right;
            case "/":
                if (right == 0)
                {
                    ReportError("División por cero.", null);
                    return null;
                }
                return left / right;
            default: return null;
        }
    }

    private bool EvaluateComparisonOperation(string op, object left, object right)
    {
        switch (op)
        {
            case "==": return left == right;
            case "!=": return left != right;
            case "<": return (int)left < (int)right;
            case ">": return (int)left > (int)right;
            case "<=": return (int)left <= (int)right;
            case ">=": return (int)left >= (int)right;
            default: return false;
        }
    }

    private bool EvaluateLogicalOperation(string op, bool left, bool right)
    {
        switch (op)
        {
            case "&&": return left && right;
            case "||": return left || right;
            default: return false;
        }
    }

    private List<CardData> Find(List<CardData> source, ASTNode predicate)
    {
        List<CardData> result = new List<CardData>();
        string varName = (string)Left(predicate).Value;
        foreach (var item in source)
        {
            EnterScope();
            AssignNewVariable(varName, item, predicate);
            var predResult = GetVal(Right(predicate));
            if (predResult is not bool)
            {
                ReportError("La expresión del predicado debe devolver un booleano", predicate);
                return result;
            }
            if ((bool)predResult)
            {
                result.Add(item);
            }
            ExitScope();
        }
        return result;
    }


}
