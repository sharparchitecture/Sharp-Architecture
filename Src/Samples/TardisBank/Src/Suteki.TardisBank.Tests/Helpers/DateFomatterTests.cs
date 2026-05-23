#if false
using NUnit.Framework;

namespace Suteki.TardisBank.Tests.Helpers
{
    using FluentAssertions;
    using global::Suteki.TardisBank.Web.Mvc.Utilities;
    using SharpArch.Testing.NUnit;

    [TestFixture]
    public class DateFomatterTests 
    {
        [Fact]
        public void Should_return_the_correct_date_format()
        {
            GetJQueryDateFormatFor("en-GB").Should().Be("dd/mm/yy");
            GetJQueryDateFormatFor("en-US").Should().Be("m/d/yy");
            GetJQueryDateFormatFor("fr").Should().Be("dd/mm/yy");
            GetJQueryDateFormatFor("de-CH").Should().Be("dd.mm.yy");
            GetJQueryDateFormatFor("de").Should().Be("dd.mm.yy");
        }

        static string GetJQueryDateFormatFor(string language)
        {
            var culture = new System.Globalization.CultureInfo(language);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            return DateFormatter.CurrentJQuery;
        }
    }
}
#endif
