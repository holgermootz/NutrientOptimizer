// Browser localStorage wrapper functions for Blazor interop

export function setLocalStorage(key, value) {
    try {
        localStorage.setItem(key, value);
        console.log(`[LocalStorage] Saved ${key}`);
        return true;
    } catch (error) {
        console.error(`[LocalStorage] Error saving ${key}:`, error);
        return false;
    }
}

export function getLocalStorage(key) {
    try {
        const value = localStorage.getItem(key);
        if (value) {
            console.log(`[LocalStorage] Loaded ${key}, length: ${value.length}`);
            return value;
        } else {
            console.log(`[LocalStorage] Key ${key} not found, returning null`);
            return null;
        }
    } catch (error) {
        console.error(`[LocalStorage] Error loading ${key}:`, error);
        return null;
    }
}

export function removeLocalStorage(key) {
    try {
        localStorage.removeItem(key);
        console.log(`[LocalStorage] Removed ${key}`);
        return true;
    } catch (error) {
        console.error(`[LocalStorage] Error removing ${key}:`, error);
        return false;
    }
}

export function clearLocalStorage() {
    try {
        localStorage.clear();
        console.log(`[LocalStorage] Cleared all storage`);
        return true;
    } catch (error) {
        console.error(`[LocalStorage] Error clearing storage:`, error);
        return false;
    }
}
