mergeInto(LibraryManager.library, {
    IsPointerLocked: function() {
        // The browser's real pointer lock state. Unity's Cursor.lockState keeps saying Locked after ESC released the lock.
        return document.pointerLockElement ? 1 : 0;
    }
});
