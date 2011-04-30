using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Validator.Engine;
using SharpArch.Data.NHibernate;
using NHibernate.Validator.Event;

namespace SharpArch.Data.NHibernate
{
    public class SharedEngineProvider : ISharedEngineProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharedEngineProvider"/> class.
        /// </summary>
        public SharedEngineProvider()
        {
        }

        /// <summary>
        /// Provide the shared engine instance.
        /// </summary>
        /// <returns>The validator engine.</returns>
        public ValidatorEngine GetEngine()
        {
            if (NHibernateSession.ValidatorEngine == null)
            {
                NHibernateSession.ValidatorEngine = new ValidatorEngine();
            }

            return NHibernateSession.ValidatorEngine;
        }
    }
}
