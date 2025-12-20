/**
 * Preserves the state of list pages by saving their URLs in sessionStorage.
 */

/**
 * Saves the current URL in sessionStorage for a given list key.
 * @param {string} key - The key to identify the list (e.g., 'tours', 'schedules').
 */
function saveListState(key) {
    const stateKey = `list_state_${key}`;
    sessionStorage.setItem(stateKey, window.location.href);
}

/**
 * Navigates back to the preserved list state or a default URL.
 * @param {string} key - The key to identify the list.
 * @param {string} defaultUrl - The fallback URL if no state is saved.
 */
function goBackToList(key, defaultUrl) {
    const stateKey = `list_state_${key}`;
    const savedUrl = sessionStorage.getItem(stateKey);
    
    if (savedUrl) {
        window.location.href = savedUrl;
    } else {
        window.location.href = defaultUrl;
    }
}
