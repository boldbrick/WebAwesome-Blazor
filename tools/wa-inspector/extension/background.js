// Service worker of the WA Inspector. It routes the messages between the panel pages (DevTools panel, tab, side
// panel) and the content script of the inspected tab, takes the screenshots, and saves the export files.
//
// Panel -> content: the panel connects a port named "panel:<tabId>" and posts { id, method, params }; a method
// "page.*" goes to the content script of that tab, and the reply comes back as { id, result } or { id, error }.
// Content -> panel: the content script sends { event, data }; it goes to every panel port of the sender's tab.

// the open panel ports by inspected tab id
const panelsByTab = new Map();

// captureVisibleTab is limited to two calls per second
const CAPTURE_INTERVAL_MS = 600;
let lastCaptureAt = 0;

chrome.runtime.onInstalled.addListener(() => {
  // the toolbar button opens the side panel
  chrome.sidePanel.setPanelBehavior({ openPanelOnActionClick: true }).catch(() => {});
});

chrome.runtime.onConnect.addListener(port => {
  const match = /^panel:(\d+)$/.exec(port.name);
  if (!match) return;

  const tabId = Number(match[1]);
  if (!panelsByTab.has(tabId)) panelsByTab.set(tabId, new Set());
  panelsByTab.get(tabId).add(port);

  port.onMessage.addListener(message => {
    handlePanelMessage(tabId, message)
      .then(result => post(port, { id: message.id, result }))
      .catch(error => post(port, { id: message.id, error: String(error && error.message || error) }));
  });

  port.onDisconnect.addListener(() => {
    const ports = panelsByTab.get(tabId);
    if (!ports) return;
    ports.delete(port);
    if (ports.size === 0) {
      panelsByTab.delete(tabId);
      // the last panel of the tab closed: stop the pick mode and the hover highlight
      sendToContent(tabId, 'page.panelClosed', {}).catch(() => {});
    }
  });
});

chrome.runtime.onMessage.addListener((message, sender) => {
  if (!message || !message.event || !sender.tab) return;
  const ports = panelsByTab.get(sender.tab.id);
  if (!ports) return;
  for (const port of ports) post(port, { event: message.event, data: message.data });
});

/**
 * Handles one request of a panel.
 *
 * @param {number} tabId the inspected tab
 * @param {{ id: number, method: string, params?: any }} message the request
 * @returns {Promise<any>} the result
 */
async function handlePanelMessage(tabId, message) {
  const { method, params } = message;
  if (method.startsWith('page.')) return sendToContent(tabId, method, params || {});

  switch (method) {
    case 'capture':
      return capture(tabId, params && params.crop);
    case 'download':
      return download(params);
    case 'tabInfo': {
      const tab = await chrome.tabs.get(tabId);
      return { id: tab.id, url: tab.url, title: tab.title, windowId: tab.windowId };
    }
    default:
      throw new Error(`unknown method ${method}`);
  }
}

/**
 * Sends a request to the content script of the top frame of a tab.
 *
 * @param {number} tabId
 * @param {string} method
 * @param {any} params
 */
async function sendToContent(tabId, method, params) {
  let reply;
  try {
    reply = await chrome.tabs.sendMessage(tabId, { method, params }, { frameId: 0 });
  } catch (error) {
    // no content script: a page opened before the extension was loaded, or a page the extension cannot access
    throw new Error('no-content-script');
  }
  if (!reply) throw new Error('no-content-script');
  if (reply.error) throw new Error(reply.error);
  return reply.result;
}

/**
 * Takes a screenshot of the visible part of the tab, optionally cropped to a rectangle in CSS pixels.
 *
 * @param {number} tabId
 * @param {{ x: number, y: number, width: number, height: number, dpr: number } | undefined} crop
 * @returns {Promise<string>} a PNG data URL
 */
async function capture(tabId, crop) {
  const tab = await chrome.tabs.get(tabId);
  // captureVisibleTab takes the active tab of the window: a hidden inspected tab would give a picture of another tab
  if (!tab.active) throw new Error('the inspected tab is not visible');
  const wait = lastCaptureAt + CAPTURE_INTERVAL_MS - Date.now();
  if (wait > 0) await new Promise(resolve => setTimeout(resolve, wait));
  lastCaptureAt = Date.now();

  const dataUrl = await chrome.tabs.captureVisibleTab(tab.windowId, { format: 'png' });
  if (!crop) return dataUrl;

  const blob = await (await fetch(dataUrl)).blob();
  const bitmap = await createImageBitmap(blob);
  const scale = crop.dpr || 1;
  const x = Math.max(0, Math.floor(crop.x * scale));
  const y = Math.max(0, Math.floor(crop.y * scale));
  const width = Math.max(1, Math.min(bitmap.width - x, Math.ceil(crop.width * scale)));
  const height = Math.max(1, Math.min(bitmap.height - y, Math.ceil(crop.height * scale)));

  const canvas = new OffscreenCanvas(width, height);
  canvas.getContext('2d').drawImage(bitmap, x, y, width, height, 0, 0, width, height);
  const cropped = await canvas.convertToBlob({ type: 'image/png' });
  return blobToDataUrl(cropped);
}

/**
 * Saves one export file under the Downloads folder.
 *
 * @param {{ filename: string, dataUrl?: string, text?: string, mime?: string }} params
 * @returns {Promise<number>} the download id
 */
async function download(params) {
  const url = params.dataUrl || `data:${params.mime || 'text/plain'};charset=utf-8,${encodeURIComponent(params.text || '')}`;
  return chrome.downloads.download({ url, filename: params.filename, saveAs: false, conflictAction: 'uniquify' });
}

async function blobToDataUrl(blob) {
  const bytes = new Uint8Array(await blob.arrayBuffer());
  let binary = '';
  const chunk = 0x8000;
  for (let index = 0; index < bytes.length; index += chunk) {
    binary += String.fromCharCode.apply(null, bytes.subarray(index, index + chunk));
  }
  return `data:${blob.type};base64,${btoa(binary)}`;
}

function post(port, message) {
  try {
    port.postMessage(message);
  } catch {
    // the panel closed while the request ran
  }
}
