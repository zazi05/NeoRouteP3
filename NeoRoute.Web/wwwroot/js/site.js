// NeoRoute / RaceDay – site scripts
(function () {
    'use strict';

    // Highlight current nav item
    document.addEventListener('DOMContentLoaded', function () {
        var path = window.location.pathname.toLowerCase();
        document.querySelectorAll('.navbar-nav .nav-link').forEach(function (link) {
            var href = (link.getAttribute('href') || '').toLowerCase();
            if (href && path.indexOf(href) !== -1 && href !== '/') {
                link.classList.add('active');
            }
        });
    });
})();
