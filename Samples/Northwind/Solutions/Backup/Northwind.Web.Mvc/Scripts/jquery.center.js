jQuery.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.outerHeight()) / 4 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.outerWidth()) / 2 + $(window).scrollLeft() + "px");
    return this;
}