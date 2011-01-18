using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpArch.Wcf.NHibernate
{
    public class WebServiceHostFactory : System.ServiceModel.Activation.WebServiceHostFactory
    {
        protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses){
            return new WebServiceHost(serviceType, baseAddresses);
        }
    }
}
