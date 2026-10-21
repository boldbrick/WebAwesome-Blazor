// DevTools page of the WA Inspector: adds the "Web Awesome" panel. The panel page (panel.html) does the work; in
// DevTools it finds the inspected tab through chrome.devtools.inspectedWindow.
chrome.devtools.panels.create('Web Awesome', '', 'panel.html?mode=devtools');
