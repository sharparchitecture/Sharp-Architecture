var app = function(me, $) {
    me.common = function(commonModule) {

        var rootUrl;

        commonModule.toLocalDate = function(aDate) {
            var date = new Date(aDate);
            return date.toLocaleDateString();
        };

        // navigation
        commonModule.nav = {
            setRootUrl : function(url) {
                rootUrl = url;
            },
            getFullUrl : function(path) {
                return rootUrl + path;
            },
            toLoginPage: function() {
                location.replace(app.common.nav.getFullUrl("User/Login"));
            }
        };


        // redirect unauthorized user requests to login page
        $(document).ajaxError(function(event, jqxhr, settings, thrownError) {
            if (jqxhr.status == 401) {
                app.common.nav.toLoginPage();
            }
        });

        return commonModule;
    }(me.common || {});
    return me;
}(app || {}, jQuery);