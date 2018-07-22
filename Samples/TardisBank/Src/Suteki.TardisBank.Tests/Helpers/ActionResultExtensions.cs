#if false

// todo check later
using System;
using System.Runtime.Serialization;
using System.Web.Mvc;
namespace Suteki.TardisBank.Tests.Helpers
{
    using global::Suteki.TardisBank.Web.Mvc.Utilities;

    public static class ActionResultExtensions
    {
        public static ViewResult ReturnsViewResult(this ActionResult result)
        {
            var viewResult = result as ViewResult;
            if (viewResult == null)
            {
                throw new TestHelperException("result is not a ViewResult");
            }
            return viewResult;
        }

        public static RedirectToRouteResult ReturnsRedirectToRouteResult(this ActionResult result)
        {
            var viewResult = result as RedirectToRouteResult;
            if (viewResult == null)
            {
                throw new TestHelperException("result is not a RedirectToRouteResult");
            }
            return viewResult;
        }

        public static RedirectResult ReturnsRedirect(this ActionResult result)
        {
            var redirectResult = result as RedirectResult;
            if (redirectResult == null)
            {
                throw new TestHelperException("result is not a RedirectResult");
            }

            return redirectResult;
        }

        public static PartialViewResult ReturnsPartialViewResult(this ActionResult result)
        {
            var viewResult = result as PartialViewResult;
            if (viewResult == null)
            {
                throw new TestHelperException("result is not a PartialResult");
            }
            return viewResult;
        }

        public static ContentResult ReturnsContentResult(this ActionResult result)
        {
            var contentResult = result as ContentResult;
            if (contentResult == null)
            {
                throw new TestHelperException("result is not a ContentResult");
            }
            return contentResult;
        }

        public static TResult ReturnsResult<TResult>(this ActionResult result) where TResult : ActionResult
        {
            var actualResult = result as TResult;
            if (actualResult == null)
            {
                throw new TestHelperException("Result is not a {0}".With(typeof(TResult).Name));
            }
            return actualResult;
        }

        public static RedirectToRouteResult ToAction(this RedirectToRouteResult result, string actionName)
        {
            return result.WithRouteValue("action", actionName);
        }

        public static RedirectToRouteResult ToController(this RedirectToRouteResult result, string controllerName)
        {
            return result.WithRouteValue("controller", controllerName);
        }

        public static RedirectToRouteResult WithRouteValue(this RedirectToRouteResult result, string key, string value)
        {
            if (result.RouteValues[key] == null)
            {
                throw new TestHelperException(string.Format("route value {0} is null", key));
            }

            if (result.RouteValues[key].ToString() != value)
            {
                throw new TestHelperException(string.Format("redirect {0} is {1}, expected {2}",
                    key, result.RouteValues[key], value));
            }
            return result;
        }

        public static JsonResult ReturnsJsonResult(this ActionResult result)
        {
            var jsonResult = result as JsonResult;
            if (jsonResult == null)
            {
                throw new TestHelperException("result is not a jsonResult");
            }
            return jsonResult;
        }

        public static T WithModel<T>(this ViewResult viewResult) where T : class
        {
            var model = viewResult.ViewData.Model as T;
            if (model == null)
            {
                throw new TestHelperException("model is not an instance of {0}".With(typeof(T).Name));
            }
            return model;
        }

        public static ViewResult ForView(this ViewResult result, string viewName)
        {
            if (viewName != result.ViewName)
            {
                throw new TestHelperException("ViewResult.ViewName is not '{0}', but {1}".With(viewName, result.ViewName));
            }
            return result;
        }

        public static PartialViewResult ForView(this PartialViewResult result, string viewName)
        {
            if (viewName != result.ViewName)
            {
                throw new TestHelperException("PartialViewResult.ViewName is not {0}".With(viewName));
            }
            return result;
        }

        public static TViewData AssertNotNull<TViewData, TProperty>(this TViewData viewData, Func<TViewData, TProperty> property)
            where TProperty : class
        {
            if (property(viewData) == null)
            {
                throw new TestHelperException("Property is null");
            }
            return viewData;
        }

        public static TViewData AssertNull<TViewData, TProperty>(this TViewData viewData, Func<TViewData, TProperty> property)
            where TProperty : class
        {
            if (property(viewData) != null)
            {
                throw new TestHelperException("Property is not null");
            }
            return viewData;
        }

        public static TViewData AssertAreSame<TViewData, TProperty>(
            this TViewData viewData,
            TProperty expected,
            Func<TViewData, TProperty> property)
            where TProperty : class
        {
            if (expected != property(viewData))
            {
                throw new TestHelperException(
                    "Not same, expected {0}, actual {1}".With(expected, property(viewData)));
            }
            return viewData;
        }

        public static TViewData AssertAreSame<TViewData>(
            this TViewData viewData,
            TViewData expected)
            where TViewData : class
        {
            return viewData.AssertAreSame(expected, vd => vd);
        }


        public static TViewData AssertAreEqual<TViewData, TProperty>(
            this TViewData viewData,
            TProperty expected,
            Func<TViewData, TProperty> property)
        {
            if (!expected.Equals(property(viewData)))
            {
                throw new TestHelperException(
                    "Not equal, expected {0}, actual {1}".With(expected, property(viewData)));
            }
            return viewData;
        }

        public static TViewData AssertIsTrue<TViewData>(
            this TViewData viewData,
            Predicate<TViewData> predicate)
        {
            if (!predicate(viewData))
            {
                throw new TestHelperException("False");
            }
            return viewData;
        }

        public static TViewData Callback<TViewData>(
            this TViewData viewData,
            Action<TViewData> action)
        {
            action(viewData);
            return viewData;
        }
    }

    [Serializable]
    public class TestHelperException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TestHelperException()
        {
        }

        public TestHelperException(string message)
            : base(message)
        {
        }

        public TestHelperException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TestHelperException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}

#endif