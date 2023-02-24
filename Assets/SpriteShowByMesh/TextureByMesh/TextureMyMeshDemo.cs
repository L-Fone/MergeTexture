using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

[Serializable]
[SerializeField]
public class AtzJson
{
    public int cid;
    public int w;
    public int h;
    public int x;
    public int y;
    public int px;
    public int py;


    public string key { get; private set; }
    public int index { get; private set; }
    public int cIndex { get; private set; }
    public string atzName { get; private set; }
    public string pngName { get; private set; }
    public Vector2 Offset { get; private set; }
    public Vector2 Position { get; private set; }

    public void SetInfo(int idx, int cIdx, string atzName)
    {
        this.index = idx;
        this.cIndex = cIdx;
        this.atzName = atzName;
        this.pngName = $"{atzName}{cid}"; //格式:atzName+序号
        this.key = $"{pngName}_{idx}"; //格式:图片名_下标
        this.Offset = new Vector2(px, py);
        this.Position = new Vector2((px + w * 0.5f) * 0.1f, -(py + h * 0.5f) * 0.1f);
    }

    public Rect GetSpriteInTextureRect(float height)
    {
        float nx = this.x;
        float ny = height - this.y - this.h;
        return new Rect(nx, ny, w, h);
    }
}

public class TextureMyMeshDemo : MonoBehaviour
{
    private const int mergeWidth = 160; //目标合成图宽
    private const int mergeHeight = 160;//目标合成图高

    public TextureFormat textureFormat = TextureFormat.ETC2_RGBA8;//目标合成图格式

    public Texture2D sourceTexture; //源图 读Resource目录
    public int sourceX = 65;        //源图 读Resource目录Json, 对应字段: x
    public int sourceY = 62;        //源图 读Resource目录Json, 对应字段: y
    public int sourceWidth = 55;    //源图 读Resource目录Json, 对应字段: w
    public int sourceHeight = 20;   //源图 读Resource目录Json, 对应字段: h

    public int targetX = -8;        //合成位置偏移 以大图中心为原点。加上该值 json对应字段: px
    public int targetY = 30;        //合成位置偏移 以大图中心为原点。加上该值 json对应字段: py

    public string[] assets; //读Resource目录Json
    public Texture2D[] sourceTextures; //读Resource目录Texture
    public AtzJson[] Jsons;

    public Texture2D mergeTexture;
    private MeshRenderer renderer;

    private Material material;


    void Start()
    {
        Jsons = new AtzJson[assets.Length];
        for (int i = 0; i < assets.Length; i++)
        {
            var asset = Resources.Load<TextAsset>($"Json/{assets[i]}");
            var jsons = LitJson.JsonMapper.ToObject<List<AtzJson>>(asset.text);
            Jsons[i] = jsons[0];
        }

        TimeWatch.Start();
        mergeTexture = new Texture2D(mergeWidth, mergeHeight, textureFormat, false);
        MergeTex3();
        renderer = this.GetComponent<MeshRenderer>();
        material = renderer.sharedMaterial;
        TimeWatch.ShowTime($"合成耗时：");
    }

    public void Update()
    {
        if (mergeTexture != null) 
        {
            material.SetTexture("_MainTex", mergeTexture);
        }
    }



    public void MergeTex3()
    {
        if (mergeTexture == null)
            return;

        // 清空大图所有像素

        //大图像素
        NativeArray<byte> mergePixels = mergeTexture.GetRawTextureData<byte>();

        for (int i = 0; i < Jsons.Length; i++)
        {
            if (Jsons[i] == null)
                continue;

            var smallWidth = Jsons[i].w; // 小图的宽度
            var smallHeight = Jsons[i].h; // 小图的高度
            var offset = Jsons[i].Offset;

            //此处是上
            //var loader = AtzFactory.GetLoader(Jsons[i]);

            // 无法获取像素
            //if (null == loader || !loader.HasTexture2D)
            //    continue;

            NativeArray<byte> pixels = sourceTextures[i].GetRawTextureData<byte>(); // 小图的所有像素


            var sourceWidth = sourceTextures[i].width;
            var sourceHeight = sourceTextures[i].height;
            var sourceX = Jsons[i].x;
            var sourceY = sourceHeight - Jsons[i].y - Jsons[i].h;

            int targetWidth = Jsons[i].w; // 小图的宽度
            int targetHeight = Jsons[i].h; // 小图的宽度
            int targetX = 80 + (int)offset.x;//大图从左往右的起始位置
            int targetY = 80 - (int)(offset.y + smallHeight);//大图从上往下的起始位置

            int count = 0;
            try
            {
                for (int y = 0; y < targetHeight; y++)
                {
                    for (int x = 0; x < targetWidth; x++)
                    {
                        //计算在源纹理中的坐标
                        int sourcePixelX = sourceX + x;
                        int sourcePixelY = sourceY + y;

                        //计算在目标纹理中的坐标
                        int targetPixelX = targetX + x;
                        int targetPixelY = targetY + y;

                        if (targetPixelX < 0 || targetPixelX >= mergeWidth || targetPixelY < 0 || targetPixelY >= mergeHeight)
                            continue;

                        //获取源纹理中指定像素的颜色
                        int sourceIndex = (sourcePixelY * sourceWidth + sourcePixelX);

                        var color = pixels[sourceIndex];

                        //int r = color >> 6;
                        //int g = (color >> 4) & 0x3;
                        //int b = (color >> 2) & 0x3;
                        //int a = color & 0x3;
                        //byte a = (byte)((color & 0x80) >> 7); // 获取alpha分量，假设a的值为1
                        //byte r = (byte)((color & 0x70) >> 4); // 获取red分量，假设r的值为10
                        //byte g = (byte)((color & 0x0C) >> 2); // 获取green分量，假设g的值为3
                        //byte b = (byte)(color & 0x03); // 获取blue分量，假设b的值为3

                        //if (a > 0)
                        //{
                        int targetIndex = (targetPixelY * mergeWidth + targetPixelX);
                        mergePixels[targetIndex] = color;
                        //}
                    }
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return;
            }
        }

        mergeTexture.LoadRawTextureData(mergePixels);
        mergeTexture.Apply();
    }


    //public unsafe Texture MergeTex()
    //{
    //    var current = texture.GetPixels();

    //    int largeWidth = texture.width;//大图的宽度
    //    int largeHeight = texture.height;//大图的高度

    //    //清空大图所有像素
    //    fixed (Color* largePtr = current)
    //        Unsafe.InitBlock(largePtr, 0, (uint)(largeWidth * largeHeight * sizeof(Color)));

    //    for (int i = 0; i < textures.Length; i++)
    //    {
    //        var rect = textures[i].textureRect;

    //        if (!dict.TryGetValue(i, out var color))
    //        {
    //            color = textures[i].texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    //            dict[i] = color;
    //        }

    //        int offsetX = 80 + (int)offset[i].x;//大图从左往右的起始位置
    //        int offsetY = 80 - (int)(offset[i].y + rect.height);//大图从上往下的起始位置

    //        int smallWidth = (int)rect.width;//小图的宽度
    //        int smallHeight = (int)rect.height;//小图的高度

    //        int count = 0;
    //        try
    //        {
    //            fixed (Color* largePtr = current, smallPtr = color)
    //            {
    //                for (int j = 0; j < smallHeight; j++)//逐行处理
    //                {
    //                    //大图这一行的指针
    //                    Color* pLarge = largePtr + offsetX + (j + offsetY) * largeWidth;//大图这一行的起始指针
    //                                                                                    //小图这一行的指针
    //                    Color* pSmall = smallPtr + j * smallWidth;//小图这一行的起始指针

    //                    while (pSmall++ < smallPtr + (j + 1) * smallWidth)//这一行没结束
    //                    {
    //                        //这里判断小图像素可以pSmall->成员，或者(*pSmall).成员
    //                        if (pSmall->a != 0)
    //                        {
    //                            *pLarge = *pSmall;//大图这个像素点的颜色 = 小图这个像素点的颜色
    //                        }
    //                        pLarge++;//自增指针
    //                    }

    //                    count += smallWidth;
    //                }

    //                //Debug.LogError($"找到了{count}个像素点");
    //            }
    //        }
    //        catch (System.Exception ex)
    //        {
    //            Debug.LogError(ex.ToString());
    //        }
    //    }

    //    texture.SetPixels(current);
    //    texture.Apply();

    //    return texture;
    //}

    public void WriteTexture() 
    {
        byte[] byt = mergeTexture.EncodeToPNG(); // EncodeToJPG();
        string name = $"text.png";
        string outPath = Path.Combine(Application.dataPath, "../../out/");
        string outFiled = outPath + name;
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);
        File.WriteAllBytes(outFiled, byt);
        Application.OpenURL(outPath);
    }

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TextureMyMeshDemo))]
public class TextureMyMeshDemoEditor : UnityEditor.Editor
{
    TextureMyMeshDemo table;
    void OnEnable()
    {
        table = target as TextureMyMeshDemo;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
  
        if (UnityEngine.GUILayout.Button("刷新图片"))
        {
            table?.MergeTex3();
        }
        if (UnityEngine.GUILayout.Button("写出图片"))
        {
            table?.WriteTexture();
        }
    }
}
#endif

