namespace SharpArch.Web.Mvc.ModelBinder
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    using SharpArch.Domain.DomainModel;

    public class SharpModelBinder : DefaultModelBinder
    {
        private const string IdPropertyName = "Id";

        protected override object GetPropertyValue(
            ControllerContext controllerContext, 
            ModelBindingContext bindingContext, 
            PropertyDescriptor propertyDescriptor, 
            IModelBinder propertyBinder)
        {
            var propertyType = propertyDescriptor.PropertyType;

            if (IsEntityType(propertyType))
            {
                // use the EntityValueBinder
                return base.GetPropertyValue(
                    controllerContext, bindingContext, propertyDescriptor, new EntityValueBinder());
            }

            if (IsSimpleGenericBindableEntityCollection(propertyType))
            {
                // use the EntityValueCollectionBinder
                return base.GetPropertyValue(
                    controllerContext, bindingContext, propertyDescriptor, new EntityCollectionValueBinder());
            }

            return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
        }

        /// <summary>
        ///     Called when the model is updating. We handle updating the Id property here because it gets filtered out
        ///     of the normal MVC2 property binding.
        /// </summary>
        /// <param name = "controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <param name = "bindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
        /// <returns>
        ///     true if the model is updating; otherwise, false.
        /// </returns>
        protected override bool OnModelUpdating(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (IsEntityType(bindingContext.ModelType))
            {
                // handle the Id property
                var idProperty =
                    (from PropertyDescriptor property in TypeDescriptor.GetProperties(bindingContext.ModelType)
                     where property.Name == IdPropertyName
                     select property).SingleOrDefault();

                this.BindProperty(controllerContext, bindingContext, idProperty);
            }

            return base.OnModelUpdating(controllerContext, bindingContext);
        }

        protected override void SetProperty(
            ControllerContext controllerContext, 
            ModelBindingContext bindingContext, 
            PropertyDescriptor propertyDescriptor, 
            object value)
        {
            if (propertyDescriptor.Name == IdPropertyName)
            {
                SetIdProperty(bindingContext, propertyDescriptor, value);
            }
            else if (value as IEnumerable != null &&
                     IsSimpleGenericBindableEntityCollection(propertyDescriptor.PropertyType))
            {
                SetEntityCollectionProperty(bindingContext, propertyDescriptor, value);
            }
            else
            {
                base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
            }
        }

        private static bool IsEntityType(Type propertyType)
        {
            var isEntityType =
                propertyType.GetInterfaces().Any(
                    type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));

            return isEntityType;
        }

        private static bool IsSimpleGenericBindableEntityCollection(Type propertyType)
        {
            var isSimpleGenericBindableCollection = propertyType.IsGenericType &&
                                                    (propertyType.GetGenericTypeDefinition() == typeof(IList<>) ||
                                                     propertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                                     propertyType.GetGenericTypeDefinition() ==
                                                     typeof(Iesi.Collections.Generic.ISet<>) ||
                                                     propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            var isSimpleGenericBindableEntityCollection = isSimpleGenericBindableCollection &&
                                                          IsEntityType(propertyType.GetGenericArguments().First());

            return isSimpleGenericBindableEntityCollection;
        }

        /// <summary>
        ///     If the property being bound is a simple, generic collection of entiy objects, then use
        ///     reflection to get past the protected visibility of the collection property, if necessary.
        /// </summary>
        private static void SetEntityCollectionProperty(
            ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            var entityCollection = propertyDescriptor.GetValue(bindingContext.Model);
            if (entityCollection != value)
            {
                var entityCollectionType = entityCollection.GetType();

                foreach (var entity in value as IEnumerable)
                {
                    entityCollectionType.InvokeMember(
                        "Add", 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                        null, 
                        entityCollection, 
                        new[] { entity });
                }
            }
        }

        /// <summary>
        ///     If the property being bound is an Id property, then use reflection to get past the
        ///     protected visibility of the Id property, accordingly.
        /// </summary>
        private static void SetIdProperty(
            ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            var idType = propertyDescriptor.PropertyType;
            object typedId;

            if (value == null)
            {
                typedId = idType.IsValueType ? Activator.CreateInstance(idType) : null;
            }
            else
            {
                //handle nullable type
                //http://stackoverflow.com/questions/793714/how-can-i-fix-this-up-to-do-generic-conversion-to-nullablet
                Type underlyingType = Nullable.GetUnderlyingType(idType);

                if (underlyingType == null)
                {
                    typedId = Convert.ChangeType(value, idType);
                }
                else
                {
                    typedId = Convert.ChangeType(value, underlyingType);
                }
            }

            // First, look to see if there's an Id property declared on the entity itself;
            // e.g., using the new keyword
            var idProperty = bindingContext.ModelType.GetProperty(propertyDescriptor.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) 
                             ?? bindingContext.ModelType.GetProperty(propertyDescriptor.Name, BindingFlags.Public | BindingFlags.Instance);

            // If an Id property wasn't found on the entity, then grab the Id property from
            // the entity base class

            // Set the value of the protected Id property
            idProperty.SetValue(bindingContext.Model, typedId, null);
        }
    }
}