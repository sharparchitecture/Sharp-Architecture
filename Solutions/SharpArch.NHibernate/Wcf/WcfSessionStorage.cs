namespace SharpArch.NHibernate.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    
    using global::NHibernate;

    public class WcfSessionStorage : ISessionStorage
    {
        private ISessionStorage fallbackSessionStorage;

        public WcfSessionStorage()
            : this(null)
        {
        }

        public WcfSessionStorage(ISessionStorage fallbackSessionStorage)
        {
            this.FallbackSessionStorage = fallbackSessionStorage;
        }

        private ISessionStorage FallbackSessionStorage
        {
            get
            {
                return this.fallbackSessionStorage ?? (this.fallbackSessionStorage = new SimpleSessionStorage());
            }

            set
            {
                this.fallbackSessionStorage = value;
            }
        }

        #region ISessionStorage Members
        public IEnumerable<ISession> GetAllSessions()
        {
            if (OperationContext.Current == null)
            {
                return this.FallbackSessionStorage.GetAllSessions();
            }

            SessionInstanceExtension instance = GetSessionInstanceExtension();
            return instance.GetAllSessions();
        }

        public ISession GetSessionForKey(string factoryKey)
        {
            if (OperationContext.Current == null)
            {
                return this.FallbackSessionStorage.GetSessionForKey(factoryKey);
            }

            SessionInstanceExtension instance = GetSessionInstanceExtension();
            return instance.GetSessionForKey(factoryKey);
        }

        public void SetSessionForKey(string factoryKey, ISession session)
        {
            if (OperationContext.Current == null)
            {
                this.FallbackSessionStorage.SetSessionForKey(factoryKey, session);
                return;
            }

            SessionInstanceExtension instance = GetSessionInstanceExtension();
            instance.SetSessionForKey(factoryKey, session);
        }
        #endregion

        private SessionInstanceExtension GetSessionInstanceExtension()
        {
            SessionInstanceExtension instance =
                OperationContext.Current.InstanceContext.Extensions.Find<SessionInstanceExtension>();

            if (instance == null)
            {
                throw new Exception("SessionInstanceExtension not found in current OperationContext");
            }

            return instance;
        }
    }
}
