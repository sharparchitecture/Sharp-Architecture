namespace SharpArch.NHibernate.FluentNHibernate
{
    using System;

    using global::FluentNHibernate.Automapping;

    public interface IAutoPersistenceModelGenerator
    {
        AutoPersistenceModel Generate();
    }
}