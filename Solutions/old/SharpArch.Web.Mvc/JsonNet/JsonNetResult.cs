namespace SharpArch.Web.Mvc.JsonNet
{
    using System;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    /// <summary>
    ///     An ActionResult to return JSON from ASP.NET MVC to the browser using Json.NET.
    ///     Taken from http://james.newtonking.com/archive/2008/10/16/asp-net-mvc-and-json-net.aspx
    /// </summary>
    [PublicAPI]
    public class JsonNetResult : ActionResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetResult"/> class.
        /// </summary>
        public JsonNetResult() : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetResult"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public JsonNetResult(object data) : this(data, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetResult"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        public JsonNetResult(object data, string contentType) : this(data, contentType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetResult"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="encoding">The encoding.</param>
        public JsonNetResult(object data, string contentType, Encoding encoding)
        {
            this.SerializerSettings = new JsonSerializerSettings();

            this.Data = data;
            this.ContentType = contentType;
            this.ContentEncoding = encoding;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Content encoding.
        /// </summary>
        public Encoding ContentEncoding { get; set; }

        /// <summary>
        /// MIME Content type.
        /// </summary>
        /// <remarks>
        /// See http://www.freeformatter.com/mime-types-list.html#mime-types-list for list of content types.
        /// </remarks>
        public string ContentType { get; set; }

        /// <summary>
        /// Data to serialize.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Controls Json formatting.
        /// </summary>
        [CLSCompliant(false)]
        public Formatting Formatting { get; set; }

        /// <summary>
        /// Json serializer settings.
        /// </summary>
        [CLSCompliant(false)]
        public JsonSerializerSettings SerializerSettings { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the <see cref="T:System.Web.Mvc.ActionResult" /> class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(this.ContentType) ? this.ContentType : "application/json";

            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }

            if (this.Data != null)
            {
                var writer = new JsonTextWriter(response.Output) { Formatting = Formatting };
                JsonSerializer serializer = JsonSerializer.Create(this.SerializerSettings);
                serializer.Serialize(writer, this.Data);
                writer.Flush();
            }
        }

        #endregion
    }
}