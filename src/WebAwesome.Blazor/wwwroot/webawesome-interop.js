/**
 * Web Awesome Blazor JavaScript Interop Module
 * Provides JavaScript functionality for Web Awesome Blazor components
 */

/**
 * Sets a custom validation message on a Web Awesome form control element
 * @param {HTMLElement} element - The Web Awesome form control element
 * @param {string} message - The validation message to display, or empty string to clear
 */
export function setCustomValidity(element, message) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    // Check if element implements WebAwesomeFormControl interface by testing for setCustomValidity method
    if (typeof element.setCustomValidity !== 'function') {
        throw new Error(`Element ${tagName} does not implement WebAwesomeFormControl interface. Ensure Web Awesome library is properly loaded.`);
    }

    try {
        element.setCustomValidity(message || '');
    } catch (error) {
        throw new Error(`Failed to set custom validity on ${tagName}: ${error.message}`);
    }
}