namespace Suteki.TardisBank.Tests.WebApi
{
    using System;
    using System.Linq;
    using System.Web.Http.Results;
    using System.Web.Http.Routing;
    using AutoMapper;
    using Domain;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    public class AnnouncementControllerTests : RepositoryTestsBase
    {
        Web.Mvc.Controllers.AnnouncementsController announcementsController;
        Mock<UrlHelper> urlHelper;

        [OneTimeSetUp]
        public void InitializeMappings()
        {
            Mapper.Initialize(c => c.AddProfile<Web.Mvc.ModelMappingProfile>());            
        }

        protected override void LoadTestData()
        {
            this.announcementsController = new Web.Mvc.Controllers.AnnouncementsController(
                new LinqRepository<Announcement>(new TransactionManager(Session), Session));
            urlHelper = new Mock<UrlHelper>();
            announcementsController.Url = urlHelper.Object;
        }

        void AddAnnouncements()
        {
            for (var i = 0; i < 15; i++)
            {
                var announcement = new Announcement
                {
                    Content = "Content " + i,
                    Date = new DateTime(2015, 01, 01).AddDays(i),
                    Title = "Title " + i,
                    LastModifiedUtc = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
                };
                Session.Save(announcement);
            }
            Session.Flush();
        }

        [Test]
        public void GetAll_should_return_announcements_in_descending_order()
        {
            AddAnnouncements();

            announcementsController.Get()
                .Should().BeInDescendingOrder(x => x.Date).And.HaveCount(15);
        }


        [Test]
        public void Latest_should_return_10_latest_announcements()
        {
            AddAnnouncements();

            var res = announcementsController.Latest().ToArray();
            res.Should().HaveCount(10, "should limit output to 10 announcements");
            res.Should().BeInDescendingOrder(x => x.Date, "should sort announcements by date");
        }

        [Test]
        public void Post_should_set_lastModified_on_new_announcement()
        {
            var model = new Web.Mvc.Models.AnnouncementModel
            {
                Date = new DateTime(2015, 12, 30),
                Title = "test title",
                Content = "content"
            };


            object o = null;
            const string location = "http://announcement/x";
            urlHelper.Setup(x => x.Link("DefaultApi", It.IsAny<object>()))
                .Callback<string, object>((route, attr) => o = attr)
                .Returns(location);
            var res = announcementsController.PostAnnouncement(model);
            res.Should().BeOfType<CreatedNegotiatedContentResult<Announcement>>("should respond with created");

            var response = (CreatedNegotiatedContentResult<Announcement>) res;
            response.Content.LastModifiedUtc.Should().BeWithin(1.Seconds());
            response.Content.Id.Should().BePositive();
            response.Location.Should().Be(new Uri(location));
        }

        [Test]
        public void Delete_should_handle_nonexising_announcements()
        {
            announcementsController.Delete(9999);
        }

        [Test]
        public void Get_should_handle_missing_announcements()
        {
            announcementsController.Get(2222).Should().BeNull();
        }
    }
}