"use strict";

var app = (function(me, $) {

    me.news = (function(newsModule, $) {

        var announcementDetails;
        var getAnnouncementUrlPart;
        var loadAnnouncementsUrl;
        var articleTemplate;
        var articleLinkTemplate;

        var displayAnnouncement = function() {
            var id = $(this).attr("data-recId");
            var url = getAnnouncementUrlPart + id;
            announcementDetails.html("<i>Loading...</i>");
            // todo: Add error handling
            $.getJSON(url).done(function(data) {
                var formattedArticle = articleTemplate.render(data, {
                        asLocalDate: app.common.toLocalDate
                    }
                );
                announcementDetails.html(formattedArticle);
            });
            return false;
        };

        // public methods
        var newsViewer = {
            init: function(root) {
                getAnnouncementUrlPart = root + "api/announcements/";
                loadAnnouncementsUrl = root + "api/announcements/latest";

                announcementDetails = $("#announcementDetails");
                articleTemplate = $.templates("#announcementContentTemplate");
                articleLinkTemplate = $.templates("#announcementLinkTemplate");
            },

            loadAnnouncements: function() {
                // todo: Add error handling
                $.getJSON(loadAnnouncementsUrl).done(function(data) {
                    var newsList = $("#news");
                    $.each(data, function(key, item) {
                        var header = articleLinkTemplate.render(item, { asLocalDate: app.common.toLocalDate});
                        $(header).appendTo(newsList);
                    });
                    $("#news a").click(displayAnnouncement);
                });
            }
        };


        newsModule.view = newsViewer;
        return newsModule;
    }(me.news || {}, $));

    return me;

}(app || {}, jQuery));