// @ts-check
const { expect } = require('../test');
const { callOk } = require('../method-harness');
const { installPageClock } = require('../page-clock');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "media-content" (tools\e2e\data\method-groups.json): WaAnimatedImage, WaAnimation,
// WaZoomableFrame, WaVideo, WaVideoPlaylist, WaMarkdown, WaMutationObserver, WaResizeObserver, WaRandomContent,
// WaRelativeTime and WaCopyButton on /testing/methods-media-content (MethodsMediaContentHarness.razor). Every call runs
// from .NET through the wrapper's @ref (a MethodCall button); the case asserts the effect on the element, and the
// returned value where there is one. wa-video and wa-video-playlist are Pro components, so their cases skip on the free
// CDN.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const ANIMATED_IMAGE = 'wa-animated-image';
const ANIMATION = 'wa-animation';
const ZOOMABLE_FRAME = 'wa-zoomable-frame';
const VIDEO = 'wa-video';
const PLAYLIST = 'wa-video-playlist';
const MARKDOWN = 'wa-markdown';
const MUTATION_OBSERVER = 'wa-mutation-observer';
const RESIZE_OBSERVER = 'wa-resize-observer';
const RANDOM_CONTENT = 'wa-random-content';
const RELATIVE_TIME = 'wa-relative-time';
const COPY_BUTTON = 'wa-copy-button';

// the values of the harness
const ANIMATION_DURATION_MS = 10_000;
const HALF_ANIMATION_TIME_MS = 5_000;
const CLIP_SECONDS = 6;
const SEEK_SECONDS = 3;
const HALF_VOLUME = 0.5;
// SetPlaybackRateAsync(1.4) snaps to the nearest rate of the element's list
const SNAPPED_PLAYBACK_RATE = 1.5;
const LAST_PLAYLIST_INDEX = 2;
const PLAYLIST_LENGTH = 3;
const RANDOM_ITEMS = 3;
const COPIED_VALUE = 'copied by CopyAsync';

// the widths the resize case gives the observed element (the harness renders it 100 px wide)
const CONNECTED_WIDTH = 150;
const DISCONNECTED_WIDTH = 200;

// the relative time of the harness is 10 minutes old; the shift makes it 70 minutes old
const INITIAL_PHRASE = '10 minutes ago';
const SHIFTED_PHRASE = '1 hour ago';
const CLOCK_SHIFT_MS = 60 * 60 * 1000;

// a call triggered by a click event only (see callMethod): the exit from fullscreen, where the page under the
// fullscreen element cannot take a real click
const QUIET = { quiet: true };

// the delay of the element modules in the first-render case, so the first render certainly comes before the definitions
// (long enough for the circuit of a prerendered page to start first)
const MODULE_DELAY_MS = 4_000;

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-media-content?early=true';

/**
 * Waits until the page is interactive (a MethodCall button is enabled). A prerendered page replaces its elements with
 * the first interactive render, so a case that reads or changes an element before its first call waits for this first.
 *
 * @param {Page} page
 * @param {string} id test id of a MethodCall of the case
 */
async function whenInteractive(page, id) {
  await expect(page.getByTestId(id), 'the page is interactive').toBeEnabled({ timeout: WA_READY_TIMEOUT_MS });
}

/**
 * Waits until the page has rendered two more frames, so a ResizeObserver or MutationObserver of the element had its
 * chance to report a change.
 *
 * @param {Page} page
 */
function nextFrames(page) {
  return page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve(undefined)))));
}

/**
 * Counts the events of one type that an element dispatches from now on, in the page; returns a reader of the count
 * and of the last event's detail.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} type
 * @returns {Promise<{ count: () => Promise<number>, lastWidth: () => Promise<number | null> }>}
 */
async function recordEvents(page, testId, type) {
  const element = page.getByTestId(testId);
  await element.evaluate((el, eventType) => {
    const host = /** @type {any} */ (el);
    host.recordedEvents = [];
    el.addEventListener(eventType, event => host.recordedEvents.push(/** @type {CustomEvent} */ (event).detail));
  }, type);
  return {
    count: () => element.evaluate(el => /** @type {any} */ (el).recordedEvents.length),
    // the observed width of the last wa-resize event
    lastWidth: () => element.evaluate(el => {
      const events = /** @type {any} */ (el).recordedEvents;
      const entries = events.length ? events[events.length - 1]?.entries : null;
      return entries?.length ? Math.round(entries[0].contentRect.width) : null;
    }),
  };
}

/**
 * The index of the one item of the random content that is shown, or -1 while it shows none or several.
 *
 * @param {Page} page
 */
function shownRandomItem(page) {
  return page.getByTestId('random-content').evaluate(el => {
    const items = [...el.querySelectorAll(':scope > [data-item]')];
    const shown = items.filter(item => !(/** @type {HTMLElement} */ (item).hidden));
    return shown.length === 1 ? items.indexOf(shown[0]) : -1;
  });
}

/**
 * The playback state of the native video element inside wa-video.
 *
 * @param {Page} page
 */
function nativeVideo(page) {
  return page.getByTestId('video').evaluate(el => {
    const media = /** @type {HTMLVideoElement | null | undefined} */ (el.shadowRoot?.querySelector('video'));
    return media
      ? { paused: media.paused, currentTime: media.currentTime, volume: media.volume, muted: media.muted, playbackRate: media.playbackRate }
      : null;
  });
}

/**
 * A property of an element.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} name
 */
function propertyOf(page, testId, name) {
  return page.getByTestId(testId).evaluate((el, property) => /** @type {any} */ (el)[property], name);
}

/**
 * The phrase a wa-relative-time shows.
 *
 * @param {Page} page
 */
function relativePhrase(page) {
  return page.getByTestId('relative-time').evaluate(el => (el.shadowRoot?.querySelector('time')?.textContent ?? '').trim());
}

/**
 * Waits until a first-render call of the harness has completed, and expects it to have succeeded.
 *
 * @param {Page} page
 * @param {string} testId
 */
async function expectEarlyOutcome(page, testId) {
  const output = page.getByTestId(testId);
  await expect(output, `${testId} was called`).not.toHaveText('not called', { timeout: WA_READY_TIMEOUT_MS });
  await expect(output, `${testId} completed without an error`).toHaveText('done');
}

/**
 * Delays the element modules of the first-render case, so its calls certainly come before the definitions.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['zoomable-frame', 'animated-image']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'media-content',

  cases: [
    {
      name: 'WaAnimatedImage plays and pauses',
      methods: ['WaAnimatedImage.PlayAsync', 'WaAnimatedImage.PauseAsync'],
      tags: [ANIMATED_IMAGE],
      run: async page => {
        const image = page.getByTestId('animated-image');
        const pressed = () => image.evaluate(el => el.shadowRoot?.querySelector('[role="button"]')?.getAttribute('aria-pressed'));
        await whenInteractive(page, 'animated-image-play');
        // the element declares play without a default, so a new image reads undefined (not playing)
        expect(await propertyOf(page, 'animated-image', 'play'), 'a new image does not play').toBeFalsy();
        await expect(image).not.toHaveAttribute('play');
        await expect.poll(pressed).toBe('false');

        await callOk(page, 'animated-image-play');
        await expect.poll(() => propertyOf(page, 'animated-image', 'play')).toBe(true);
        await expect(image, 'play reflects to its attribute').toHaveAttribute('play', '');
        await expect.poll(pressed).toBe('true');

        await callOk(page, 'animated-image-pause');
        await expect.poll(() => propertyOf(page, 'animated-image', 'play')).toBe(false);
        await expect(image).not.toHaveAttribute('play');
        await expect.poll(pressed).toBe('false');
      },
    },
    {
      name: 'WaAnimation takes keyframes, sets and returns its current time, finishes and cancels',
      methods: ['WaAnimation.SetKeyframesAsync', 'WaAnimation.SetCurrentTimeAsync', 'WaAnimation.GetCurrentTimeAsync',
        'WaAnimation.FinishAsync', 'WaAnimation.CancelAsync'],
      tags: [ANIMATION],
      run: async page => {
        const target = page.getByTestId('animation-target');
        const opacity = () => target.evaluate(el => Number(getComputedStyle(el).opacity));
        const animations = () => target.evaluate(el => el.getAnimations().length);

        // no built-in name and no keyframes: there is no animation yet
        expect(await callOk(page, 'animation-get-time')).toBe(0);
        expect(await animations()).toBe(0);

        // a paused fade out from opacity 1 to 0
        await callOk(page, 'animation-set-keyframes');
        await expect.poll(animations, { message: 'the keyframes create the animation' }).toBe(1);
        expect(await propertyOf(page, 'animation', 'keyframes')).toEqual([{ opacity: 1 }, { opacity: 0 }]);
        expect(await opacity()).toBe(1);

        // half of the linear fade
        await callOk(page, 'animation-set-time');
        await expect.poll(opacity).toBeCloseTo(0.5, 2);
        expect(await callOk(page, 'animation-get-time')).toBeCloseTo(HALF_ANIMATION_TIME_MS, 0);

        await callOk(page, 'animation-finish');
        await expect(page.getByTestId('animation-finish-count'), 'OnFinish reached .NET').toHaveText('1');
        expect(await callOk(page, 'animation-get-time'), 'the end of the single iteration').toBeCloseTo(ANIMATION_DURATION_MS, 0);

        await callOk(page, 'animation-cancel');
        await expect(page.getByTestId('animation-cancel-count'), 'OnCancel reached .NET').toHaveText('1');
        await expect.poll(animations, { message: 'the cancelled animation leaves the target' }).toBe(0);
        expect(await callOk(page, 'animation-get-time')).toBe(0);
        expect(await opacity()).toBe(1);
      },
    },
    {
      name: 'WaZoomableFrame zooms in and out, sets and resets the zoom',
      methods: ['WaZoomableFrame.ZoomInAsync', 'WaZoomableFrame.ZoomOutAsync', 'WaZoomableFrame.SetZoomAsync', 'WaZoomableFrame.ResetAsync'],
      tags: [ZOOMABLE_FRAME],
      run: async page => {
        const zoom = () => propertyOf(page, 'zoomable-frame', 'zoom');
        const label = () => page.getByTestId('zoomable-frame').evaluate(el => (el.shadowRoot?.querySelector('[part="controls"] span')?.textContent ?? '').replace(/\s/g, ''));
        await whenInteractive(page, 'zoom-in');
        expect(await zoom()).toBe(1);

        // the default zoom levels: 25% to 200% in steps of 25%
        await callOk(page, 'zoom-in');
        await expect.poll(zoom).toBe(1.25);
        await expect.poll(label).toBe('125%');
        await callOk(page, 'zoom-out');
        await expect.poll(zoom).toBe(1);
        await callOk(page, 'zoom-out');
        await expect.poll(zoom).toBe(0.75);
        await expect.poll(label).toBe('75%');

        await callOk(page, 'zoom-set');
        await expect.poll(zoom).toBe(1.5);
        await expect.poll(label).toBe('150%');
        await callOk(page, 'zoom-reset');
        await expect.poll(zoom).toBe(1);
        await expect.poll(label).toBe('100%');
      },
    },
    {
      // real clicks: playback needs the page's user activation
      name: 'WaVideo plays, pauses, toggles, seeks, sets the volume and the rate, mutes and returns its state',
      methods: ['WaVideo.PlayAsync', 'WaVideo.PauseAsync', 'WaVideo.TogglePlayAsync', 'WaVideo.ToggleMuteAsync', 'WaVideo.SeekAsync',
        'WaVideo.SetVolumeAsync', 'WaVideo.SetPlaybackRateAsync', 'WaVideo.GetStateAsync'],
      tags: [VIDEO],
      pro: true,
      run: async page => {
        // the element pauses itself when less than a fifth of it is in view
        await whenInteractive(page, 'video-play');
        await page.getByTestId('video').scrollIntoViewIfNeeded();
        await expect.poll(() => propertyOf(page, 'video', 'duration'), { message: 'the clip metadata is loaded', timeout: WA_READY_TIMEOUT_MS })
          .toBeCloseTo(CLIP_SECONDS, 1);

        const initial = await callOk(page, 'video-get-state');
        expect(initial).toMatchObject({ playing: false, volume: 1, muted: false, playbackRate: 1 });
        expect(initial.currentTime).toBe(0);
        expect(initial.duration).toBeCloseTo(CLIP_SECONDS, 1);

        await callOk(page, 'video-play');
        await expect.poll(() => propertyOf(page, 'video', 'playing')).toBe(true);
        expect((await nativeVideo(page))?.paused).toBe(false);
        await callOk(page, 'video-pause');
        await expect.poll(() => propertyOf(page, 'video', 'playing')).toBe(false);
        expect((await nativeVideo(page))?.paused).toBe(true);

        await callOk(page, 'video-toggle-play');
        await expect.poll(() => propertyOf(page, 'video', 'playing')).toBe(true);
        await callOk(page, 'video-toggle-play');
        await expect.poll(() => propertyOf(page, 'video', 'playing')).toBe(false);

        await callOk(page, 'video-seek');
        await expect.poll(async () => (await nativeVideo(page))?.currentTime).toBeCloseTo(SEEK_SECONDS, 1);

        await callOk(page, 'video-set-volume');
        await expect.poll(async () => (await nativeVideo(page))?.volume).toBe(HALF_VOLUME);

        await callOk(page, 'video-set-rate');
        await expect.poll(async () => (await nativeVideo(page))?.playbackRate).toBe(SNAPPED_PLAYBACK_RATE);

        await callOk(page, 'video-toggle-mute');
        await expect.poll(async () => (await nativeVideo(page))?.muted).toBe(true);
        expect(await propertyOf(page, 'video', 'muted')).toBe(true);
        await callOk(page, 'video-toggle-mute');
        await expect.poll(async () => (await nativeVideo(page))?.muted).toBe(false);

        const state = await callOk(page, 'video-get-state');
        expect(state).toMatchObject({ playing: false, volume: HALF_VOLUME, muted: false, playbackRate: SNAPPED_PLAYBACK_RATE });
        expect(state.currentTime).toBeCloseTo(SEEK_SECONDS, 1);
        expect(state.duration).toBeCloseTo(CLIP_SECONDS, 1);
      },
    },
    {
      // WAB-64 F11: requestFullscreen needs a user activation; the real click gives it, also over a Blazor Server
      // round trip (the activation lasts a few seconds)
      name: 'WaVideo enters and exits fullscreen',
      methods: ['WaVideo.RequestFullscreenAsync', 'WaVideo.ExitFullscreenAsync'],
      tags: [VIDEO],
      pro: true,
      run: async page => {
        const video = page.getByTestId('video');
        const isFullscreen = () => video.evaluate(el => document.fullscreenElement === el);
        await whenInteractive(page, 'video-fullscreen');
        await video.scrollIntoViewIfNeeded();
        expect(await isFullscreen()).toBe(false);

        await callOk(page, 'video-fullscreen');
        await expect.poll(isFullscreen, { message: 'the video is the fullscreen element' }).toBe(true);
        await expect(video).toHaveAttribute('fullscreen', '');

        await callOk(page, 'video-exit-fullscreen', QUIET);
        await expect.poll(isFullscreen).toBe(false);
        await expect(video).not.toHaveAttribute('fullscreen');
      },
    },
    {
      name: 'WaVideoPlaylist moves to the next, the previous and a given video',
      methods: ['WaVideoPlaylist.NextAsync', 'WaVideoPlaylist.PreviousAsync', 'WaVideoPlaylist.GoToAsync'],
      tags: [PLAYLIST, VIDEO],
      pro: true,
      run: async page => {
        // the active index and the one video the playlist shows
        const active = () => page.getByTestId('playlist').evaluate(el => {
          const videos = [...el.querySelectorAll(':scope > wa-video')];
          const shown = videos.filter(v => !(/** @type {HTMLElement} */ (v).hidden));
          return { index: /** @type {any} */ (el).activeIndex, shown: shown.length === 1 ? videos.indexOf(shown[0]) : -1, count: videos.length };
        });
        await whenInteractive(page, 'playlist-next');
        await expect.poll(active, { timeout: WA_READY_TIMEOUT_MS }).toEqual({ index: 0, shown: 0, count: PLAYLIST_LENGTH });

        await callOk(page, 'playlist-next');
        await expect.poll(active).toEqual({ index: 1, shown: 1, count: PLAYLIST_LENGTH });
        await callOk(page, 'playlist-previous');
        await expect.poll(active).toEqual({ index: 0, shown: 0, count: PLAYLIST_LENGTH });
        await callOk(page, 'playlist-go-to-last');
        await expect.poll(active).toEqual({ index: LAST_PLAYLIST_INDEX, shown: LAST_PLAYLIST_INDEX, count: PLAYLIST_LENGTH });

        // the last video has no next one: the call completes and changes nothing
        await callOk(page, 'playlist-next');
        await expect.poll(active).toEqual({ index: LAST_PLAYLIST_INDEX, shown: LAST_PLAYLIST_INDEX, count: PLAYLIST_LENGTH });
      },
    },
    {
      name: 'WaMarkdown renders its changed source on request',
      methods: ['WaMarkdown.RenderMarkdownAsync'],
      tags: [MARKDOWN],
      run: async page => {
        const markdown = page.getByTestId('markdown');
        await whenInteractive(page, 'markdown-render');
        await expect(markdown.locator('h1')).toHaveText('Initial heading');

        // the new source reaches the script child, but the element does not watch it
        await page.getByTestId('markdown-change-source').click();
        await expect.poll(() => markdown.evaluate(el => el.querySelector('script[type="text/markdown"]')?.textContent)).toBe('# Changed heading');
        await expect(markdown.locator('h1'), 'the old source stays rendered until the call').toHaveText('Initial heading');

        await callOk(page, 'markdown-render');
        await expect(markdown.locator('h1')).toHaveText('Changed heading');
        await expect(markdown.locator('h1')).toHaveCount(1);
      },
    },
    {
      name: 'WaMutationObserver stops and restarts reporting changes',
      methods: ['WaMutationObserver.DisconnectAsync', 'WaMutationObserver.ReconnectAsync'],
      tags: [MUTATION_OBSERVER],
      run: async page => {
        await whenInteractive(page, 'mutation-disconnect');
        const events = await recordEvents(page, 'mutation-observer', 'wa-mutation');
        const setState = (/** @type {string} */ value) => page.getByTestId('mutation-target').evaluate((el, v) => el.setAttribute('data-state', v), value);

        await setState('connected');
        await expect.poll(events.count, { message: 'a connected observer reports the change' }).toBe(1);

        await callOk(page, 'mutation-disconnect');
        await setState('disconnected');
        await nextFrames(page);
        expect(await events.count(), 'a disconnected observer reports nothing').toBe(1);

        await callOk(page, 'mutation-reconnect');
        await setState('reconnected');
        await expect.poll(events.count, { message: 'the reconnected observer reports the change' }).toBe(2);
      },
    },
    {
      name: 'WaResizeObserver stops and restarts reporting size changes',
      methods: ['WaResizeObserver.DisconnectAsync', 'WaResizeObserver.ReconnectAsync'],
      tags: [RESIZE_OBSERVER],
      run: async page => {
        await whenInteractive(page, 'resize-disconnect');
        const events = await recordEvents(page, 'resize-observer', 'wa-resize');
        const setWidth = (/** @type {number} */ width) => page.getByTestId('resize-target').evaluate((el, w) => { /** @type {HTMLElement} */ (el).style.width = `${w}px`; }, width);

        await setWidth(CONNECTED_WIDTH);
        await expect.poll(events.lastWidth, { message: 'a connected observer reports the new size' }).toBe(CONNECTED_WIDTH);
        const connected = await events.count();

        await callOk(page, 'resize-disconnect');
        await setWidth(DISCONNECTED_WIDTH);
        await nextFrames(page);
        expect(await events.count(), 'a disconnected observer reports nothing').toBe(connected);

        // observing again reports the current size at once
        await callOk(page, 'resize-reconnect');
        await expect.poll(events.lastWidth, { message: 'the reconnected observer reports the size changed meanwhile' }).toBe(DISCONNECTED_WIDTH);
        expect(await events.count()).toBeGreaterThan(connected);
      },
    },
    {
      name: 'WaRandomContent.RandomizeAsync shows the next item in sequence mode',
      methods: ['WaRandomContent.RandomizeAsync'],
      tags: [RANDOM_CONTENT],
      run: async page => {
        await whenInteractive(page, 'random-randomize');
        await expect.poll(() => shownRandomItem(page), { message: 'the element shows one item' }).not.toBe(-1);
        const first = await shownRandomItem(page);

        await callOk(page, 'random-randomize');
        await expect.poll(() => shownRandomItem(page)).toBe((first + 1) % RANDOM_ITEMS);
        await callOk(page, 'random-randomize');
        await expect.poll(() => shownRandomItem(page)).toBe((first + 2) % RANDOM_ITEMS);
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaZoomableFrame.ZoomInAsync', 'WaAnimatedImage.PlayAsync'],
      route: EARLY_ROUTE,
      tags: [ZOOMABLE_FRAME, ANIMATED_IMAGE],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-zoom-in', 'early-animated-image-play']) await expectEarlyOutcome(page, id);
        await expect.poll(() => propertyOf(page, 'zoomable-frame', 'zoom'), { message: 'the frame zoomed in one level' }).toBe(1.25);
        await expect.poll(() => propertyOf(page, 'animated-image', 'play'), { message: 'the image plays' }).toBe(true);
        await expect(page.getByTestId('animated-image')).toHaveAttribute('play', '');
      },
    },
  ],

  knownDefects: [
    {
      // CopyAsync calls the native click() on the host, while the copy handler listens on an element inside the shadow
      // root, so nothing is copied; the real click gives the clipboard write its user activation (a real click on the
      // element itself copies the value with the same permissions)
      name: 'WaCopyButton.CopyAsync copies the value and reports it',
      defect: 'WAB-64 F2',
      methods: ['WaCopyButton.CopyAsync'],
      exemption: 'media-content-copy-button-copy',
      tags: [COPY_BUTTON],
      clipboard: true,
      run: async page => {
        await page.evaluate(() => navigator.clipboard.writeText('nothing copied yet'));

        await callOk(page, 'copy-button-copy');
        await expect.poll(() => page.evaluate(() => navigator.clipboard.readText()), { message: 'the value is on the clipboard' }).toBe(COPIED_VALUE);
        await expect(page.getByTestId('copy-count'), 'OnCopy reached .NET').toHaveText('1');
      },
    },
    {
      // UpdateAsync calls the Lit lifecycle update() directly, which skips willUpdate, where the element computes its
      // phrase: the element renders the old phrase again
      name: 'WaRelativeTime.UpdateAsync shows the phrase for the current time',
      defect: 'WAB-64 F4',
      methods: ['WaRelativeTime.UpdateAsync'],
      exemption: 'media-content-relative-time-update',
      tags: [RELATIVE_TIME],
      setup: installPageClock,
      run: async page => {
        // the first interactive render of a prerendered page replaces the element, which then computes its phrase anew
        await whenInteractive(page, 'relative-update');
        await expect.poll(() => relativePhrase(page)).toBe(INITIAL_PHRASE);

        // an hour later (no timer fires, and the element does not sync)
        await page.clock.setSystemTime(Date.now() + CLOCK_SHIFT_MS);
        await callOk(page, 'relative-update');
        await expect.poll(() => relativePhrase(page)).toBe(SHIFTED_PHRASE);
      },
    },
  ],

  external: [],
};
