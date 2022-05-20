namespace Suteki.TardisBank.Domain;

using System.Runtime.Serialization;


[Serializable]
public class TardisBankException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public TardisBankException()
    {
    }

    public TardisBankException(string format, params object[] args)
        : this(string.Format(format, args))
    {
    }

    public TardisBankException(string message)
        : base(message)
    {
    }

    public TardisBankException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected TardisBankException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context)
    {
    }
}
