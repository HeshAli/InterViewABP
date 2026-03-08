/* Global scripts */
(function () {
    var loginPath = "/account/login";

    function isLoginPage() {
        return window.location.pathname.toLowerCase() === loginPath;
    }

    function removeTenantSection() {
        var selectors = [
            "#AbpTenantSwitchLink",
            "[id*='TenantSwitch']",
            ".tenant-switch-box",
            ".abp-tenant-switch",
            "[data-tenant-switch]"
        ];

        var nodes = [];

        selectors.forEach(function (selector) {
            document.querySelectorAll(selector).forEach(function (el) {
                var tenantContainer = el.closest("[data-tenant-switch], .tenant-switch-box, .abp-tenant-switch");
                nodes.push(tenantContainer || el);
            });
        });

        nodes.forEach(function (node) {
            if (node && node.parentNode) {
                node.parentNode.removeChild(node);
            }
        });
    }

    function replaceBrandText() {
        if (!document.body) {
            return;
        }

        var pattern = /\bupload\.data\b/gi;
        var walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        var node = walker.nextNode();

        while (node) {
            if (node.nodeValue && pattern.test(node.nodeValue)) {
                node.nodeValue = node.nodeValue.replace(pattern, "Upload Data");
            }

            pattern.lastIndex = 0;
            node = walker.nextNode();
        }

        if (document.title && pattern.test(document.title)) {
            document.title = document.title.replace(pattern, "Upload Data");
        }
    }

    function applyLoginCustomizations() {
        if (!isLoginPage()) {
            return;
        }

        removeTenantSection();
        replaceBrandText();
    }

    function init() {
        applyLoginCustomizations();

        if (!isLoginPage()) {
            return;
        }

        // Run once more shortly after first paint to catch late-rendered tenant UI,
        // without continuously mutating the login DOM.
        setTimeout(applyLoginCustomizations, 250);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
