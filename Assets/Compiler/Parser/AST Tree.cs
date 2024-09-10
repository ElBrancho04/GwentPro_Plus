#nullable enable
using System.Collections.Generic;
public class ASTNode
{
    public NodeType NodeType{get;}
    public int Line{get;}
    public int Pos{get;}
    public object Value{get;}

    public ASTNode (NodeType nodeType, int line, int pos, object value)
    {
        NodeType = nodeType;
        Line = line;
        Pos = pos;
        Value = value;
    }
}

public class UnaryNode : ASTNode
{
    public ASTNode? Child {get; set;}

    public UnaryNode (NodeType nodeType, object value, int line, int pos, ASTNode? child = null)
        :base(nodeType, line, pos, value)
    {
        Child = child;
    }
}

public class BinaryNode : ASTNode
{
    public ASTNode? Left {get; set;}
    public ASTNode? Right {get; set;}

    public BinaryNode(NodeType nodeType, object value, int line, int pos, ASTNode? left = null, ASTNode? right = null)
        : base(nodeType, line, pos, value)
    {
        Left = left;
        Right = right;
    }
}

public class MultiChildNode : ASTNode
{
    public List<ASTNode?> Children { get; }

    public MultiChildNode(NodeType nodeType, object value, int line, int pos)
        : base(nodeType, line, pos, value)
    {
        Children = new List<ASTNode?>();
    }

    public void AddChild(ASTNode? child)
    {
        Children.Add(child);
    }
}

public enum NodeType
{
    Program,
    Literal,
    Variable,
    BinaryOp,
    UnaryOp,
    Assigment,
    Statement,
    FunctionCall,
    Block,
    EfectDecl,
    CardDecl,
    Identifier,
    EfctDeclActionParms,
    PropertyAcces,
    Function,
    Predicate,
    Property,
    OnActivationEfct,
    IndexAccess
}