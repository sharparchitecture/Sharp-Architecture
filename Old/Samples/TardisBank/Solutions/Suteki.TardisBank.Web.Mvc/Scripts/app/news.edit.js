"use strict";

var app = (function (me, $) {
    me.news = (function(newsModule, $) {
        var announcementsUrl;
        var content;
        var title;
        var status;
        var template;
        var publishedAnnouncements;


        var deleteAnnouncement = function () {
            var id = $(this).attr("data-recId");
            if (id == "") return false;
            var row = $(this).parent("li");

            status.text("");
            $.ajax(announcementsUrl + id, {
                method: "DELETE",
                dataType: "json"
            }).done(function() {
                status.text("Announcement deleted");
                
                row.remove();
            }).fail(function () {
                status.text("Delete failed");
            });
            return false;
        };

        // load announcements
        var displayAnnouncements = function (data) {
            publishedAnnouncements.html("");
            $.each(data, function (key, item) {
                var header = template.render(item, { asLocalDate: app.common.toLocalDate });
                $(header).appendTo(publishedAnnouncements);
            });
            $("#publishedAnnouncements a.delete-link").click(deleteAnnouncement);
        };

        var loadAnnouncements = function () {
            $.getJSON(announcementsUrl + "latest")
                .done(displayAnnouncements)
                .fail(function () {
                    status.text("Cannot load announcements");
                });
        };


        // new announcement
        var addNewAnnouncement = function() {
            if (title.get(0).checkValidity() && content.get(0).checkValidity()) {

                $.post(announcementsUrl, { Title: title.val(), "Date": new Date().toISOString(), Content: content.val() }, "json")
                    .done(function(data) {
                        status.text("Announcement posted");
                        title.val("");
                        content.val("");
                        loadAnnouncements();

                    })
                    .fail(function() {
                        status.text("Error saving announcement");
                    });
            }
        }

        
        
        var newsEditor = {
            init: function() {
                announcementsUrl = app.common.nav.getFullUrl("api/announcements/");

                title = $("#announcementTitle");
                content = $("#announcementContent");
                status = $("#status");
                template = $.templates("#announcementLinkTemplate");
                publishedAnnouncements = $("#publishedAnnouncements");

                $("#addAnnouncement").click(addNewAnnouncement);
                $("#deleteAnnouncement").click(deleteAnnouncement);
            },

            loadAnnouncements : loadAnnouncements
        };

        newsModule.edit = newsEditor;
        return newsModule;
    }(me.news || {}, $));

    return me;
}(app || {}, jQuery));
