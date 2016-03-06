namespace Northwind.Infrastructure
{
    using System.IO;
    using Newtonsoft.Json;
    using Web.Controllers;

    public static class NHibernateExtension
    {
        public static void SerializeToJsonFile<T>(this T itemToSerialize, string filePath) 
        {
            using (var streamWriter = new StreamWriter(filePath)) 
            {
                using (var jsonWriter = new JsonTextWriter(streamWriter)) 
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    var serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new NHibernateContractResolver()

                    };

                    serializer.Serialize(jsonWriter, itemToSerialize);
                }
            }
        }
    }
}
