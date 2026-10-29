using Unity.Collections;
using UnityEngine;

/// <summary>
/// Texture2D.LoadImage ignores the EXIF orientation of JPEGs, so photos taken on a phone in portrait
/// come out rotated by 90°. Reads the orientation tag and turns the decoded pixels accordingly.
/// </summary>
public static class ImageOrientationUtils {

    const ushort OrientationTag = 0x0112;

    /// <summary>
    /// Applies the EXIF orientation of the file to the texture decoded from it. The texture is changed in place,
    /// so references to it (e.g. a reused preview texture) stay valid.
    /// </summary>
    public static void ApplyExifOrientation(Texture2D texture, byte[] fileData) {
        if (texture == null) return;

        int orientation = GetJpegOrientation(fileData);
        if (orientation <= 1 || orientation > 8) return;

        Reorient(texture, orientation);
    }

    /// <summary>EXIF orientation 1–8, or 0 if the data isn't a JPEG or has no orientation tag.</summary>
    public static int GetJpegOrientation(byte[] data) {
        if (data == null || data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8) return 0;

        int pos = 2;
        while (pos + 4 <= data.Length) {
            if (data[pos] != 0xFF) return 0;
            byte marker = data[pos + 1];
            if (marker == 0xD9 || marker == 0xDA) return 0; // End of image / start of scan, no more metadata

            int segmentLength = (data[pos + 2] << 8) | data[pos + 3];
            if (segmentLength < 2) return 0;

            int segmentStart = pos + 4;
            int segmentEnd = pos + 2 + segmentLength;
            if (segmentEnd > data.Length) return 0;

            // APP1 segment starting with "Exif\0\0"
            if (marker == 0xE1 && segmentLength >= 8
                && data[segmentStart] == 'E' && data[segmentStart + 1] == 'x' && data[segmentStart + 2] == 'i' && data[segmentStart + 3] == 'f') {
                return ReadTiffOrientation(data, segmentStart + 6, segmentEnd);
            }

            pos = segmentEnd;
        }
        return 0;
    }

    static int ReadTiffOrientation(byte[] data, int tiffStart, int end) {
        if (tiffStart + 8 > end) return 0;

        bool littleEndian;
        if (data[tiffStart] == 'I' && data[tiffStart + 1] == 'I') littleEndian = true;
        else if (data[tiffStart] == 'M' && data[tiffStart + 1] == 'M') littleEndian = false;
        else return 0;

        int ifdStart = tiffStart + (int)ReadUInt32(data, tiffStart + 4, littleEndian);
        if (ifdStart + 2 > end) return 0;

        int entryCount = ReadUInt16(data, ifdStart, littleEndian);
        for (int i = 0; i < entryCount; i++) {
            int entry = ifdStart + 2 + i * 12;
            if (entry + 12 > end) return 0;

            if (ReadUInt16(data, entry, littleEndian) == OrientationTag) {
                return ReadUInt16(data, entry + 8, littleEndian); // SHORT value is stored inline
            }
        }
        return 0;
    }

    static ushort ReadUInt16(byte[] data, int offset, bool littleEndian) {
        return littleEndian
            ? (ushort)(data[offset] | (data[offset + 1] << 8))
            : (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    static uint ReadUInt32(byte[] data, int offset, bool littleEndian) {
        return littleEndian
            ? (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24))
            : (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
    }

    // Works on native buffers, so the only extra memory is one copy of the top mip level
    static void Reorient(Texture2D texture, int orientation) {
        int w = texture.width;
        int h = texture.height;
        bool swapsSides = orientation >= 5;
        int dw = swapsSides ? h : w;
        int dh = swapsSides ? w : h;

        NativeArray<byte> pixels = texture.GetPixelData<byte>(0);
        int bytesPerPixel = pixels.Length / (w * h);
        if (bytesPerPixel < 1 || pixels.Length != w * h * bytesPerPixel) return;

        var src = new NativeArray<byte>(pixels, Allocator.Persistent);
        texture.Reinitialize(dw, dh, texture.format, texture.mipmapCount > 1);
        CopyReoriented(src, w, h, bytesPerPixel, orientation, texture.GetPixelData<byte>(0));

        src.Dispose();
        texture.Apply(updateMipmaps: true);
    }

    /// <summary>
    /// Returns a copy of the texture turned by 90° clockwise, or null if its pixels can't be read.
    /// The source is left unchanged.
    /// </summary>
    public static Texture2D Rotated90Clockwise(Texture2D source) {
        if (source == null || !source.isReadable) return null;

        int w = source.width;
        int h = source.height;
        NativeArray<byte> pixels = source.GetPixelData<byte>(0);
        int bytesPerPixel = pixels.Length / (w * h);
        if (bytesPerPixel < 1 || pixels.Length != w * h * bytesPerPixel) return null;

        var rotated = new Texture2D(h, w, source.format, source.mipmapCount > 1);
        CopyReoriented(pixels, w, h, bytesPerPixel, 6, rotated.GetPixelData<byte>(0));
        rotated.Apply(updateMipmaps: true);
        return rotated;
    }

    // Writes the w × h source pixels into dst turned by the EXIF orientation (dst has swapped sides for 5–8)
    static void CopyReoriented(NativeArray<byte> src, int w, int h, int bytesPerPixel, int orientation, NativeArray<byte> dst) {
        bool swapsSides = orientation >= 5;
        int dw = swapsSides ? h : w;
        int dh = swapsSides ? w : h;

        // EXIF works with rows from the top, Unity stores them from the bottom
        for (int dy = 0; dy < dh; dy++) {
            int dstRow = (dh - 1 - dy) * dw;
            for (int dx = 0; dx < dw; dx++) {
                int sx, sy;
                switch (orientation) {
                    case 2: sx = w - 1 - dx; sy = dy; break;              // Mirrored horizontally
                    case 3: sx = w - 1 - dx; sy = h - 1 - dy; break;      // Rotated 180°
                    case 4: sx = dx; sy = h - 1 - dy; break;              // Mirrored vertically
                    case 5: sx = dy; sy = dx; break;                      // Transposed
                    case 6: sx = dy; sy = h - 1 - dx; break;              // Needs 90° clockwise
                    case 7: sx = w - 1 - dy; sy = h - 1 - dx; break;      // Transversed
                    default: sx = w - 1 - dy; sy = dx; break;             // 8: needs 90° counter-clockwise
                }

                int srcIndex = ((h - 1 - sy) * w + sx) * bytesPerPixel;
                int dstIndex = (dstRow + dx) * bytesPerPixel;
                for (int b = 0; b < bytesPerPixel; b++) {
                    dst[dstIndex + b] = src[srcIndex + b];
                }
            }
        }
    }
}
