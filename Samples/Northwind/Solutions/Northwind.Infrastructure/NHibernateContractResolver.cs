using System.Linq;

namespace Northwind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NHibernate.Proxy;

    public class NHibernateContractResolver : DefaultContractResolver
    {
        private static readonly MemberInfo[] NHibernateProxyInterfaceMembers = typeof(INHibernateProxy).GetMembers();

        protected override List<MemberInfo> GetSerializableMembers(Type objectType) 
        {
            var members = base.GetSerializableMembers(objectType);

            members.RemoveAll(memberInfo =>
                              IsMemberPartOfNHibernateProxyInterface(memberInfo) ||
                              IsMemberDynamicProxyMixin(memberInfo) ||
                              IsMemberMarkedWithIgnoreAttribute(memberInfo, objectType) ||
                              IsMemberInheritedFromProxySuperclass(memberInfo, objectType));

            return (from memberInfo in members
                    let infos = memberInfo.DeclaringType.BaseType.GetMember(memberInfo.Name)
                    select infos.Length == 0 ? memberInfo : infos[0]).ToList();
        }

        private static bool IsMemberDynamicProxyMixin(MemberInfo memberInfo) 
        {
            return memberInfo.Name == "__interceptors";
        }

        private static bool IsMemberInheritedFromProxySuperclass(MemberInfo memberInfo, Type objectType) 
        {
            return memberInfo.DeclaringType.Assembly == typeof(INHibernateProxy).Assembly;
        }

        private static bool IsMemberMarkedWithIgnoreAttribute(MemberInfo memberInfo, Type objectType) 
        {
            var infos = typeof(INHibernateProxy).IsAssignableFrom(objectType)
                          ? objectType.BaseType.GetMember(memberInfo.Name)
                          : objectType.GetMember(memberInfo.Name);

            return infos[0].GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0;
        }

        private static bool IsMemberPartOfNHibernateProxyInterface(MemberInfo memberInfo) 
        {
            return Array.Exists(NHibernateProxyInterfaceMembers, mi => memberInfo.Name == mi.Name);
        }
    }


}