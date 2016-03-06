// Entity.cs
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using jQueryApi;

namespace Northwind.Client.Models
{
    [Imported]
    [IgnoreNamespace]
    [ScriptName("Object")]
    public sealed class ValidationResults : Record
    {
        [PreserveCase] 
        public ValidationResult[] ValidationResult;

        public static implicit operator ValidationResults(Dictionary o)
        {
            return null;
        }

        [PreserveCase]
        public IEnumerator GetEnumerator() 
        {
            return (this.ValidationResult as IEnumerable).GetEnumerator();
        }
    }

    [Imported]
    [IgnoreNamespace]
    [ScriptName("Object")]
    public sealed class ValidationResult : Record
    {
        [PreserveCase] public string PropertyName;
        [PreserveCase] public string Message;

        public static implicit operator ValidationResult(Dictionary o) { return null; }
    }

    [Imported]
    [IgnoreNamespace]
    [ScriptName("Object")]
    public abstract class Entity
    {
        [PreserveCase] public int Id;
        [PreserveCase] public bool Valid;
        [PreserveCase] public ValidationResult[] ValidationResultsJson;
    }

    [Imported]
    [IgnoreNamespace]
    [ScriptName("Object")]
    public abstract class EntityWithAssignedId
    {
        [PreserveCase] public string Id;
        [PreserveCase] public bool Valid;
        [PreserveCase] public ValidationResult[] ValidationResultsJson;
    }

    [Imported]
    [ScriptName("Object")]
    [IgnoreNamespace]
    public class Dialogs : jQueryObject
    {
        private Dialogs() { }

        public jQueryObject Dialog(params object[] nameValuePairs) 
        {
            return null;
        }
    }
}
