// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, readableText } = require('./helpers/showcase');

// Model-driven carousel slides (/testing/carousel-slides): each carousel renders its WaCarouselItems from a list with
// @key, and the harness buttons add a slide, remove the active slide and remove the last one. Blazor adds and removes
// the elements, and Web Awesome picks the change up itself (its mutation observer re-initializes the slides and, when
// looping, its clones): the pagination shows one dot per slide, the active dot and OnSlideChange follow the
// navigation, and removing the active slide leaves a working carousel, without a Blazor error. Removing the active
// slide keeps the active index, so the slide after it shows; removing the active last slide shows the new last slide,
// or the first one when the carousel loops (reported through OnSlideChange). The second carousel loops, so Web Awesome
// keeps clones of the slides next to the ones Blazor renders.

const ROUTE = '/testing/carousel-slides';
const TAGS = ['wa-carousel', 'wa-carousel-item', 'wa-button'];

// the two harness carousels, with the slide that shows once the active last slide (index 4 of 5) is removed
const DECKS = [
  { key: 'plain', afterRemovingActiveLast: { index: 3, slide: 'Slide 4' } },
  { key: 'loop', afterRemovingActiveLast: { index: 0, slide: 'Slide 1' } },
];

/**
 * The parts of one harness carousel.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} key
 */
function deck(page, key) {
  const carousel = page.getByTestId(`cs-${key}`);
  return {
    carousel,
    // the slides Blazor rendered (Web Awesome's loop clones carry data-clone)
    slides: carousel.locator('wa-carousel-item:not([data-clone])'),
    dots: carousel.locator('[part~="pagination-item"]'),
    model: page.getByTestId(`cs-${key}-model`),
    active: page.getByTestId(`cs-${key}-active`),
    add: page.getByTestId(`cs-${key}-add`),
    removeActive: page.getByTestId(`cs-${key}-remove-active`),
    removeLast: page.getByTestId(`cs-${key}-remove-last`),
    next: carousel.locator('[part~="navigation-button-next"]'),
    previous: carousel.locator('[part~="navigation-button-previous"]'),
  };
}

/**
 * What the carousel shows: the index of the active pagination dot and the text of the slide inside the scroll
 * container.
 *
 * @param {ReturnType<typeof deck>} d
 */
function shown(d) {
  return d.carousel.evaluate(el => {
    const root = /** @type {ShadowRoot} */ (el.shadowRoot);
    const dot = [...root.querySelectorAll('[part~="pagination-item"]')]
      .findIndex(item => (item.getAttribute('part') ?? '').split(' ').includes('pagination-item-active'));
    const box = /** @type {Element} */ (root.querySelector('[part~="scroll-container"]')).getBoundingClientRect();
    const inView = [...el.querySelectorAll('wa-carousel-item')].filter(slide => {
      const rect = slide.getBoundingClientRect();
      return rect.left >= box.left - 1 && rect.right <= box.right + 1;
    });
    return { dot, slide: inView.map(slide => (slide.textContent ?? '').trim()).join(' | ') };
  });
}

/**
 * Asserts the slides the carousel holds, by their text, and one pagination dot per slide.
 *
 * @param {ReturnType<typeof deck>} d
 * @param {number[]} slides the slide numbers in order
 */
async function expectSlides(d, slides) {
  await expect(d.model).toHaveText(slides.join(','));
  await expect(d.slides).toHaveText(slides.map(n => `Slide ${n}`));
  await expect(d.dots, 'one pagination dot per slide').toHaveCount(slides.length);
}

/**
 * Asserts the active slide: the index OnSlideChange reported to .NET, the active dot and the slide in view.
 *
 * @param {ReturnType<typeof deck>} d
 * @param {number} index
 * @param {string} slide
 */
async function expectActive(d, index, slide) {
  await expect(d.active, `OnSlideChange reports ${index}`).toHaveText(String(index));
  await expect.poll(() => shown(d), `${slide} shows with dot ${index} active`).toEqual({ dot: index, slide });
}

for (const { key, afterRemovingActiveLast } of DECKS) {
  test(`carousel slides (${key}): slides added and removed through the model, the active and the last one included`, async ({ page }) => {
    const problems = await openShowcase(page, ROUTE, TAGS);
    const d = deck(page, key);

    await expectSlides(d, [1, 2, 3]);
    await expect.poll(() => shown(d)).toEqual({ dot: 0, slide: 'Slide 1' });

    // added slides get their dots and the navigation reaches them
    await d.add.click();
    await d.add.click();
    await expectSlides(d, [1, 2, 3, 4, 5]);
    for (let index = 1; index <= 4; index++) {
      await d.next.click();
      await expectActive(d, index, `Slide ${index + 1}`);
    }

    // removing the active slide that is the last one
    await d.removeActive.click();
    await expectSlides(d, [1, 2, 3, 4]);
    await expectActive(d, afterRemovingActiveLast.index, afterRemovingActiveLast.slide);

    // removing an active slide in the middle keeps the index: the slide after it shows
    await d.carousel.evaluate(el => /** @type {any} */ (el).goToSlide(1));
    await expectActive(d, 1, 'Slide 2');
    await d.removeActive.click();
    await expectSlides(d, [1, 3, 4]);
    await expect.poll(() => shown(d), 'the next slide took the place').toEqual({ dot: 1, slide: 'Slide 3' });
    await expect(d.active, 'the index did not change').toHaveText('1');

    // the navigation works across the new set, both ways
    await d.next.click();
    await expectActive(d, 2, 'Slide 4');
    await d.previous.click();
    await expectActive(d, 1, 'Slide 3');

    // removing the last slide while another is active, then adding one
    await d.removeLast.click();
    await expectSlides(d, [1, 3]);
    await expect.poll(() => shown(d), 'the active slide stays').toEqual({ dot: 1, slide: 'Slide 3' });
    await d.add.click();
    await expectSlides(d, [1, 3, 6]);
    await d.next.click();
    await expectActive(d, 2, 'Slide 6');
    expect(await readableText(d.slides.nth(2))).toBe('Slide 6');

    await expectHealthy(page, problems);
  });
}
