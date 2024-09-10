class Prueba
{

    static void Main(string[] args)
    {
        Error error = new Error(1,1,"LOL");
        string str = "PuTa";
        str += "\n" + error;
        System.Console.WriteLine(str);
    }
}