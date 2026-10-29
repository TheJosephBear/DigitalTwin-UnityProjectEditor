using Unity.Collections;
using UnityEngine;

/// <summary>
/// Texture2D.LoadImage ignores the EXIF orientation of JPEGs, so photos taken on a phone in portrait
/// come out rotated by 90°. Reads the orientation tag and turns the decoded pixels accordingly.
/// Also turns JPEGs by rewriting the tag, without re-encoding them.
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
        int valuePos = FindJpegOrientation(data, out bool littleEndian, out _, out _);
        return valuePos >= 0 ? ReadUInt16(data, valuePos, littleEndian) : 0;
    }

    // Orientation after turning the shown picture by another 90° clockwise, indexed by the current one (0 = none, as 1)
    static readonly int[] NextClockwise = { 6, 6, 7, 8, 5, 2, 3, 4, 1 };

    /// <summary>
    /// JPEG turned by 90° clockwise by changing only its EXIF orientation, so the picture isn't re-encoded and
    /// loses no quality. Four turns give back the original file. Returns null if the data isn't a JPEG,
    /// or its EXIF has no orientation tag (adding one there would move the other EXIF data).
    /// </summary>
    public static byte[] RotateJpeg90ClockwiseByExif(byte[] data) {
        if (!IsJpeg(data)) return null;

        int valuePos = FindJpegOrientation(data, out bool littleEndian, out int exifStart, out int exifEnd);
        if (valuePos < 0) {
            if (exifStart >= 0) return null;
            return Insert(data, MetadataInsertPosition(data), MinimalExifSegment(NextClockwise[1]));
        }

        int current = ReadUInt16(data, valuePos, littleEndian);
        if (current < 1 || current > 8) current = 1;
        int next = NextClockwise[current];

        // The segment added by an earlier turn goes away once the picture is upright again
        if (next == 1 && SegmentEquals(data, exifStart, exifEnd, MinimalExifSegment(current))) {
            return Remove(data, exifStart, exifEnd);
        }

        byte[] result = (byte[])data.Clone();
        WriteUInt16(result, valuePos, (ushort)next, littleEndian);
        return result;
    }

    static bool IsJpeg(byte[] data) {
        return data != null && data.Length >= 4 && data[0] == 0xFF && data[1] == 0xD8;
    }

    // Position of the orientation value, or -1. exifStart / exifEnd: the EXIF segment (from its marker), -1 without one
    static int FindJpegOrientation(byte[] data, out bool littleEndian, out int exifStart, out int exifEnd) {
        littleEndian = false;
        exifStart = -1;
        exifEnd = -1;
        if (!IsJpeg(data)) return -1;

        int pos = 2;
        while (pos + 4 <= data.Length) {
            if (data[pos] != 0xFF) return -1;
            byte marker = data[pos + 1];
            if (marker == 0xD9 || marker == 0xDA) return -1; // End of image / start of scan, no more metadata

            int segmentLength = (data[pos + 2] << 8) | data[pos + 3];
            if (segmentLength < 2) return -1;

            int segmentStart = pos + 4;
            int segmentEnd = pos + 2 + segmentLength;
            if (segmentEnd > data.Length) return -1;

            // APP1 segment starting with "Exif\0\0"
            if (marker == 0xE1 && segmentLength >= 8
                && data[segmentStart] == 'E' && data[segmentStart + 1] == 'x' && data[segmentStart + 2] == 'i' && data[segmentStart + 3] == 'f') {
                exifStart = pos;
                exifEnd = segmentEnd;
                return FindTiffOrientation(data, segmentStart + 6, segmentEnd, out littleEndian);
            }

            pos = segmentEnd;
        }
        return -1;
    }

    static int FindTiffOrientation(byte[] data, int tiffStart, int end, out bool littleEndian) {
        littleEndian = false;
        if (tiffStart + 8 > end) return -1;

        if (data[tiffStart] == 'I' && data[tiffStart + 1] == 'I') littleEndian = true;
        else if (data[tiffStart] == 'M' && data[tiffStart + 1] == 'M') littleEndian = false;
        else return -1;

        long ifdStart = tiffStart + (long)ReadUInt32(data, tiffStart + 4, littleEndian);
        if (ifdStart + 2 > end) return -1;

        int entryCount = ReadUInt16(data, (int)ifdStart, littleEndian);
        for (int i = 0; i < entryCount; i++) {
            int entry = (int)ifdStart + 2 + i * 12;
            if (entry + 12 > end) return -1;

            // SHORT (type 3), stored inline in the entry
            if (ReadUInt16(data, entry, littleEndian) == OrientationTag && ReadUInt16(data, entry + 2, littleEndian) == 3) {
                return entry + 8;
            }
        }
        return -1;
    }

    // APP1 with an EXIF block holding only the orientation (big endian TIFF, one IFD entry)
    static byte[] MinimalExifSegment(int orientation) {
        return new byte[] {
            0xFF, 0xE1, 0x00, 0x22,                         // APP1, length 34
            (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0,
            (byte)'M', (byte)'M', 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08,  // TIFF header, IFD at 8
            0x00, 0x01,                                     // One entry
            0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, // Orientation, SHORT, count 1
            0x00, (byte)orientation, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00                          // No next IFD
        };
    }

    // After the start of image, or after the JFIF header, which has to stay first
    static int MetadataInsertPosition(byte[] data) {
        if (data.Length >= 6 && data[2] == 0xFF && data[3] == 0xE0) {
            int end = 4 + ((data[4] << 8) | data[5]);
            if (end <= data.Length) return end;
        }
        return 2;
    }

    static bool SegmentEquals(byte[] data, int start, int end, byte[] segment) {
        if (end - start != segment.Length) return false;
        for (int i = 0; i < segment.Length; i++) {
            if (data[start + i] != segment[i]) return false;
        }
        return true;
    }

    static byte[] Insert(byte[] data, int position, byte[] bytes) {
        byte[] result = new byte[data.Length + bytes.Length];
        System.Array.Copy(data, 0, result, 0, position);
        System.Array.Copy(bytes, 0, result, position, bytes.Length);
        System.Array.Copy(data, position, result, position + bytes.Length, data.Length - position);
        return result;
    }

    static byte[] Remove(byte[] data, int start, int end) {
        byte[] result = new byte[data.Length - (end - start)];
        System.Array.Copy(data, 0, result, 0, start);
        System.Array.Copy(data, end, result, start, data.Length - end);
        return result;
    }

    static void WriteUInt16(byte[] data, int offset, ushort value, bool littleEndian) {
        data[offset + (littleEndian ? 0 : 1)] = (byte)value;
        data[offset + (littleEndian ? 1 : 0)] = (byte)(value >> 8);
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

    /// <summary>
    /// Turns the texture by 90° clockwise in place, so everything showing it shows the turned picture.
    /// Returns false if its pixels can't be read.
    /// </summary>
    public static bool Rotate90ClockwiseInPlace(Texture2D texture) {
        return texture != null && texture.isReadable && Reorient(texture, 6);
    }

    // Works on native buffers, so the only extra memory is one copy of the top mip level
    static bool Reorient(Texture2D texture, int orientation) {
        int w = texture.width;
        int h = texture.height;
        bool swapsSides = orientation >= 5;
        int dw = swapsSides ? h : w;
        int dh = swapsSides ? w : h;

        NativeArray<byte> pixels = texture.GetPixelData<byte>(0);
        int bytesPerPixel = pixels.Length / (w * h);
        if (bytesPerPixel < 1 || pixels.Length != w * h * bytesPerPixel) return false;

        var src = new NativeArray<byte>(pixels, Allocator.Persistent);
        texture.Reinitialize(dw, dh, texture.format, texture.mipmapCount > 1);
        CopyReoriented(src, w, h, bytesPerPixel, orientation, texture.GetPixelData<byte>(0));

        src.Dispose();
        texture.Apply(updateMipmaps: true);
        return true;
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
