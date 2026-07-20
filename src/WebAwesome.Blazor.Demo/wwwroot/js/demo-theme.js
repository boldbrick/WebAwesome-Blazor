// demo-local theming helper: theme stylesheet swap + wa-dark/wa-light color scheme classes
window.demoTheme = {
    setDark: function (dark) {
        document.documentElement.classList.toggle('wa-dark', dark);
        document.documentElement.classList.toggle('wa-light', !dark);
    },
    setTheme: function (theme, href) {
        // Web Awesome theme stylesheets scope their rules under a "wa-theme-{name}" class
        // (e.g. ".wa-theme-awesome"), so swapping the <link> alone has no visible effect
        // unless that class is also applied to <html>
        var html = document.documentElement;
        Array.from(html.classList)
            .filter(function (c) { return c.indexOf('wa-theme-') === 0; })
            .forEach(function (c) { html.classList.remove(c); });

        let link = document.getElementById('wa-theme');
        if (href) {
            if (!link) {
                link = document.createElement('link');
                link.rel = 'stylesheet';
                link.id = 'wa-theme';
                document.head.appendChild(link);
            }
            link.href = href;
            html.classList.add('wa-theme-' + theme);
        }
        else if (link) {
            link.remove();
        }
    }
};
