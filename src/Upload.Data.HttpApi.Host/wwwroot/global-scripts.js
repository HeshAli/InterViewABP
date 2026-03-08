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
            ".abp-tenant-switch"
        ];

        var containers = [];
        selectors.forEach(function (selector) {
            document.querySelectorAll(selector).forEach(function (el) {
                var container = el.closest(".mb-3, .form-group, .form-floating, .input-group, .tenant-switch-box");
                containers.push(container || el);
            });
        });

        containers.forEach(function (node) {
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

        if (!isLoginPage() || !document.body) {
            return;
        }

        var observer = new MutationObserver(function () {
            applyLoginCustomizations();
        });

        observer.observe(document.body, { childList: true, subtree: true });

        setTimeout(function () {
            observer.disconnect();
        }, 5000);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
