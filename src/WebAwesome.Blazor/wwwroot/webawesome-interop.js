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

/**
 * Invokes a method on a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} methodName - The name of the method to invoke
 * @param {Array} args - Arguments to pass to the method
 * @returns {any} The result of the method call
 */
export function invokeMethod(element, methodName, args) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!methodName) {
        throw new Error('Method name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    // Check if the method exists on the element
    if (typeof element[methodName] !== 'function') {
        throw new Error(`Method '${methodName}' does not exist on element ${tagName} or is not a function. Ensure Web Awesome library is properly loaded.`);
    }

    try {
        // Apply the method with the provided arguments
        const result = element[methodName].apply(element, args || []);
        return result;
    } catch (error) {
        throw new Error(`Failed to invoke method '${methodName}' on ${tagName}: ${error.message}`);
    }
}

/**
 * Sets a property value on a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} propertyName - The name of the property to set
 * @param {any} value - The value to set
 */
export function setProperty(element, propertyName, value) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!propertyName) {
        throw new Error('Property name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    try {
        element[propertyName] = value;
    } catch (error) {
        throw new Error(`Failed to set property '${propertyName}' on ${tagName}: ${error.message}`);
    }
}

/**
 * Gets a property value from a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} propertyName - The name of the property to get
 * @returns {any} The property value
 */
export function getProperty(element, propertyName) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!propertyName) {
        throw new Error('Property name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    try {
        return element[propertyName];
    } catch (error) {
        throw new Error(`Failed to get property '${propertyName}' from ${tagName}: ${error.message}`);
    }
}