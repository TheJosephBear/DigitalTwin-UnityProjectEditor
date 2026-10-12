mergeInto(LibraryManager.library, {
    DownloadImage: function (arrayPtr, byteLength, fileNamePtr) {
        var bytes = new Uint8Array(HEAPU8.buffer, arrayPtr, byteLength);
        var blob = new Blob([bytes], { type: "image/png" });
        var fileName = UTF8ToString(fileNamePtr);

        var link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    }
});