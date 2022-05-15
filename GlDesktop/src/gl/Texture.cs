using MathStuff;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl; 

public enum TextureCoordinate : uint
{
	S = TextureParameterName.TextureWrapS,
	T = TextureParameterName.TextureWrapT,
	R = TextureParameterName.TextureWrapR
}

public class Texture {
	public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat)All.Srgb8Alpha8;
        public const SizedInternalFormat RGB32F = (SizedInternalFormat)All.Rgb32f;

        public const GetPName MAX_TEXTURE_MAX_ANISOTROPY = (GetPName)0x84FF;

        public static readonly float MaxAniso;

        static Texture()
        {
            GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPY, ref MaxAniso);
        }

        public readonly string Name;
        public readonly int GLTexture;
        public readonly int width, height;
        public readonly int MipmapLevels;
        public readonly SizedInternalFormat InternalFormat;

        public unsafe Texture(string name, byte* image, int width, int height, bool generateMipmaps, bool srgb) {
            this.width = width;
            this.height = height;
            Name = name;
            InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;

            if (generateMipmaps)
            {
                // Calculate how many levels to generate for this texture
                MipmapLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
            }
            else
            {
                // There is only one level
                MipmapLevels = 1;
            }

            //Util.CheckGLError("Clear");

            GLTexture = GL.CreateTexture(TextureTarget.Texture2d).Handle;
            GL.TextureStorage2D((TextureHandle)GLTexture, MipmapLevels, InternalFormat, width, height);
            //Util.CheckGLError("Storage2d");

            //BitmapData data = image.LockBits(new Rectangle(0, 0, Width, Height),
            //    ImageLockMode.ReadOnly, global::System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            GL.TextureSubImage2D((TextureHandle)GLTexture, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, image);
            //Util.CheckGLError("SubImage");

            //image.UnlockBits(data);

            if (generateMipmaps) GL.GenerateTextureMipmap((TextureHandle)GLTexture);

            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            //Util.CheckGLError("WrapS");
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
           // Util.CheckGLError("WrapT");

            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMinFilter, (int)(generateMipmaps ? TextureMinFilter.Linear : TextureMinFilter.LinearMipmapLinear));
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //Util.CheckGLError("Filtering");

            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            // This is a bit weird to do here
            //image.Dispose();
        }

        public Texture(string name, int GLTex, int width, int height, int mipmaplevels, SizedInternalFormat internalFormat)
        {
            Name = name;
            GLTexture = GLTex;
            this.width = width;
            this.height = height;
            MipmapLevels = mipmaplevels;
            InternalFormat = internalFormat;
        }

        public Texture(string name, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
        {
            Name = name;
            this.width = width;
            this.height = height;
            InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
            MipmapLevels = generateMipmaps == false ? 1 : (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            GLTexture = GL.CreateTexture(TextureTarget.Texture2d).Handle;
            //Util.CreateTexture(TextureTarget.Texture2d, Name, out GLTexture);
            GL.TextureStorage2D((TextureHandle)GLTexture, MipmapLevels, InternalFormat, width, height);

            GL.TextureSubImage2D((TextureHandle)GLTexture, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            if (generateMipmaps) GL.GenerateTextureMipmap((TextureHandle)GLTexture);

            SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
            SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMinFilter, (int)filter);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMagFilter, (int)filter);
        }

        public void SetAnisotropy(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPY = (TextureParameterName)0x84FE;
            GL.TextureParameteri((TextureHandle)GLTexture, TEXTURE_MAX_ANISOTROPY, (int)math.clamp(level, 1, MaxAniso));
        }

        public void SetLod(int @base, int min, int max)
        {
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMinLod, min);
            GL.TextureParameteri((TextureHandle)GLTexture, TextureParameterName.TextureMaxLod, max);
        }
        
        public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
        {
            GL.TextureParameteri((TextureHandle)GLTexture, (TextureParameterName)coord, (int)mode);
        }

        public void Dispose()
        {
            GL.DeleteTexture((TextureHandle)GLTexture);
        }
}