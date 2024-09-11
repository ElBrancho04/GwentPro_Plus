class Evaluator
{
    private readonly Stack<Dictionary<string, object>> scopes;
    public List<Error> Errors { get; private set; }
    private readonly Context context;
    private readonly CreatedCard createdCard;

    public Evaluator(Context context, CreatedCard createdCard)
    {
        this.context = context;
        this.createdCard = createdCard;
        Errors = new List<Error>();
        scopes = new Stack<Dictionary<string, object>>();
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
    private void AssignVariable(ASTNode node)
    {
        if (node is UnaryNode)
        {
            var name = ((UnaryNode)node).Child.Value;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name));
                {
                    if (scope[name] is not int)
                    {
                        ReportError("Los incrementos y decrementos solo se pueden usar en variables int ya asignadas");
                        return;
                    }
                    else
                    
                }
            }

        }

        var name = (string)node.left.Value;
        var value = EvaluateExpression(node.right);
        if (value == null)
        {
            ReportError("Expresión no válida en la asignación.", node);
            return;
        }

        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name))
            {
                if (!((scope[name] is string && value is string) || (scope[name] is int && value is int) || (scope[name] is bool && value is bool)))
                {
                    ReportError("Tratas de asignar un valor que no es el de la variable", node);
                    return;
                }
                else
                    switch (type)
                    {
                        case "=":
                            scope[name] = value;
                            return;
                        case "-=":
                            if (scope[name] is not int)
                            {
                                ReportError("Este tipo de asignación solo se usa en enteros", node);
                                return;
                            }
                            else
                            {
                                scope[name] -= value;
                                return;
                            }
                        default:
                            if (scope[name] is not int)
                            {
                                ReportError("Este tipo de asignación solo se usa en enteros", node);
                                return;
                            }
                            else
                            {
                                scope[name] += value;
                                return;
                            }
                    }
            }
        }
        if (type == "=")
        {
            scope[name] = value;
        }
        else
        ReportError("Ese tipo de asignación solo se puede hacer en variables tipo entero ya asignadas");
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
    private object? EvaluateIdentifier(IdentifierNode node)
    {
        return LookupVariable(node.Name, node);
    }

    private void EvaluateBlock(MultiChildNode block)
    {
        foreach (var stmn in block.Children)
        {
            EvaluateStatement(stmn);
        }
    }

    private void EvaluateStatement(ASTNode node)
    {
        switch ((string)node.Value)
        {
            case "if":
                {
                    var condition = EvaluateExpression(node.Children[0]);
                    if (condition is bool result)
                    {
                        if (result)
                        {
                            EnterScope();
                            EvaluateStatement(node.Children[1]);
                            ExitScope();
                        }
                        else if (node.Children.Count == 3)
                        {
                            EnterScope();
                            EvaluateStatement(node.Children[2]);
                            ExitScope();
                        }
                    }
                    else
                    {
                        ReportError("La condición del 'if' no es booleana.", node);
                    }
                    break;
                }
            case NodeType.WhileStatement:
                {
                    var condition = EvaluateExpression(node.Children[0]);
                    if (condition is bool)
                    {
                        EnterScope();
                        while (EvaluateExpression(node.Children[0]) is bool result && result)
                        {
                            EvaluateStatement(node.Children[1]);
                        }
                        ExitScope();
                    }
                    else
                    {
                        ReportError("La condición del 'while' no es booleana.", node);
                    }
                    break;
                }
            case NodeType.Assignment:
                {
                    AssignVariable(node);
                    break;
                }
            case NodeType.ForStatement:
                {
                    var variable = node.Children[0].Value.ToString();
                    var collection = EvaluateExpression(node.Children[1]);
                    if (collection is IEnumerable<object> enumerable)
                    {
                        EnterScope();
                        foreach (var item in enumerable)
                        {
                            AssignVariable(variable, item);
                            EvaluateStatement(node.Children[2]);
                        }
                        ExitScope();
                    }
                    else
                    {
                        ReportError("La colección del 'for' no es válida.", node);
                    }
                    break;
                }
            default:
                ReportError("Tipo de statement no soportado.", node);
                break;
        }
    }
}
