"""Copies an OBJ and its MTL with the decimal commas changed to dots.

OBJ files exported on a PC set to a Czech (or other comma) locale can contain numbers like "v 181,1149 ...".
TriLib reads the comma as a thousands separator, so the model comes out about 10 000x too big and partly distorted.
This writes fixed copies and leaves the originals alone. Only number lines are touched (v/vt/vn/vp in the OBJ,
Ka/Kd/Ks/... in the MTL), and only commas between two digits, so names and paths stay as they are.

The upload tests use it for the Akagi model (Akagi_obj_fixed_png_textures in TestModelCatalog.cs):

    python Tools/fix_obj_decimal_commas.py "D:/Unity stuff/akagi mountain pass/texture_png/akagi mountain pass.obj" --out "D:/Unity stuff/akagi mountain pass/fixed_decimal_dots"
"""
import argparse
import os
import re
import sys

DIGIT_COMMA = re.compile(r"(?<=\d),(?=\d)")
OBJ_NUMBER_LINES = ("v ", "vt ", "vn ", "vp ")
MTL_NUMBER_LINES = ("Ka ", "Kd ", "Ks ", "Ke ", "Ns ", "Ni ", "d ", "Tr ", "Tf ", "illum ")


def fix(src, dst, prefixes):
    changed = lines = 0
    # surrogateescape keeps bytes that aren't UTF-8 (names from other code pages) exactly as they were
    with open(src, "r", encoding="utf-8", errors="surrogateescape", newline="") as fi, \
         open(dst, "w", encoding="utf-8", errors="surrogateescape", newline="") as fo:
        for line in fi:
            lines += 1
            if line.lstrip().startswith(prefixes) and "," in line:
                line = DIGIT_COMMA.sub(".", line)
                changed += 1
            fo.write(line)
    print(f"{dst}: {lines} lines, {changed} fixed")


def main():
    parser = argparse.ArgumentParser(description="Copy an OBJ (and its MTL) with decimal commas changed to dots.")
    parser.add_argument("obj", help="the OBJ file")
    parser.add_argument("--mtl", help="the MTL file (default: the OBJ's name with .mtl, if it exists)")
    parser.add_argument("--out", help="folder for the fixed copies (default: fixed_decimal_dots next to the OBJ)")
    args = parser.parse_args()

    obj = os.path.abspath(args.obj)
    if not os.path.isfile(obj):
        sys.exit(f"No such file: {obj}")

    mtl = args.mtl or os.path.splitext(obj)[0] + ".mtl"
    if args.mtl and not os.path.isfile(mtl):
        sys.exit(f"No such file: {mtl}")

    out = os.path.abspath(args.out or os.path.join(os.path.dirname(obj), "fixed_decimal_dots"))
    if out == os.path.dirname(obj):
        sys.exit("--out must be a different folder, the originals would be overwritten")
    os.makedirs(out, exist_ok=True)

    fix(obj, os.path.join(out, os.path.basename(obj)), OBJ_NUMBER_LINES)
    if os.path.isfile(mtl):
        fix(mtl, os.path.join(out, os.path.basename(mtl)), MTL_NUMBER_LINES)
    else:
        print("No MTL found, only the OBJ was copied")


if __name__ == "__main__":
    main()
