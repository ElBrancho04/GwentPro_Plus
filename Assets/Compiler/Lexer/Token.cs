public enum TokenType
{
    Kewyword,
    Identifier,
    LogOperator,
    AritOperator,
    Comparator,
    Assigment,
    String,
    Number,
    Punctuation,
    EOF
}

public class Token
{
    public TokenType TokenType { get;}
    public object Value { get;}
    public int Line { get;}
    public int Pos { get;}

    public Token(TokenType tokenType, object value, int line, int pos)
    {
        TokenType = tokenType;
        Value = value;
        Line = line;
        Pos = pos;
    }

    public override string ToString()
    {
        return $"{TokenType}:{Value}, {Line}, {Pos}";
    }
}