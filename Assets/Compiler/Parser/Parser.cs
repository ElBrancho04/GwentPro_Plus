#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class Parser
{
    private List<Token> tokens;
    public List<Error> Errors {get;}
    private int pos;
    private Token currentToken;
    private string currTokenVal()
    {
        return (currentToken.Value is string) ? (string)currentToken.Value : "";
    }

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        pos = 0;
        currentToken = tokens.Count > 0? tokens[pos]: new Token(TokenType.EOF, "#", 0, 0);
        Errors = new List<Error>();
    }

    private void Advance()
    {
        pos++;
        if (pos < tokens.Count)
        {
            currentToken = tokens[pos];
        }
        else
        {
            currentToken = tokens[tokens.Count -1];
        }
    }

    private Token Peek(int tokensAhead = 1)
    {
        if (pos + tokensAhead >= tokens.Count || pos + tokensAhead < 0)
        {
            return tokens[tokens.Count - 1];
        }
        return tokens[pos + tokensAhead];
    }

    private void ReportError(string message)
    {
        Errors.Add(new Error(currentToken.Line, currentToken.Pos, message));
    }

    public ASTNode Parse()
    {
        return ParseProgram();
    }

    private ASTNode ParseProgram()
    {
        var programNode = new MultiChildNode(NodeType.Program, "Program", 0, 0);

        while (currentToken.TokenType != TokenType.EOF)
        {
            if (currTokenVal() == "card")
            {
                var cardNode = ParseCardDecl();
                programNode.AddChild(cardNode);
            }
            else if (currTokenVal() == "effect")
            {
                var effectNode = ParseEffectDecl();
                programNode.AddChild(effectNode);

            }
            else
            {
                ReportError($"Token inesperado: {currentToken.Value}, sólo se permite declarar card y effect en este contexto");
                while (currTokenVal() != "card" && currTokenVal() != "effect" && currentToken.TokenType != TokenType.EOF)
                    Advance();

                continue;
            }
        }
        return programNode;
    }

    private ASTNode ParseEffectDecl()
    {
        var effectNode = new MultiChildNode(NodeType.EfectDecl, "effect", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != "{")
        {
            ReportError("Token inesperado, se esperaba un {");
            while (currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return effectNode;
                Advance();
            }
            return effectNode;
        }
        Advance();
        while (currentToken.TokenType != TokenType.EOF && currTokenVal() != "}")
        {
            if (currentToken.TokenType == TokenType.EOF) return effectNode;
            if (currTokenVal() == "card" || currTokenVal() == "effect")
            {
                ReportError("Declaración de efecto incompleta, se esperaba: }");
                return effectNode;
            }
            if (currTokenVal() == "Name")
            {
                var NameNode = ParseEfctDeclName();
                effectNode.AddChild(NameNode);
            }
            if (currTokenVal() == "Params")
            {
                var ParamsNode = ParseEfctDeclParams();
                effectNode.AddChild(ParamsNode);
            }
            if (currTokenVal() == "Action")
            {
                var ActionNode = ParseEfctDeclAction();
                effectNode.AddChild(ActionNode);
            }
        }
        if (currTokenVal() == "}") Advance();
        else ReportError("Debe terminar la declaración de effect con }");
        return effectNode;
    }

    private ASTNode ParseEfctDeclName()
    {
        var nameNode = new UnaryNode(NodeType.Property, "Name", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return nameNode;
                Advance();
            }
            return nameNode;
        }
        Advance();
        nameNode.Child = ParseExpression();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de effect");
                return nameNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return nameNode;
            }
        }
        else return nameNode;
    }
    private ASTNode ParseEfctDeclParams()
    {
        var paramsNode = new MultiChildNode(NodeType.Property, "Params", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return paramsNode;
                Advance();
            }
            return paramsNode;
        }
        Advance();
        if (currTokenVal() != "{")
        {
            ReportError("Token inesperado, se esperaba '{'");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return paramsNode;
                Advance();
            }
            return paramsNode;
        }
        Advance();
        while (currentToken.TokenType == TokenType.Identifier)
        {
            if (currentToken.TokenType == TokenType.EOF) return paramsNode;
            var paramIDNode = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
            var paramNode = new BinaryNode(NodeType.Property, "Param", currentToken.Line, currentToken.Pos, paramIDNode);
            Advance();
            if (currTokenVal() != ":")
            {
                ReportError("Token inesperado, se esperaba ':'");
                while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
                {
                    if (currentToken.TokenType == TokenType.EOF) return paramsNode;
                    if (currTokenVal() == ",")
                    {
                        if (Peek().Value is string && (string)Peek().Value == "}")
                        {
                            ReportError("La coma es innecesaria");
                            Advance();
                            Advance();
                            return paramsNode;
                        }
                        Advance();
                        break;
                    }
                    Advance();
                }
                return paramsNode;
            }
            Advance();
            if (currTokenVal() != "Number" && currTokenVal() != "String" && currTokenVal() != "Bool")
            {
                ReportError("Token inesperado, sólo se aceptan: Number, String o Bool");
                while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
                {
                    if (currentToken.TokenType == TokenType.EOF) return paramsNode;
                    if (currTokenVal() == ",")
                    {
                        if (Peek().Value is string && (string)Peek().Value == "}")
                        {
                            ReportError("La coma es innecesaria");
                            Advance();
                            Advance();
                            return paramsNode;
                        }
                        Advance();
                        break;
                    }
                    Advance();
                }
                return paramsNode;
            }
            paramNode.Right = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
            paramsNode.AddChild(paramNode);
            Advance();
            if (currTokenVal() == ",")
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                    Advance();
                    Advance();
                    return paramsNode;
                }
                Advance();
            }
        }
        if (currTokenVal() != "}")
        {
            ReportError("Token inesperado, se debe concluir la declaración con un '}'");
        }
        else Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de effect");
                return paramsNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return paramsNode;
            }
        }
        else return paramsNode;
    }
    private ASTNode ParseEfctDeclAction()
    {
        var actionNode = new BinaryNode(NodeType.Property, "Action", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                Advance();
            }
            return actionNode;
        }
        Advance();
        if (currTokenVal() != "(")
        {
            ReportError("Token inesperado, se esperaba '('");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                if (currTokenVal() == "{")
                {
                    Advance();
                    actionNode.Right = ParseBlock();
                    if (currTokenVal() != "}")
                        ReportError("Token inesperado, debe terminar la declaración con '}'");
                    else
                        Advance();
                    return actionNode;
                }
                Advance();
            }
            return actionNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Token inesperado, se esperaba un identificador");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                if (currTokenVal() == "{")
                {
                    Advance();
                    actionNode.Right = ParseBlock();
                    if (currTokenVal() != "}")
                        ReportError("Token inesperado, debe terminar la declaración con '}'");
                    else
                        Advance();
                    return actionNode;
                }
                Advance();
            }
            return actionNode;
        }
        var paramsNode = new BinaryNode(NodeType.EfctDeclActionParms, "Params", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
        actionNode.Left = paramsNode;
        Advance();
        if (currTokenVal() != ",")
        {
            ReportError("Token inesperado, se esperaba ','");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                if (currTokenVal() == "{")
                {
                    Advance();
                    actionNode.Right = ParseBlock();
                    if (currTokenVal() != "}")
                        ReportError("Token inesperado, debe terminar la declaración con '}'");
                    else
                        Advance();
                    return actionNode;
                }
                Advance();
            }
            return actionNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Token inesperado, se esperaba un identificador");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                if (currTokenVal() == "{")
                {
                    Advance();
                    actionNode.Right = ParseBlock();
                    if (currTokenVal() != "}")
                        ReportError("Token inesperado, debe terminar la declaración con '}'");
                    else
                        Advance();
                    return actionNode;
                }
                Advance();
            }
            return actionNode;
        }
        paramsNode.Right = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() != ")")
        {
            ReportError("Token inesperado, se esperaba ')'");
            while (currTokenVal() != "Name" && currTokenVal() != "Params" && currTokenVal() != "Action" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return actionNode;
                if (currTokenVal() == "{")
                {
                    Advance();
                    actionNode.Right = ParseBlock();
                    if (currTokenVal() != "}")
                        ReportError("Token inesperado, debe terminar la declaración con '}'");
                    else
                        Advance();
                    return actionNode;
                }
                Advance();
            }
            return actionNode;
        }
        Advance();
        if (currTokenVal() != "=>")
        {
            ReportError("Token inesperado, se esperaba '=>'");
        }
        else Advance();
        if (currTokenVal() != "{")
        {
            ReportError("Token inesperado, se esperaba '{'");
        }
        else Advance();
        actionNode.Right = ParseBlock();
        if (currTokenVal() != "}")
        {
            ReportError("Token inesperado, se debe concluir la declaración con un '}'");
        }
        else Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de effect");
                return actionNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return actionNode;
            }
        }
        else return actionNode;
    }

    private ASTNode ParseBlock()
    {
        var blockNode = new MultiChildNode(NodeType.Block, "Block", currentToken.Line, currentToken.Pos);

        while (currTokenVal() != "}")
        {
            if (currentToken.TokenType == TokenType.EOF) return blockNode;
            var statement = ParseStatement();
            if (statement != null)
            {
                blockNode.AddChild(statement);
                if (currTokenVal() != ";") ReportError("';' faltante, todas las declaraciones deben terminar con ';'");
                else Advance();
            }
            else
            {
                Advance();
            }
        }

        return blockNode;
    }

    private ASTNode? ParseStatement()
    {
        if (currTokenVal() == "if")
        {
            return ParseIfStatement();
        }
        else if (currTokenVal() == "while")
        {
            return ParseWhileStatement();
        }
        else if (currTokenVal() == "for")
        {
            return ParseForStatement();
        }
        else if (currentToken.TokenType == TokenType.Identifier && Peek().Value is string && (string)Peek().Value == ".")
        {
            var functionCall = ParseFunctStatement();
            if (currTokenVal() == "==" || currTokenVal() == "+=" || currTokenVal() == "-=")
            {
                var assigmentType = currentToken;
                Advance();
                return new BinaryNode(NodeType.Assigment, assigmentType.Value, assigmentType.Line, assigmentType.Pos, functionCall, ParseExpression());
            }
            else return functionCall;
        }
        else if (currentToken.TokenType == TokenType.Identifier)
        {
            return ParseAssignment();
        }
        else
        {
            ReportError("Token inesperado, no hay ningún tipo de declaración que comience con ese token");
            return null;
        }
    }

    private ASTNode? ParseIfStatement()
    {
        int line = currentToken.Line;
        int pos = currentToken.Pos;
        Advance();

        if (currTokenVal() != "(")
        {
            ReportError("Se esperaba '(' después de 'if'");
            return null;
        }
        Advance();

        var condition = ParseExpression();

        if (currTokenVal() != ")")
        {
            ReportError("Se esperaba ')' después de la condición 'if'");
            return null;
        }
        Advance();

        if (currTokenVal() != "{")
        {
            var statement = ParseStatement();
            if (statement == null)
                ReportError("Se esperaba '{' antes del bloque de código del 'if'");
            return new BinaryNode(NodeType.Statement, "while", line, pos, condition, statement);
        }
        Advance();

        var body = ParseBlock();

        if (currTokenVal() != "}")
        {
            ReportError("Se esperaba '}' después del bloque de código del 'if'");
            return null;
        }
        Advance();

        var ifNode = new BinaryNode(NodeType.Statement, "if", line, pos, condition, body);

        return ifNode;
    }

    private ASTNode? ParseWhileStatement()
    {
        int line = currentToken.Line;
        int pos = currentToken.Pos;
        Advance();

        if (currTokenVal() != "(")
        {
            ReportError("Se esperaba '(' después de 'while'");
            return null;
        }
        Advance();

        var condition = ParseExpression();

        if (currTokenVal() != ")")
        {
            ReportError("Se esperaba ')' después de la condición 'while'");
            return null;
        }
        Advance();

        if (currTokenVal() != "{")
        {
            var statement = ParseStatement();
            if (statement == null)
                ReportError("Se esperaba '{' antes del bloque de código del 'while'");
            return new BinaryNode(NodeType.Statement, "while", line, pos, condition, statement);
        }
        Advance();

        var body = ParseBlock();

        if (currTokenVal() != "}")
        {
            ReportError("Se esperaba '}' después del bloque de código del 'while'");
            return null;
        }
        Advance();

        var whileNode = new BinaryNode(NodeType.Statement, "while", line, pos, condition, body);

        return whileNode;
    }

    private ASTNode? ParseForStatement()
    {
        int line = currentToken.Line;
        int pos = currentToken.Pos;
        Advance();

        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Se esperaba un identificador después de 'for('");
            return null;
        }
        var variable = currentToken;
        Advance();

        if (currTokenVal() != "in")
        {
            ReportError("Se esperaba 'in' después del identificador en 'for'");
            return null;
        }
        Advance();

        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Se esperaba un identificador después de 'in'");
            return null;
        }
        var collection = currentToken;
        Advance();

        if (currTokenVal() != "{")
        {
            var statement = ParseStatement();
            if (statement == null)
                ReportError("Se esperaba '{' antes del bloque de código del 'for'");
            var forNod = new MultiChildNode(NodeType.Statement, "for", line, pos);
            forNod.AddChild(new ASTNode(NodeType.Identifier, variable.Line, variable.Pos, variable.Value));
            forNod.AddChild(new ASTNode(NodeType.Identifier, collection.Line, collection.Pos, collection.Value));
            forNod.AddChild(statement);

            return forNod;
        }
        Advance();

        var body = ParseBlock();

        if (currTokenVal() != "}")
        {
            ReportError("Se esperaba '}' después del bloque de código del 'for'");
            return null;
        }
        Advance();

        var forNode = new MultiChildNode(NodeType.Statement, "for", line, pos);
        forNode.AddChild(new ASTNode(NodeType.Identifier, variable.Line, variable.Pos, variable.Value));
        forNode.AddChild(new ASTNode(NodeType.Identifier, collection.Line, collection.Pos, collection.Value));
        forNode.AddChild(body);

        return forNode;
    }

    private ASTNode? ParseAssignment()
    {
        int line = currentToken.Line;
        int pos = currentToken.Pos;

        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Se esperaba un identificador en el lado izquierdo de la asignación");
            return null;
        }
        var left = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() == "++" || currTokenVal() == "--")
        {
            return new UnaryNode(NodeType.Assigment, currTokenVal(), line, pos, left);
        }
        if (currTokenVal() != "=" && currTokenVal() != "+=" && currTokenVal() != "-=")
        {
            ReportError("Se esperaba '=' o '+=' o '-=' en la asignación");
            return null;
        }
        string assigmentType = currTokenVal();
        Advance();

        var right = ParseExpression();

        var assignmentNode = new BinaryNode(NodeType.Assigment, assigmentType, line, pos, left, right);
        return assignmentNode;
    }
    private ASTNode ParseFunctStatement()
    {
        var functionCall = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        while (currTokenVal() == ".")
        {
            if (currentToken.TokenType == TokenType.EOF) return functionCall;
            Advance();
            if (currentToken.TokenType != TokenType.Identifier)
            {
                ReportError("Token inesperado, se esperaba un identificador");
                return functionCall;
            }
            if (Peek().Value is string && (string)Peek().Value == "(")
            {
                var function = new BinaryNode(NodeType.Function, "", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
                functionCall = new BinaryNode(NodeType.FunctionCall, "", currentToken.Line, currentToken.Pos, functionCall, function);
                Advance();
                Advance();
                if (currentToken.TokenType == TokenType.Identifier)
                {
                    function.Right = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
                    Advance();
                }
                else if (currentToken.TokenType == TokenType.Number)
                {
                    function.Right = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
                    Advance();
                }
                if (currTokenVal() != ")")
                {
                    ReportError("Token inesperado, se esperaba ')'");
                    return functionCall;
                }
                Advance();
                return functionCall;
            }
            functionCall = new BinaryNode(NodeType.PropertyAcces, "", currentToken.Line, currentToken.Pos, functionCall, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
            Advance();
        }
        if (currTokenVal() != "-=" && currTokenVal() != "=" && currTokenVal() != "+=" && currTokenVal() != "--" && currTokenVal() != "++")
            ReportError("Token inesperado, se esperaba un '.' o '('");
        else
        {
            var assigmentType = currTokenVal();
            Advance();
            ASTNode assignmentNode = assigmentType == "++" || assigmentType == "--"? new UnaryNode(NodeType.Assigment, assigmentType, currentToken.Line, currentToken.Pos, functionCall): new BinaryNode(NodeType.Assigment, assigmentType, currentToken.Line, currentToken.Pos, functionCall, ParseExpression());
            return assignmentNode;
        }
        return functionCall;
    }

    private ASTNode? ParseExpression()
    {
        var left = ParseLogicalOr();
        return left;
    }

    private ASTNode? ParseLogicalOr()
    {
        var left = ParseLogicalAnd();
        if (left == null) return null;

        while (currTokenVal() == "||")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseLogicalAnd();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '||'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseLogicalAnd()
    {
        var left = ParseEquality();
        if (left == null) return null;

        while (currTokenVal() == "&&")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseEquality();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '&&'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseEquality()
    {
        var left = ParseComparison();
        if (left == null) return null;

        while (currTokenVal() == "==" || currTokenVal() == "!=")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseComparison();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseComparison()
    {
        var left = ParseConcatenation();
        if (left == null) return null;

        while (currTokenVal() == "<" || currTokenVal() == ">" ||

               currTokenVal() == "<=" || currTokenVal() == ">=")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseConcatenation();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseConcatenation()
    {
        var left = ParseTerm();
        if (left == null) return null;

        while (currTokenVal() == "@" || currTokenVal() == "@@")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseTerm();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseTerm()
    {
        var left = ParseFactor();
        if (left == null) return null;

        while (currTokenVal() == "+" || currTokenVal() == "-")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseFactor();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseFactor()
    {
        var left = ParseUnary();
        if (left == null) return null;

        while (currTokenVal() == "*" || currTokenVal() == "/")
        {
            if (currentToken.TokenType == TokenType.EOF) return left;
            var operatorToken = currentToken;
            Advance();
            var right = ParseUnary();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            left = new BinaryNode(NodeType.BinaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left, right);
        }

        return left;
    }

    private ASTNode? ParseUnary()
    {
        if (currTokenVal() == "-" || currTokenVal() == "++" || currTokenVal() == "--")
        {
            var operatorToken = currentToken;
            Advance();
            var right = ParsePrimary();
            if (right == null)
            {
                ReportError($"Se esperaba una expresión después de '{operatorToken.Value}'");
                return null;
            }

            return new UnaryNode(NodeType.UnaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, right);
        }
        else if (Peek().Value is string && ((string)Peek().Value == "-" || (string)Peek().Value == "++" || (string)Peek().Value == "--"))
        {
            var operatorToken = Peek();
            var left = ParsePrimary();
            if (left == null)
            {
                ReportError($"Se esperaba una expresión antes de '{operatorToken.Value}'");
                return null;
            }
            Advance();
            return new UnaryNode(NodeType.UnaryOp, operatorToken.Value, operatorToken.Line, operatorToken.Pos, left);
        }

        return ParsePrimary();
    }

    private ASTNode? ParsePrimary()
    {

        if (currentToken.TokenType == TokenType.Number || currentToken.TokenType == TokenType.String)
        {
            var literal = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
            Advance();
            return literal;
        }
        else if (currentToken.TokenType == TokenType.Identifier)
        {
            var identifierNode = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
            Advance();
            while (currTokenVal() == "." || currTokenVal() == "[")
            {
                if (currentToken.TokenType == TokenType.EOF) return identifierNode;
                Advance();
                if ((string)Peek(-1).Value == "[")
                {
                    if (currentToken.TokenType != TokenType.Number)
                    {
                        ReportError("Token inesperado, se esperaba un literal de número");
                        return identifierNode;
                    }
                    identifierNode = new BinaryNode(NodeType.IndexAccess, "[]", currentToken.Line, currentToken.Pos, identifierNode, new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value));
                    Advance();
                    if (currTokenVal() != "]")
                    {
                        ReportError("Token inesperado, se esperaba: ']'");
                        return identifierNode;
                    }
                    Advance();
                    continue;
                }
                if (currentToken.TokenType != TokenType.Identifier)
                {
                    ReportError("Token inesperado, se esperaba un identificador");
                    return identifierNode;
                }
                if (Peek().Value is string && (string)Peek().Value != "(")
                {
                    identifierNode = new BinaryNode(NodeType.PropertyAcces, ".", currentToken.Line, currentToken.Pos, identifierNode, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
                    Advance();
                }
                else
                {
                    if (currTokenVal() == "Find")
                    {
                        var function = new BinaryNode(NodeType.Function, "", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
                        identifierNode = new BinaryNode(NodeType.FunctionCall, "", currentToken.Line, currentToken.Pos, identifierNode, function);
                        Advance();
                        if (currTokenVal() != "(")
                        {
                            ReportError("Token inesperado, se esperaba '('");
                            return identifierNode;
                        }
                        Advance();
                        function.Right = ParsePredicate();
                        if (currTokenVal() != ")")
                        {
                            ReportError("Token inesperado, se esperaba ')'");
                            return identifierNode;
                        }
                        Advance();
                        return identifierNode;
                    }
                    var funtionName = new BinaryNode(NodeType.Function, "", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
                    Advance();
                    Advance();
                    if (currentToken.TokenType == TokenType.Identifier)
                    {
                        funtionName.Right = new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value);
                        Advance();
                    }
                    else if (currentToken.TokenType == TokenType.Number)
                    {
                        funtionName.Right = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
                        Advance();
                    }
                    identifierNode = new BinaryNode(NodeType.FunctionCall, "", currentToken.Line, currentToken.Pos, identifierNode, funtionName);
                    if (currTokenVal() != ")")
                    {
                        ReportError("Token inesperado, se esperaba ')'");
                        return identifierNode;
                    }
                    Advance();
                    return identifierNode;
                }
            }
            return identifierNode;
        }
        else if (currTokenVal() == "(")
        {
            Advance();
            var expr = ParseExpression();
            if (currTokenVal() != ")")
            {
                ReportError("Se esperaba ')'");
                return null;
            }
            Advance();
            return expr;
        }
        else
        {
            ReportError($"Token inesperado: {currentToken.Value}");
            return null;
        }
    }

    private ASTNode? ParsePredicate()
    {
        if (currTokenVal() != "(")
        {
            ReportError("Token inesperado, se esperaba '('");
            return null;
        }
        Advance();
        if (currentToken.TokenType != TokenType.Identifier)
        {
            ReportError("Token inesperado, se esperaba un identificador");
            return null;
        }
        var predicate = new BinaryNode(NodeType.Predicate, "", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Identifier, currentToken.Line, currentToken.Pos, currentToken.Value));
        Advance();
        if (currTokenVal() != ")")
        {
            ReportError("Token inesperado, se esperaba ')'");
            return null;
        }
        Advance();
        if (currTokenVal() != "=>")
        {
            ReportError("Token inesperado, se esperaba '=>'");
            return null;
        }
        Advance();
        predicate.Right = ParseExpression();
        return predicate;
    }

    private ASTNode ParseCardDecl()
    {
        var cardNode = new MultiChildNode(NodeType.CardDecl, "card", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != "{")
        {
            ReportError("Token inesperado, se esperaba '{'");
        }
        else Advance();
        while (currTokenVal() != "}")
        {
            if (currentToken.TokenType == TokenType.EOF)
            {
                return cardNode;
            }
            if (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation")
            {
                ReportError("Token inesperado en la declaración de la carta, solo se permite declarar: 'Type', 'Name', 'Faction', 'Power', 'Range', 'OnActivation'");
                return cardNode;
            }
            if (currTokenVal() == "Type")
            {
                var typeNode = ParseCardDeclType();
                cardNode.AddChild(typeNode);
            }
            if (currTokenVal() == "Name")
            {
                var nameNode = ParseCardDeclName();
                cardNode.AddChild(nameNode);
            }
            if (currTokenVal() == "Faction")
            {
                var factionNode = ParseCardDeclFaction();
                cardNode.AddChild(factionNode);
            }
            if (currTokenVal() == "Power")
            {
                var powerNode = ParseCardDeclPower();
                cardNode.AddChild(powerNode);
            }
            if (currTokenVal() == "Range")
            {
                var rangeNode = ParseCardDeclRange();
                cardNode.AddChild(rangeNode);
            }
            if (currTokenVal() == "OnActivation")
            {
                var onActNode = ParseCardDeclOnAct();
                cardNode.AddChild(onActNode);
            }
        }
        if (currTokenVal() == "}") Advance();
        return cardNode;
    }
    private ASTNode ParseCardDeclType()
    {
        var typeNode = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return typeNode;
                Advance();
            }
            return typeNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.String)
        {
            ReportError("Token inesperado, se esperaba un token de tipo string");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return typeNode;
                Advance();
            }
            return typeNode;
        }
        typeNode.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return typeNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return typeNode;
            }
        }
        else return typeNode;
    }
    private ASTNode ParseCardDeclName()
    {
        var nameNode = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return nameNode;
                Advance();
            }
            return nameNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.String)
        {
            ReportError("Token inesperado, se esperaba un token de tipo string");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return nameNode;
                Advance();
            }
            return nameNode;
        }
        nameNode.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return nameNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return nameNode;
            }
        }
        else return nameNode;
    }
    private ASTNode ParseCardDeclFaction()
    {
        var factionNode = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return factionNode;
                Advance();
            }
            return factionNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.String)
        {
            ReportError("Token inesperado, se esperaba un token de tipo string");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return factionNode;
                Advance();
            }
            return factionNode;
        }
        factionNode.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return factionNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return factionNode;
            }
        }
        else return factionNode;
    }
    private ASTNode ParseCardDeclPower()
    {
        var powerNode = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return powerNode;
                Advance();
            }
            return powerNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.Number)
        {
            ReportError("Token inesperado, se esperaba un token de tipo Number");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return powerNode;
                Advance();
            }
            return powerNode;
        }
        powerNode.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return powerNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return powerNode;
            }
        }
        else return powerNode;
    }
    private ASTNode ParseCardDeclRange()
    {
        var rangeNode = new MultiChildNode(NodeType.Property, "Range", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return rangeNode;
                Advance();
            }
            return rangeNode;
        }
        Advance();
        if (currTokenVal() != "[")
        {
            ReportError("Token inesperado, se esperaba '['");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return rangeNode;
                Advance();
            }
            return rangeNode;
        }
        Advance();
        if (currentToken.TokenType != TokenType.String)
        {
            ReportError("Token inesperado, se esperaba un token de tipo string");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return rangeNode;
                Advance();
            }
            return rangeNode;
        }
        while (currTokenVal() != "]")
        {
            if (currentToken.TokenType == TokenType.EOF) return rangeNode;
            if (currentToken.TokenType != TokenType.String)
            {
                ReportError("Token inesperado, se debe terminar el array con ']'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
                {
                    if (currentToken.TokenType == TokenType.EOF) return rangeNode;
                    Advance();
                }
                return rangeNode;
            }
            else
            {
                if (currTokenVal() != "Melee" && currTokenVal() != "Ranged" && currTokenVal() != "Siege")
                {
                    ReportError("Token inesperado, solo se permite: 'Melee', 'Ranged' o 'Siege'");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return rangeNode;
                        Advance();
                    }
                    return rangeNode;
                }
                rangeNode.AddChild(new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value));
                Advance();
                if (currTokenVal() == ",")
                {
                    Advance();
                    if (currTokenVal() == "]")
                        ReportError("Coma innnecesaria");
                }
                else if (currentToken.TokenType == TokenType.String)
                {
                    ReportError("Se deben separar los elementos del array con coma");
                }
            }
        }
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return rangeNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return rangeNode;
            }
        }
        else return rangeNode;
    }
    private ASTNode ParseCardDeclOnAct()
    {
        var onActNode = new MultiChildNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() != ":")
        {
            ReportError("Token inesperado, se esperaba ':'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return onActNode;
                Advance();
            }
            return onActNode;
        }
        Advance();
        if (currTokenVal() != "[")
        {
            ReportError("Token inesperado, se esperaba '['");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return onActNode;
                Advance();
            }
            return onActNode;
        }
        Advance();
        if (currTokenVal() == "{")
            while (currTokenVal() != "]")
            {
                if (currentToken.TokenType == TokenType.EOF) return onActNode;
                if (currTokenVal() != "{")
                {
                    ReportError("Token inesperado, se esperaba '{'");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return onActNode;
                        Advance();
                    }
                    return onActNode;
                }
                var effect = ParseCardDeclOnActEfct();
                onActNode.AddChild(effect);
            }
        else
        {
            ReportError("Token inesperado, se esperaba '{'");
            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
            {
                if (currentToken.TokenType == TokenType.EOF) return onActNode;
                Advance();
            }
            return onActNode;
        }
        Advance();
        if (currTokenVal() != "}")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de card");
                return onActNode;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "}")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return onActNode;
            }
        }
        else return onActNode;
    }
    private MultiChildNode ParseCardDeclOnActEfct()
    {
        var effect = new MultiChildNode(NodeType.OnActivationEfct, "effect", currentToken.Line, currentToken.Pos);
        Advance();
        if (currTokenVal() == "Effect" && Peek().Value is string && (string)Peek().Value == ":" && Peek(2).TokenType == TokenType.String)
        {
            Advance();
            Advance();
            effect.AddChild(new UnaryNode(NodeType.Property, "Name", currentToken.Line, currentToken.Pos, new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value)));
            Advance();
            if (currTokenVal() != "}")
                ReportError("Se esperaba '}' al final de la declaración del efecto");
            else
                Advance();

            if (currTokenVal() != "]")
            {
                if (currTokenVal() != ",")
                {
                    ReportError("Token inesperado en la declaración de OnActivation");
                    return effect;
                }
                else
                {
                    if (Peek().Value is string && (string)Peek().Value == "]")
                    {
                        ReportError("La coma es innecesaria");
                    }
                    Advance();
                    return effect;
                }
            }
            else return effect;
        }
        while (currTokenVal() != "}")
        {
            if (currentToken.TokenType == TokenType.EOF) return effect;
            if (currTokenVal() == "Effect")
            {
                var Effect = ParseEffect();
                effect.AddChild(Effect);
                continue;
            }
            if (currTokenVal() == "Selector")
            {
                var Selector = ParseSelector();
                effect.AddChild(Selector);
                continue;
            }
            if (currTokenVal() == "PostAction")
            {
                var PostAction = ParsePostAction();
                effect.AddChild(PostAction);
                continue;
            }
            else
            {
                ReportError("Token inesperado, solo se pueden declarar las propiedaes del efecto: 'Effect', 'Selector' o 'PostAction'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect")
                {
                    if (currentToken.TokenType == TokenType.EOF) return effect;
                    Advance();
                }
                return effect;
            }
        }
        Advance();
        if (currTokenVal() != "]")
        {
            if (currTokenVal() != ",")
            {
                ReportError("Token inesperado en la declaración de OnActivation");
                return effect;
            }
            else
            {
                if (Peek().Value is string && (string)Peek().Value == "]")
                {
                    ReportError("La coma es innecesaria");
                }
                Advance();
                return effect;
            }
        }
        else return effect;
        MultiChildNode ParseEffect()
        {
            var Effect = new MultiChildNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
            Advance();
            if (currTokenVal() != ":")
            {
                ReportError("Token inesperado, se esperaba ':'");
                while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                {
                    if (currentToken.TokenType == TokenType.EOF) return Effect;
                    Advance();
                }
                return Effect;
            }
            Advance();
            if (currTokenVal() != "{")
            {
                ReportError("Token inesperado, se esperaba '{'");
                while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                {
                    if (currentToken.TokenType == TokenType.EOF) return Effect;
                    Advance();
                }
                return Effect;
            }
            Advance();
            while (currTokenVal() != "}")
            {
                if (currentToken.TokenType == TokenType.EOF) return Effect;
                if (currentToken.TokenType != TokenType.Identifier)
                {
                    ReportError("Token inesperado, se esperaba un token de tipo identificador");
                    while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Effect;
                        Advance();
                    }
                    return Effect;
                }
                var parameter = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
                Advance();
                if (currTokenVal() != ":")
                {
                    ReportError("Token inesperado, se esperaba ':'");
                    while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Effect;
                        Advance();
                    }
                    return Effect;
                }
                Advance();
                if (currentToken.TokenType != TokenType.Number && currentToken.TokenType != TokenType.String && currTokenVal() != "true" && currTokenVal() != "false")
                {
                    ReportError("Token inesperado, se esperaba un token de tipo literal");
                    while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Effect;
                        Advance();
                    }
                    return Effect;
                }
                if (currTokenVal() == "true" || currTokenVal() == "false")
                parameter.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, bool.Parse((string)currentToken.Value));
                else
                parameter.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
                Effect.AddChild(parameter);
                Advance();
                if (currTokenVal() != "}")
                {
                    if (currTokenVal() != ",")
                    {
                        ReportError("Token inesperado, se esperaba una ','");
                        while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                        {
                            if (currentToken.TokenType == TokenType.EOF) return Effect;
                            Advance();
                        }
                        return Effect;
                    }
                    else
                    {
                        if (Peek().Value is string && (string)Peek().Value == "}")
                        {
                            ReportError("La coma es innecesaria");
                            Advance();
                            while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                            {
                                if (currentToken.TokenType == TokenType.EOF) return Effect;
                                Advance();
                            }
                            return Effect;
                        }
                        Advance();
                    }
                }
            }
            Advance();
            if (currTokenVal() != "}")
            {
                if (currTokenVal() != ",")
                {
                    ReportError("Token inesperado, solo se permite declarar las propiedades del efecto: 'Effect', 'Selector' o 'PostAction'");
                    while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Effect;
                        Advance();
                    }
                    return Effect;
                }
                else
                {
                    if (Peek().Value is string && (string)Peek().Value == "}")
                    {
                        ReportError("La coma es innecesaria");
                    }
                    Advance();
                    while (currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "PostAction")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Effect;
                        Advance();
                    }
                    return Effect;
                }
            }
            else return Effect;
        }
        MultiChildNode ParseSelector()
        {
            var Selector = new MultiChildNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
            Advance();
            if (currTokenVal() != ":")
            {
                ReportError("Token inesperado, se esperaba ':'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                {
                    if (currentToken.TokenType == TokenType.EOF) return Selector;
                    Advance();
                }
                return Selector;
            }
            Advance();
            if (currTokenVal() != "{")
            {
                ReportError("Token inesperado, se esperaba '{'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                {
                    if (currentToken.TokenType == TokenType.EOF) return Selector;
                    Advance();
                }
                return Selector;
            }
            Advance();
            while (currTokenVal() != "}")
            {
                if (currentToken.TokenType == TokenType.EOF) return Selector;
                if (currTokenVal() != "Source" && currTokenVal() != "Single" && currTokenVal() != "Predicate")
                {
                    ReportError("Token inesperado, se esperaba 'Source', 'Single' o 'Predicate'");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Selector;
                        Advance();
                    }
                    return Selector;
                }
                var parameter = new UnaryNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
                Advance();
                if (currTokenVal() != ":")
                {
                    ReportError("Token inesperado, se esperaba ':'");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Selector;
                        Advance();
                    }
                    return Selector;
                }
                Advance();
                if (currentToken.TokenType != TokenType.Number && currentToken.TokenType != TokenType.String && currTokenVal() != "true" && currTokenVal() != "false" && (string)parameter.Value != "Predicate")
                {
                    ReportError("Token inesperado, se esperaba un token de tipo literal");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Selector;
                        Advance();
                    }
                    return Selector;
                }
                if ((string)parameter.Value == "Predicate")
                {
                    parameter.Child = ParsePredicate();
                }
                else if ((string)parameter.Value == "Source")
                {
                    if (currTokenVal() != "board" && currTokenVal() != "hand" && currTokenVal() != "otherhand" && currTokenVal() != "deck" && currTokenVal() != "otherdeck" && currTokenVal() != "field" && currTokenVal() != "otherfield" && currTokenVal() != "parent")
                    {
                        ReportError("Token inesperado, no es una Source válida");
                        while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                        {
                            if (currentToken.TokenType == TokenType.EOF) return Selector;
                            Advance();
                        }
                        return Selector;
                    }
                    parameter.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
                    Advance();
                }
                else
                {
                    if (currTokenVal() != "true" && currTokenVal() != "false")
                    {
                        ReportError("Token inesperado, se esperaba 'true' o 'false'");
                        while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                        {
                            if (currentToken.TokenType == TokenType.EOF) return Selector;
                            Advance();
                        }
                        return Selector;
                    }
                    parameter.Child = new ASTNode(NodeType.Literal, currentToken.Line, currentToken.Pos, currentToken.Value);
                    Advance();
                }
                Selector.AddChild(parameter);

                if (currTokenVal() != "}")
                {
                    if (currTokenVal() != ",")
                    {
                        ReportError("Token inesperado, se esperaba una ','");
                        while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                        {
                            if (currentToken.TokenType == TokenType.EOF) return Selector;
                            Advance();
                        }
                        return Selector;
                    }
                    else
                    {
                        if (Peek().Value is string && (string)Peek().Value == "}")
                        {
                            ReportError("La coma es innecesaria");
                            Advance();
                            while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                            {
                                if (currentToken.TokenType == TokenType.EOF) return Selector;
                                Advance();
                            }
                            return Selector;
                        }
                        Advance();
                    }
                }
            }
            Advance();
            if (currTokenVal() != "}")
            {
                if (currTokenVal() != ",")
                {
                    ReportError("Token inesperado, solo se permite declarar las propiedades del efecto: 'Effect', 'Selector' o 'PostAction'");
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Selector;
                        Advance();
                    }
                    return Selector;
                }
                else
                {
                    if (Peek().Value is string && (string)Peek().Value == "}")
                    {
                        ReportError("La coma es innecesaria");
                    }
                    Advance();
                    while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                    {
                        if (currentToken.TokenType == TokenType.EOF) return Selector;
                        Advance();
                    }
                    return Selector;
                }
            }
            else return Selector;
        }
        MultiChildNode ParsePostAction()
        {
            var postAction = new MultiChildNode(NodeType.Property, currentToken.Value, currentToken.Line, currentToken.Pos);
            Advance();
            if (currTokenVal() != ":")
            {
                ReportError("Token inesperado, se esperaba ':'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                {
                    if (currentToken.TokenType == TokenType.EOF) return postAction;
                    Advance();
                }
                return postAction;
            }
            Advance();
            if (currTokenVal() != "{")
            {
                ReportError("Token inesperado, se esperaba '{'");
                while (currTokenVal() != "Type" && currTokenVal() != "Name" && currTokenVal() != "Faction" && currTokenVal() != "Power" && currTokenVal() != "Range" && currTokenVal() != "OnActivation" && currTokenVal() != "card" && currTokenVal() != "effect" && currTokenVal() != "Effect" && currTokenVal() != "Selector" && currTokenVal() != "OnActivation")
                {
                    if (currentToken.TokenType == TokenType.EOF) return postAction;
                    Advance();
                }
                return postAction;
            }
            postAction = ParseCardDeclOnActEfct();
            return postAction;
        }
    }
}