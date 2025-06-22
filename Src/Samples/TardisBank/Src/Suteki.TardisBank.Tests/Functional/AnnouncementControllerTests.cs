namespace Suteki.TardisBank.Tests.Functional;

using Api.Announcements;
using Shouldly;
using Setup;
using Xunit;

[Trait("Category", "Functional")]
public class AnnouncementControllerTests : IClassFixture<TestServerSetup>, IDisposable
{
    readonly TestServerSetup _setup;
    Uri? _newAnnouncementUri;

    public AnnouncementControllerTests(TestServerSetup setup)
    {
        _setup = setup ?? throw new ArgumentNullException(nameof(setup));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_newAnnouncementUri != null)
        {
            DeleteAnnouncement(_newAnnouncementUri).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    Task DeleteAnnouncement(Uri uri)
        => _setup.Client.DeleteAsync(uri);

    [Fact]
    public async Task CanSaveAnnouncement()
    {
        var today = DateTime.Now.Date;
        var uid = Guid.NewGuid().ToString("N");
        var newAnnouncement = new NewAnnouncement
        {
            Date = today,
            Content = "New announcement " + today,
            Title = uid
        };
        var response = await _setup.Client.PostAsJsonAsync("announcements", newAnnouncement);
        response.EnsureSuccessStatusCode();

        _newAnnouncementUri = response.Headers.Location!;

        var announcementResponse = await _setup.Client.GetAsync(_newAnnouncementUri);
        announcementResponse.EnsureSuccessStatusCode();
        var announcementSummary = await announcementResponse.Content.ReadAsAsync<AnnouncementModel>();
        announcementSummary.Id.ShouldBeGreaterThan(0);
        announcementSummary.Title.ShouldBe(newAnnouncement.Title);

        await DeleteAnnouncement(_newAnnouncementUri);
    }
}