namespace SharpArch.Infrastructure.Caching
{
    using System;


    public class CachedData
    {
        public DateTime ModificationDateUtc { get; }

        public byte[] Data { get; }
    }
}
