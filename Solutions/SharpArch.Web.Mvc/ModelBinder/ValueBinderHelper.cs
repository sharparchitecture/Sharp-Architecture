namespace SharpArch.Web.Mvc.ModelBinder
{
    using System;
    using System.Reflection;

    internal static class ValueBinderHelper
    {
        internal static object GetEntityFor(Type collectionEntityType, object typedId, Type idType)
        {
            var entityRepository = GenericRepositoryFactory.CreateEntityRepositoryFor(collectionEntityType, idType);

            return entityRepository.GetType().InvokeMember(
                "get_Item", BindingFlags.InvokeMethod, null, entityRepository, new[] { typedId });
        }
    }
}