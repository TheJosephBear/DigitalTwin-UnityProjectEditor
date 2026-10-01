mergeInto(LibraryManager.library, {
    GetWasmHeapSizeMB: function() {
        // Size of the whole WASM heap. It only grows, so this is the peak so far.
        return HEAPU8.length / (1024 * 1024);
    }
});
