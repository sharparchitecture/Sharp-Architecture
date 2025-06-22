namespace Suteki.TardisBank.Domain;

[Serializable]
public class CashWithdrawException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public CashWithdrawException()
    {
    }

    public CashWithdrawException(string message)
        : base(message)
    {
    }

    public CashWithdrawException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
