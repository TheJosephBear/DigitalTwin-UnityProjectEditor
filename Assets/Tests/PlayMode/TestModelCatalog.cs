using System.Collections.Generic;
using System.IO;

/// <summary>
/// The models the upload tests load. They live outside the project (they're too big for git), so a case whose files
/// are missing on this machine is skipped, not failed. Point the tests at your copies of the folders with
/// Tools > Model Upload Tests > Set Model Folders (see TestModelFolders).
/// </summary>
public static class TestModelCatalog {

    public static string DTModelsRoot => TestModelFolders.DTModels.Path;
    public static string AkagiRoot => TestModelFolders.Akagi.Path;

    /// <summary>
    /// One upload: the files a user would pick in the upload dialog. The first file is the main model.
    /// </summary>
    public class ModelCase {
        public string Name;
        public string Notes;
        public string Root;
        public string[] FilePatterns; // relative to Root, wildcards allowed in the file name ("texture_png/*.png")
        public bool ExpectTextures;   // the selection has textures the model uses, so at least one must end up on a material

        public List<string> ResolveFiles() {
            var files = new List<string>();
            foreach (string pattern in FilePatterns) {
                string fullPattern = Path.Combine(Root, pattern);
                string dir = Path.GetDirectoryName(fullPattern);
                string namePattern = Path.GetFileName(fullPattern);

                if (namePattern.Contains("*") || namePattern.Contains("?")) {
                    if (!Directory.Exists(dir)) continue;
                    string[] matches = Directory.GetFiles(dir, namePattern);
                    System.Array.Sort(matches, System.StringComparer.OrdinalIgnoreCase);
                    files.AddRange(matches);
                } else if (File.Exists(fullPattern)) {
                    files.Add(fullPattern);
                }
            }
            return files;
        }

        public bool MainFileExists => File.Exists(Path.Combine(Root, FilePatterns[0]));

        // Shown as the test name in the Test Runner
        public override string ToString() => Name;
    }

    static ModelCase DT(string name, string notes, bool expectTextures, params string[] files) =>
        new ModelCase { Name = name, Notes = notes, Root = DTModelsRoot, FilePatterns = files, ExpectTextures = expectTextures };

    static ModelCase Akagi(string name, string notes, bool expectTextures, params string[] files) =>
        new ModelCase { Name = name, Notes = notes, Root = AkagiRoot, FilePatterns = files, ExpectTextures = expectTextures };

    const string ServerProjects = @"from server\projects";

    /// <summary>
    /// Up to ~70 MB each. Run by default.
    /// </summary>
    public static IEnumerable<ModelCase> Standard() {
        yield return DT("Carpincho_obj_without_mtl", "Blender OBJ whose mtllib (Carpincho.mtl) isn't in the selection.", false,
            @"Carpincho\Carpincho.obj");
        yield return DT("Capybara_czech_file_name", "File name with Czech diacritics (č), no MTL.", false,
            @"čeština\Capybara09180č.obj");
        yield return DT("Usti_obj_sketchup", "SketchUp OBJ, its MTL ('RUR_Usti - základ.mtl', space + diacritics) is missing.", false,
            @"usti.obj");
        yield return DT("Usti_fbx", "FBX export of the same Ústí model.", false,
            @"usti.fbx");
        yield return DT("CEMMTECH_RGB_obj_mtl", "Small OBJ + MTL from a server project.", false,
            $@"{ServerProjects}\CEMMTECH_rgb\models\1e870477fa97af9247e7ad3f5774c178\CEMMTECH_RGB.obj",
            $@"{ServerProjects}\CEMMTECH_rgb\models\1e870477fa97af9247e7ad3f5774c178\CEMMTECH_RGB.mtl");
        yield return DT("CEMMTECH_RGB_stromy_obj_mtl", "OBJ + MTL with trees.", false,
            $@"{ServerProjects}\CEMMTECH_rgb_stromy\models\c02fc359c0f16e924d04a183d09d269b\CEMMTECH_RGB_stromy.obj",
            $@"{ServerProjects}\CEMMTECH_rgb_stromy\models\c02fc359c0f16e924d04a183d09d269b\CEMMTECH_RGB_stromy.mtl");
        yield return DT("MFC_stary_obj_mtl", "MFC, old version. Folder name with diacritics (starý).", false,
            @"MFC_testovani_obj\starý\MFC_stary.obj", @"MFC_testovani_obj\starý\MFC_stary.mtl");
        yield return DT("MFC_optimalizovany_obj_mtl", "MFC, optimized version, file name with a dash.", false,
            @"MFC_testovani_obj\optimalizovaný\MFC-optimalizovany.obj", @"MFC_testovani_obj\optimalizovaný\MFC-optimalizovany.mtl");
        yield return DT("MFC_explode_obj_mtl", "MFC, exploded into many objects.", false,
            @"MFC_testovani_obj\explode\mfc_explode\MFC_explode.obj", @"MFC_testovani_obj\explode\mfc_explode\MFC_explode.mtl");
        yield return DT("MFC_merged_obj_png", "MFC merged into one mesh, with one texture. File name 'material.mtl' differs from the OBJ.", true,
            @"MFC_testovani_obj\explode\merge obj\merged.obj", @"MFC_testovani_obj\explode\merge obj\material.mtl",
            @"MFC_testovani_obj\explode\merge obj\material_0.png");
        yield return DT("MFC_zip_three_variants", "ZIP with three OBJ + MTL pairs in subfolders, folder names with diacritics.", false,
            @"MFC_testovani_obj.zip");
        yield return DT("MFC_model_glb", "MFC as GLB.", false,
            @"MFC_testovani_obj\explode\model.glb");
        yield return DT("MFC_merged_glb", "MFC merged as GLB.", false,
            @"MFC_testovani_obj\explode\merged.glb");
        yield return DT("MFC_gltf_report_glb", "GLB optimized with glTF Report, palette texture.", false,
            @"MFC_testovani_obj\explode\optim gltf report\model-v2.glb", @"MFC_testovani_obj\explode\optim gltf report\PaletteBaseColor.jpg");
        yield return DT("Kampus_UJEP_v2_obj_mtl_png", "Textured SketchUp OBJ (soil texture).", true,
            @"obj mtl\lokalita_Kampus_UJEP_v2.obj", @"obj mtl\lokalita_Kampus_UJEP_v2.mtl", @"obj mtl\Soil_04_1K.png");
        yield return DT("PNG_nahled_obj_many_textures",
            "OBJ + MTL + 10 JPG/PNG textures with long generated names. The MTL has no map_Kd lines, so the textures go unused.", false,
            $@"{ServerProjects}\png náhled test\models\c7fb35122c26a323df2eeca7e04c2225\model.obj",
            $@"{ServerProjects}\png náhled test\models\c7fb35122c26a323df2eeca7e04c2225\model.mtl",
            $@"{ServerProjects}\png náhled test\models\c7fb35122c26a323df2eeca7e04c2225\*.png",
            $@"{ServerProjects}\png náhled test\models\c7fb35122c26a323df2eeca7e04c2225\*.jp*g");
        yield return DT("Usti_Bilina_glb", "Photogrammetry GLB with an embedded texture.", true,
            @"usti_nad_labem_bilina\model.glb");
        yield return DT("Kampus_glb_castecne", "Kampus GLB, partly split into objects.", false,
            @"kampus glb test\castecne\kampus_castecne.glb");
        yield return DT("Litvinov_glb_draco_etc1s", "GLB with Draco meshes and KTX2 (ETC1S) textures.", true,
            @"Litvínov\Litvinov_Vystupy\optim_draco_etc1s.glb");
    }

    /// <summary>
    /// Big models (70 MB and more). Marked [Explicit]: run them on purpose from the Test Runner.
    /// </summary>
    public static IEnumerable<ModelCase> Heavy() {
        yield return DT("Kampus_glb_explode", "Kampus GLB, split into many objects.", false,
            @"kampus glb test\explode\kampus_explode.glb");
        yield return DT("Kampus_glb_vsude", "Kampus GLB, split everywhere.", false,
            @"kampus glb test\vsude\kampus_vsude.glb");
        yield return DT("Usti_Bilina_project_glb", "GLB from the 'Ústí nad Labem (Bílina) I' project.", true,
            @"Ústí nad Labem (Bílina) I\models\5356931be81973855ffdca4867bb5ce1\model.glb");
        yield return DT("Stromy_3_obj_mtl", "74 MB OBJ, file name with a space.", false,
            $@"{ServerProjects}\Stromy 3\models\95335941d4260e5dfddf6cfad193b0fb\Stromy 3.obj",
            $@"{ServerProjects}\Stromy 3\models\95335941d4260e5dfddf6cfad193b0fb\Stromy 3.mtl");
        yield return DT("Kampus_3_9_2026_obj_mtl", "114 MB SketchUp OBJ, the newest Kampus.", false,
            $@"{ServerProjects}\Kampus_05_09_2026\models\f5c3f850f91360223fe0d7af0c3cf1c3\lokalita Kampus UJEP_3_9_2026.obj",
            $@"{ServerProjects}\Kampus_05_09_2026\models\f5c3f850f91360223fe0d7af0c3cf1c3\lokalita Kampus UJEP_3_9_2026.mtl");
        yield return DT("Litvinov_glb_draco", "72 MB GLB with Draco meshes.", true,
            @"Litvínov\Litvinov_Vystupy\draco.glb");
        yield return DT("Litvinov_glb_uncompressed", "302 MB GLB, no compression.", true,
            @"Litvínov\Litvinov_Vystupy\Litvinov.glb");
        yield return Akagi("Akagi_obj_png_textures",
            "131 MB OBJ (5 km incl. the horizon meshes), 114 PNG textures. Decimal commas in the numbers ('v 181,1149 ...', exported " +
            "on a Czech-locale PC). TriLib reads the comma as a thousands separator (181,1149 -> 1811149): 75 % of the numbers have " +
            "4 decimals, so the map keeps its shape but is ~10 000x too big, the rest come out distorted (spikes). " +
            "MTL refers to 'texture\\x.png' (a subfolder), the textures still load.", true,
            @"texture_png\akagi mountain pass.obj", @"texture_png\akagi mountain pass.mtl", @"texture_png\*.png");
        yield return Akagi("Akagi_obj_fixed_png_textures",
            "The Akagi OBJ + MTL with the decimal commas changed to dots (fixed_decimal_dots folder, made with " +
            "Tools/fix_obj_decimal_commas.py), 114 PNG textures. " +
            "Should come out about 5 km across (the horizon meshes), the road itself about 1.1 x 1.3 km.", true,
            @"fixed_decimal_dots\akagi mountain pass.obj", @"fixed_decimal_dots\akagi mountain pass.mtl", @"texture_png\*.png");
        yield return Akagi("Akagi_fbx", "86 MB ASCII FBX with decimal commas ('a: 181,1149,218,5373,...'), TriLib can't parse it. " +
            "Its textures are .dds, which the upload dialog doesn't allow, so none are selected.", false,
            @"akagi mountain pass.fbx");
    }
}
