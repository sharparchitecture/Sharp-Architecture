using NHibernate.Validator.Engine;
using NHibernate.Validator.Event;
using SharpArch.Data.NHibernate;

namespace SharpArch.Core.NHibernateValidator.ValidatorProvider
{
    internal class ValidatorEngineFactory
    {
        public static ValidatorEngine ValidatorEngine
        {
            get
            {
                if (NHibernate.Validator.Cfg.Environment.SharedEngineProvider == null)
                {
                    NHibernate.Validator.Cfg.Environment.SharedEngineProvider = new SharedEngineProvider();
                }

                return NHibernate.Validator.Cfg.Environment.SharedEngineProvider.GetEngine();
            }
        }
    }
}