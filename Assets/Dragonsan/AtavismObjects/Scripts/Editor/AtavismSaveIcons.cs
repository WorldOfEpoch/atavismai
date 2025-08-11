using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEditor;
using UnityEngine;
//using System.Drawing.Image;
using B83.Image.BMP;
using Color = UnityEngine.Color;

namespace Atavism
{

    public class AtavismSaveIcons : EditorWindow
    {
        [MenuItem("Window/Atavism/Atavism Icon Saver")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AtavismSaveIcons));
        }

        int size = 64;
        bool overide = false;

        private string status = "";
        private Sprite icon;
        public void OnGUI()
        { 
            overide = EditorGUILayout.Toggle("Override Icon size", overide);
            if (overide)
                size = EditorGUILayout.IntField("New Icon Size", size);

            if (GUILayout.Button("Save Icons to database"))
            {
                status = "";
                updateIcons();
                
            }
            GUILayout.Space(10);
            GUILayout.Label(status);
            // EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite));
            // if (GUILayout.Button("load tga"))
            // {
            //    // status = "";
            //    // updateIcons();
            //    // Assets/Dragonsan/AtavismObjects/Icons/Custom/Scrolls and Books/Scrolls and Books_13_ver.png
            //    // using (TargaImage ti = new TargaImage("Assets/Archer/1020000.tga"))
            //    // {
            //    //     //return CopyToBitmap(ti);
            //    // }
            //     TargaImage aa = new TargaImage();
            //    // Bitmap bmp  = aa.LoadTargaImage(@"Assets/Archer/1020000.tga");
            //    // MemoryStream ms= new MemoryStream();
            //    // bmp.Save(ms,ImageFormat.Png);
            //    // var buffer = new byte[ms.Length];
            //    // ms.Position = 0;
            //    // ms.Read(buffer,0,buffer.Length);
            //    // Texture2D t = new Texture2D(1,1);
            //    // t.LoadImage(buffer);
            //    Texture2D t  = aa.LoadTargaImage(@"Assets/Archer/1020001.tga");
            //    icon = Sprite.Create(t, new Rect(0.0f, 0.0f, t.width, t.height), new Vector2(0.5f, 0.5f), 100.0f);
            //
            //  //  loadtgaico("Assets/Archer/1020000.tga");
            //    
            // }
        }

        void loadtgaico(string file)
        {
            using (var imageFile = File.OpenRead(file))
            {
                try
                {
                    Texture2D tex = LoadTGA(imageFile);
                    icon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    status = "done";
                }
                catch (Exception e)
                {
                  Debug.LogError("Exception "+e);
                  status = "exeption";
                }
            }
            
        }
        
        void updateIcons()
        {
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            // string query2 = "SELECT action FROM server";
            string[] tables = new string[] { "skills", "currencies", "item_templates", "abilities", "effects", "build_object_template", "crafting_recipes" };
            // If there is a row, clear it.
            foreach (string table in tables)
            {
                string query = "SELECT id, icon FROM " + table + " WHERE isactive = 1";

                if (rows != null)
                    rows.Clear();
                // Load data
                rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                // Read all the data
                if ((rows != null) && (rows.Count > 0))
                {
                    foreach (Dictionary<string, string> data in rows)
                    {
                       
                            int id = int.Parse(data["id"]);
                            string icon = data["icon"];
                            string icon2 = "";
                        try
                        {
                            if (overide)
                                icon2 = getIcon2(icon);
                            else
                                icon2 = getIcon(icon);
                            Debug.Log("In " + table + " Icon " + icon + " form id " + id + " Length=" + icon2.Length);

                            if (icon2.Length > 250000)
                            {
                                Debug.LogError("In " + table + " Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                            }
                            else if (icon2.Length > 50000)
                            {
                                Debug.LogWarning("In " + table + " Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                            }

                            string queryU = "UPDATE " + table + " SET icon2=?icon2, updatetimestamp=?updatetimestamp WHERE id=?id";

                            // Setup the register data		
                            List<Register> update = new List<Register>();

                            update.Add(new Register("id", "?id", MySqlDbType.Int32, id.ToString(), Register.TypesOfField.Int));
                            update.Add(new Register("icon2", "?icon2", MySqlDbType.String, icon2, Register.TypesOfField.String));
                            update.Add(new Register("updatetimestamp", "?updatetimestamp", MySqlDbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Register.TypesOfField.String));

                            DatabasePack.Update(DatabasePack.contentDatabasePrefix, queryU, update);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Exception with icon "+icon+" from table "+table+"  "+e);
                        }
                    }
                }

                Debug.Log("Done for " + table);
            }

            string query1 = "SELECT id, icon FROM character_create_gender WHERE isactive = 1";

            if (rows != null)
                rows.Clear();
            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query1);
            // Read all the data
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    int id = int.Parse(data["id"]);
                    string icon = data["icon"];
                    string icon2 = "";
                    try
                    {
                        if (overide)
                            icon2 = getIcon2(icon);
                        else
                            icon2 = getIcon(icon);
                        Debug.Log("In character_create_gender Icon " + icon + " form id " + id + " Length=" + icon2.Length);

                        if (icon2.Length > 250000)
                        {
                            Debug.LogError("In  character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution. ");
                        }
                        else if (icon2.Length > 50000)
                        {
                            Debug.LogWarning("In character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                        }

                        string queryU = "UPDATE character_create_gender SET icon2=?icon2, updatetimestamp=?updatetimestamp WHERE id=?id";

                        // Setup the register data		
                        List<Register> update = new List<Register>();

                        update.Add(new Register("id", "?id", MySqlDbType.Int32, id.ToString(), Register.TypesOfField.Int));
                        update.Add(new Register("icon2", "?icon2", MySqlDbType.String, icon2, Register.TypesOfField.String));
                        update.Add(new Register("updatetimestamp", "?updatetimestamp", MySqlDbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Register.TypesOfField.String));

                        DatabasePack.Update(DatabasePack.contentDatabasePrefix, queryU, update);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception with icon " + icon + " for gender " + e);
                    }
                }
            }

            query1 = "SELECT id, race_icon, class_icon FROM character_create_template WHERE isactive = 1";

            if (rows != null)
                rows.Clear();
            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query1);
            // Read all the data
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    int id = int.Parse(data["id"]);
                    string icon = data["race_icon"];
                    string icon2 = "";
                    try
                    {
                        if (overide)
                            icon2 = getIcon2(icon);
                        else
                            icon2 = getIcon(icon);
                        Debug.Log("In character_create_gender Icon " + icon + " form id " + id + " Length=" + icon2.Length);

                        if (icon2.Length > 250000)
                        {
                            Debug.LogError("In  character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                        }
                        else if (icon2.Length > 50000)
                        {
                            Debug.LogWarning("In character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                        }

                        string queryU = "UPDATE character_create_template SET race_icon2=?race_icon2, updatetimestamp=?updatetimestamp WHERE id=?id";

                        // Setup the register data		
                        List<Register> update = new List<Register>();

                        update.Add(new Register("id", "?id", MySqlDbType.Int32, id.ToString(), Register.TypesOfField.Int));
                        update.Add(new Register("race_icon2", "?race_icon2", MySqlDbType.String, icon2, Register.TypesOfField.String));
                        update.Add(new Register("updatetimestamp", "?updatetimestamp", MySqlDbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Register.TypesOfField.String));

                        DatabasePack.Update(DatabasePack.contentDatabasePrefix, queryU, update);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception with icon " + icon + " for race " + e);
                    }

                    icon = data["class_icon"];
                    icon2 = "";
                    try
                    {
                        if (overide)
                            icon2 = getIcon2(icon);
                        else
                            icon2 = getIcon(icon);
                        Debug.Log("In character_create_gender Icon " + icon + " form id " + id + " Length=" + icon2.Length);

                        if (icon2.Length > 250000)
                        {
                            Debug.LogError("In  character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                        }
                        else if (icon2.Length > 50000)
                        {
                            Debug.LogWarning("In character_create_gender Icon " + icon + " form id " + id + " is not optimized and will consume some of your server memory. Consider optimizing its size by reducing its resolution.");
                        }

                        string queryU = "UPDATE character_create_template SET class_icon2=?class_icon2, updatetimestamp=?updatetimestamp WHERE id=?id";

                        // Setup the register data		
                        List<Register> update = new List<Register>();

                        update.Add(new Register("id", "?id", MySqlDbType.Int32, id.ToString(), Register.TypesOfField.Int));
                        update.Add(new Register("class_icon2", "?class_icon2", MySqlDbType.String, icon2, Register.TypesOfField.String));
                        update.Add(new Register("updatetimestamp", "?updatetimestamp", MySqlDbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Register.TypesOfField.String));

                        DatabasePack.Update(DatabasePack.contentDatabasePrefix, queryU, update);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception with icon " + icon + " for class " + e);
                    }


                }
            }





            Debug.Log("Done");
            status = "Status: Done";
        }

        string getIcon(string icon)
        {
            Debug.Log("getIcon");
            Sprite sicon = (Sprite)AssetDatabase.LoadAssetAtPath(icon, typeof(Sprite));
            if (System.IO.File.Exists(icon))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(icon);

                Texture2D tex = new Texture2D(2, 2);
                
                if (icon.EndsWith(".BMP") || icon.EndsWith(".bmp"))
                {
                    BMPLoader bmpLoader = new BMPLoader();
                    BMPImage bmpImg = bmpLoader.LoadBMP(fileData);
                    tex = bmpImg.ToTexture2D();
                }
                else if (icon.EndsWith(".TGA") || icon.EndsWith(".tga"))
                {
                    TargaImage aa = new TargaImage();
                    tex  = aa.LoadTargaImage(icon);

                }
                else

                {
                    tex.LoadImage(fileData);
                }
                
                byte[] b = tex.EncodeToPNG();
                if (tex.width > sicon.texture.width && tex.height > sicon.texture.height)
                {
                    Texture2D result = new Texture2D(sicon.texture.width, sicon.texture.height, tex.format, true);
                    Color[] rpixels = tex.GetPixels(0);
                    Color[] rezpixel = new Color[(sicon.texture.width * sicon.texture.height)];
                    float incX = ((float)1 / tex.width) * ((float)tex.width / sicon.texture.width);
                    float incY = ((float)1 / tex.height) * ((float)tex.height / sicon.texture.height);
                    // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);
                    for (int px = 0; px < rezpixel.Length; px++)
                    {
                        //   Debug.LogError("TestImage: px=" + px + " X=" + (incX * ((float)px % sicon.texture.width) + " Y=" + (incY * (Mathf.Floor(px / sicon.texture.width))) ));
                        rezpixel[px] = tex.GetPixelBilinear(incX * ((float)px % sicon.texture.width),
                            incY * (Mathf.Floor(px / sicon.texture.width)));
                    }

                    // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);
                    result.SetPixels(rezpixel, 0);
                    result.Apply();
                    b = result.EncodeToPNG();
                }

                return System.Convert.ToBase64String(b);
            }

            return "";
        }

        string getIcon2(string icon)
        {
            Debug.Log("getIcon2");
            //  Sprite sicon = (Sprite)AssetDatabase.LoadAssetAtPath(icon, typeof(Sprite));
            if (System.IO.File.Exists(icon))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(icon);
                Texture2D tex = new Texture2D(2, 2);
                int width = 0;
                int height = 0;
                if (icon.EndsWith(".BMP") || icon.EndsWith(".bmp"))
                {
                    BMPLoader bmpLoader = new BMPLoader();
                    BMPImage bmpImg = bmpLoader.LoadBMP(fileData);
                    tex = bmpImg.ToTexture2D();
                }
                else if (icon.EndsWith(".TGA") || icon.EndsWith(".tga"))
                {
                    TargaImage aa = new TargaImage();
                    tex  = aa.LoadTargaImage(icon);
/*                    using (var imageFile = File.OpenRead(icon))
                    {
                        tex = LoadTGA(imageFile);
                    }*/
                }
                else

                {
                    tex.LoadImage(fileData);
                }

                byte[] b = tex.EncodeToPNG();
                if (tex.width == tex.height)
                {
                    if (tex.width > size || tex.height > size)
                    {
                        Texture2D result = new Texture2D(size, size, tex.format, true);
                        Color[] rpixels = tex.GetPixels(0);
                        Color[] rezpixel = new Color[(size * size)];
                        float incX = ((float)1 / tex.width) * ((float)tex.width / size);
                        float incY = ((float)1 / tex.height) * ((float)tex.height / size);
                        // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);
                        for (int px = 0; px < rezpixel.Length; px++)
                        {
                            //   Debug.LogError("TestImage: px=" + px + " X=" + (incX * ((float)px % sicon.texture.width) + " Y=" + (incY * (Mathf.Floor(px / sicon.texture.width))) ));
                            rezpixel[px] = tex.GetPixelBilinear(incX * ((float)px % size),
                                incY * (Mathf.Floor(px / size)));
                        }
                        // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);

                        result.SetPixels(rezpixel, 0);

                        result.Apply();


                        b = result.EncodeToPNG();
                    }

                    return System.Convert.ToBase64String(b);
                }
                else
                {
                    if (tex.width > size || tex.height > size)
                    {
                        if (tex.width > tex.height)
                        {
                            width = size;
                            height = (int)Mathf.Floor(tex.height / (float)(tex.width / size));
                        }
                        else
                        {
                            height = size;
                            width = (int)Mathf.Floor(tex.width / (float)(tex.height / size));
                        }

                        //   Debug.Log("Size h=" + height + " w=" + width + " | th=" + tex.height + " tw=" + tex.width + " | s=" + size);
                        Texture2D result = new Texture2D(width, height, tex.format, true);
                        Color[] rpixels = tex.GetPixels(0);
                        Color[] rezpixel = new Color[(width * height)];
                        float incX = ((float)1 / tex.width) * ((float)tex.width / width);
                        float incY = ((float)1 / tex.height) * ((float)tex.height / height);
                        // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);
                        for (int px = 0; px < rezpixel.Length; px++)
                        {
                            //   Debug.LogError("TestImage: px=" + px + " X=" + (incX * ((float)px % sicon.texture.width) + " Y=" + (incY * (Mathf.Floor(px / sicon.texture.width))) ));
                            rezpixel[px] = tex.GetPixelBilinear(incX * ((float)px % width),
                                incY * (Mathf.Floor(px / width)));
                        }

                        // Debug.LogError("TestImage: rpixels=" + rpixels.Length + " incX=" + incX + " incY=" + incY);
                        result.SetPixels(rezpixel, 0);
                        result.Apply();
                        b = result.EncodeToPNG();
                    }

                    return System.Convert.ToBase64String(b);
                }
            }

            return "";
        }

        public static Texture2D LoadTGA(Stream TGAStream)
        {

            using (BinaryReader r = new BinaryReader(TGAStream))
            {
                int id = r.ReadByte();
                int colorType = r.ReadByte();
                int imgType = r.ReadByte();
                Int16 colorMapFirstIndex = r.ReadInt16();
                Int16 colorMapLenght = r.ReadInt16();
                int colorMapEntrySize = r.ReadByte();
                Debug.LogError("id="+id+" colorType="+colorType+" imgType="+imgType+" colorMapFirstIndex="+colorMapFirstIndex+
                               " colorMapLenght="+colorMapLenght+" colorMapEntrySize="+colorMapEntrySize);

                Int16 xOrgin = r.ReadInt16();
                Int16 yOrgin = r.ReadInt16();

                // Skip some header info we don't care about.
                // Even if we did care, we have to move the stream seek point to the beginning,
                // as the previous method in the workflow left it at the end.
                //r.BaseStream.Seek(12, SeekOrigin.Begin);

                short width = r.ReadInt16();
                short height = r.ReadInt16();
                int bitDepth = r.ReadByte();
                int imgDecrypt = r.ReadByte();
Debug.LogError("xOrgin="+xOrgin+" yOrgin="+yOrgin+" width="+width+" height="+height+" bitDepth="+bitDepth+" imgDecrypt="+imgDecrypt);
                // Skip a byte of header information we don't care about.
                //r.BaseStream.Seek(1, SeekOrigin.Current);
                // for (int i = 0; i < 100; i++)
                // {
                //     int red1 = r.ReadByte();
                //     byte green1 = r.ReadByte();
                //     byte blue1 = r.ReadByte();
                //     byte alpha1 = r.ReadByte();
                //     byte alpha2 = r.ReadByte();
                //
                //     Debug.LogError("red=" + red1 + " green=" + green1 + " blue=" + blue1 + " alpha=" + alpha1 + " a2=" + alpha1);
                // }

           /*     int red11 = r.ReadByte();
                byte green11 = r.ReadByte();
                byte blue11 = r.ReadByte();
                byte alpha11 = r.ReadByte();
                byte alpha21 = r.ReadByte();

                Debug.LogError("red="+red11+" green="+green11+" blue="+blue11+" alpha="+alpha11+" a2="+alpha21);
                int red12 = r.ReadByte();
                byte green12 = r.ReadByte();
                byte blue12 = r.ReadByte();
                byte alpha12 = r.ReadByte();
                byte alpha22 = r.ReadByte();

                Debug.LogError("red="+red12+" green="+green12+" blue="+blue12+" alpha="+alpha12+" a2="+alpha22);
*/
                Texture2D tex = new Texture2D(width, height);
                Color32[] pulledColors = new Color32[width * height];
                try
                {
                    if (imgType == 10)
                    {
                        int count = 0;
                        for (int j = 0;j < 350; j++)  {
                            byte rle = r.ReadByte();
                            // internal static int GetBits(byte b, int offset, int count)
                            // {
                            //     return (b >> offset) & ((1 << count) - 1);
                            // }
                            int rle1 = (rle >> 7) & ((1 << 1) - 1);
                            int rlenum = (rle >> 0) & ((1 << 7) - 1);
                            byte alpha = r.ReadByte();
                            byte red = r.ReadByte();
                            byte green = r.ReadByte();
                            byte blue = r.ReadByte();
                            
                            Debug.LogWarning("count=" + count + " rle1="+rle1+" number=" + rlenum + " red=" + red + " green=" + green + " blue=" + blue + " alpha=" + alpha);
                            for (int i = 0; i <=  rlenum+1; i++)
                            {
                                pulledColors[count+i] = new Color32(blue, green, red, alpha);
                            }

                            count += rlenum+1;
          
                        }
                        //while (count < width * height) ;
                        tex.SetPixels32(pulledColors);
                        tex.Apply();
                        Debug.LogError("End count=" + count);
                        return tex;

                    } 


                    if (imgType != 10 && bitDepth == 32)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        Debug.LogWarning("For "+i);
                        byte red = r.ReadByte();
                        byte green = r.ReadByte();
                        byte blue = r.ReadByte();
                        byte alpha = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, alpha);
                    }
                }
                else if (bitDepth == 24)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        byte red = r.ReadByte();
                        byte green = r.ReadByte();
                        byte blue = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, 255);
                    }
                }else if (bitDepth == 8)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        byte red = r.ReadByte();
                        byte green = red;
                        byte blue = red;

                        pulledColors[i] = new Color32(blue, green, red, 255);
                    }
                }
                else
                {
                    throw new Exception("TGA texture had non 32/24 bit depth.");
                }
                }
                catch (Exception e)
                {
                    Debug.LogError("End Exception=" + e);
                }
                tex.SetPixels32(pulledColors);
                tex.Apply();
                return tex;

            }

        }
    }
}