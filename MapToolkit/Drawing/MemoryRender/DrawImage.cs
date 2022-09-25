using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawImage : IDrawOperation
    {
        public DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            Image = image;
            Pos = pos;
            Size = size;
            Alpha = alpha;
            Min = pos;
            Max = pos + size;
        }
        public Vector Min { get; }
        public Vector Max { get; }
        public Image Image { get; }
        public Vector Pos { get; }
        public Vector Size { get; }
        public double Alpha { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawImage(Image, Pos, Size, Alpha);
        }

        public void DrawClipped(MemDrawClipped context)
        {
            var scale = Size / new Vector(Image.Width, Image.Height);

            var target = context.Translate(Pos);
            var posX = target.X;
            var posY = target.Y;

            var cropX = 0;
            var cropY = 0;
            var cropW = Image.Width;
            var cropH = Image.Height;

            if (posX < 0)
            {
                var cropXD = -(target.X * Image.Width / Size.X);
                cropX = (int)Math.Floor(cropXD);
                cropW -= cropX;
                posX = -((cropXD - cropX) * Size.X / Image.Width);
            }

            if (posY < 0)
            {
                var cropYD = -(target.Y * Image.Height / Size.Y);
                cropY = (int)Math.Floor(cropYD);
                cropH -= cropY;
                posY = -((cropYD - cropY) * Size.Y / Image.Height);
            }

            var x2 = context.ClipMin.X + posX + (cropW * Size.X / Image.Width);
            if (x2 > context.ClipMax.X)
            {
                cropW = Math.Min((int)Math.Floor((context.ClipMax.X - posX - context.ClipMin.X) * Image.Width / Size.X), Image.Width - cropX);
            }

            var y2 = context.ClipMin.Y + posY + (cropH * Size.Y / Image.Height);
            if (y2 > context.ClipMax.Y)
            {
                cropH = Math.Min((int)Math.Floor((context.ClipMax.Y - posY - context.ClipMin.Y) * Image.Height / Size.Y), Image.Height - cropY);
            }

            var posW = (cropW * Size.X / Image.Width);
            var posH = (cropH * Size.Y / Image.Height);

            var clipped = Image.Clone(i => i.Crop(new Rectangle(cropX, cropY, cropW, cropH)));
            context.Target.DrawImage(clipped, new Vector(posX, posY), new Vector(posW, posH), Alpha);
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawImage(Image.Clone(i => i.Resize((int)(Image.Width * context.Scale), (int)(Image.Height * context.Scale))), Pos * context.Scale, Size * context.Scale, Alpha);
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            yield return this;
        }
    }
}