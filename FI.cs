using System;
using System.Runtime.InteropServices;
namespace FreeImage
{
        public enum FI_FORMAT
        {
            FIF_UNKNOWN = -1,
            FIF_BMP = 0,
            FIF_ICO = 1,
            FIF_JPEG = 2,
            FIF_JNG = 3,
            FIF_KOALA = 4,
            FIF_LBM = 5,
            FIF_IFF = FIF_LBM,
            FIF_MNG = 6,
            FIF_PBM = 7,
            FIF_PBMRAW = 8,
            FIF_PCD = 9,
            FIF_PCX = 10,
            FIF_PGM = 11,
            FIF_PGMRAW = 12,
            FIF_PNG = 13,
            FIF_PPM = 14,
            FIF_PPMRAW = 15,
            FIF_RAS = 16,
            FIF_TARGA = 17,
            FIF_TIFF = 18,
            FIF_WBMP = 19,
            FIF_PSD = 20,
            FIF_CUT = 21,
            FIF_XBM = 22,
            FIF_XPM = 23,
            FIF_DDS = 24,
            FIF_GIF = 25,
            FIF_HDR = 26
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Reserved;
        }

	   unsafe public class FI
	   {
        //创建一个位图
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_Allocate", SetLastError = true)]
        public static extern IntPtr Allocate(int width, int height, int bpp);

        //从内存载入一个位图
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_LoadFromMemory", SetLastError = true)]
        private static extern IntPtr LoadFromMemory(FI_FORMAT fif, int stream, int Flag);

        //创建流
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_OpenMemory", SetLastError = true)]
        public static extern int OpenMemory(int data, int size);

        //关闭流
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_CloseMemory", SetLastError = true)]
        public static extern void CloseMemory(int stream);

        //从流中获取数据
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_AcquireMemory", SetLastError = true)]
        public static extern bool AcquireMemory(int stream, ref int data, ref int size);

        //将位图保存到内存
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_SaveToMemory", SetLastError = true)]
        private static extern bool SaveToMemory(FI_FORMAT fif, IntPtr dib, int stream, int flag);

        //水平翻转图像
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_FlipHorizontal", SetLastError = true)]
        public static extern bool FlipHorizontal(IntPtr dib);

        //垂直翻转图像
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_FlipVertical", SetLastError = true)]
        public static extern bool FlipVertical(IntPtr dib);
        
        //获取图像宽度
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_GetWidth", SetLastError = true)]
        public static extern int GetWidth(IntPtr Dib);
 
        //获取图像高度
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_GetHeight", SetLastError = true)]
        public static extern int GetHeight(IntPtr Dib);
 
        //获取图像色表指针
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_GetPalette", SetLastError = true)]
        public static extern RGBQUAD* GetPalette(IntPtr Dib);
 
        //获取图像颜色数量
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_GetColorsUsed", SetLastError = true)]
        public static extern int GetColorsUsed(IntPtr Dib);

        //获取图像像素数据指针
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_GetBits", SetLastError = true)]
        public static extern IntPtr GetBits(IntPtr Dib);
 
        //释放图像
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_Unload", SetLastError = true)]
        public static extern void Free(IntPtr Dib);

        //从像素数据创建
        [DllImport("FreeImage.dll", EntryPoint = "FreeImage_ConvertFromRawBits", SetLastError = true)]
        public static extern IntPtr ConvertFromRawBits(byte[] bits, int width, int height, int pitch, int bpp, int red_mask, int green_mask, int blue_mask, bool topdown);

        public static IntPtr LoadFromMemory(byte[] sprData,FI_FORMAT type)
        {
            //读图像数据并储存在非托管内存
            IntPtr sprPoint = Marshal.AllocHGlobal(sprData.Length);
            Marshal.Copy(sprData,0,sprPoint,sprData.Length);

            //从非托管内存提交给FreeImage
            int sprStream = FI.OpenMemory((int)sprPoint, sprData.Length);
            IntPtr dib = FI.LoadFromMemory(type, sprStream, 0);

            //释放
            Marshal.FreeHGlobal(sprPoint);
            FI.CloseMemory(sprStream);

            return dib;
        }

        public static bool SaveToMemory(IntPtr dib,ref byte[] sprData,FI_FORMAT type)
        {
            //保存图像到流
            int newSprStream = FI.OpenMemory(0,0);
            if(FI.SaveToMemory(type, dib, newSprStream, 0) == false)
            {
                return false;
            }

            //将非托管数据重新转为托管类型
            int newSprPoint = 0;
            int newLen = 0;
            FI.AcquireMemory(newSprStream, ref newSprPoint, ref newLen);
            sprData = new byte[newLen];
            Marshal.Copy((IntPtr)newSprPoint, sprData, 0, newLen);

            //释放
            FI.CloseMemory(newSprStream);

            return true;
        }

	   }

}