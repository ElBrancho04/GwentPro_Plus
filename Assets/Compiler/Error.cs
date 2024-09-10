public class Error
{
    public int Line {get;}
    public int Pos {get;}
    public string Message {get;}

    public Error(int line, int pos, string message)
    {
        Line = line;
        Pos = pos;
        Message = message;
    }

    public override string ToString()
    {
        return "Line: " + Line + ", Pos: " + Pos + ", " + Message;
    }
}