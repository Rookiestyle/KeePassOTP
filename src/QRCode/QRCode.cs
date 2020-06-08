/*
This file was taken from https://github.com/codebude/QRCoder and is licensed under the MIT license.
---------------------
The MIT License (MIT)

Copyright (c) 2013-2015 Raffael Herrmann

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace QRCoder
{
	public class QRCode : AbstractQRCode, IDisposable
	{
		/// <summary>
		/// Constructor without params to be used in COM Objects connections
		/// </summary>
		public QRCode() { }

		public QRCode(QRCodeData data) : base(data) { }

		public Bitmap GetGraphic(int pixelsPerModule)
		{
			return this.GetGraphic(pixelsPerModule, Color.Black, Color.White, true);
		}

		public Bitmap GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
		{
			return this.GetGraphic(pixelsPerModule, ColorTranslator.FromHtml(darkColorHtmlHex), ColorTranslator.FromHtml(lightColorHtmlHex), drawQuietZones);
		}

		public Bitmap GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, bool drawQuietZones = true)
		{
			var size = (this.QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
			var offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

			var bmp = new Bitmap(size, size);
			using (var gfx = Graphics.FromImage(bmp))
			using (var lightBrush = new SolidBrush(lightColor))
			using (var darkBrush = new SolidBrush(darkColor))
			{
				for (var x = 0; x < size + offset; x = x + pixelsPerModule)
				{
					for (var y = 0; y < size + offset; y = y + pixelsPerModule)
					{
						var module = this.QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];

						if (module)
						{
							gfx.FillRectangle(darkBrush, new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
						}
						else
						{
							gfx.FillRectangle(lightBrush, new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
						}
					}
				}

				gfx.Save();
			}

			return bmp;
		}

		public Bitmap GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, Bitmap icon = null, int iconSizePercent = 15, int iconBorderWidth = 6, bool drawQuietZones = true)
		{
			var size = (this.QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
			var offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

			var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			using (var gfx = Graphics.FromImage(bmp))
			using (var lightBrush = new SolidBrush(lightColor))
			using (var darkBrush = new SolidBrush(darkColor))
			{
				gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gfx.CompositingQuality = CompositingQuality.HighQuality;
				gfx.Clear(lightColor);

				var drawIconFlag = icon != null && iconSizePercent > 0 && iconSizePercent <= 100;

				GraphicsPath iconPath = null;
				float iconDestWidth = 0, iconDestHeight = 0, iconX = 0, iconY = 0;

				if (drawIconFlag)
				{
					iconDestWidth = iconSizePercent * bmp.Width / 100f;
					iconDestHeight = drawIconFlag ? iconDestWidth * icon.Height / icon.Width : 0;
					iconX = (bmp.Width - iconDestWidth) / 2;
					iconY = (bmp.Height - iconDestHeight) / 2;

					var centerDest = new RectangleF(iconX - iconBorderWidth, iconY - iconBorderWidth, iconDestWidth + iconBorderWidth * 2, iconDestHeight + iconBorderWidth * 2);
					iconPath = this.CreateRoundedRectanglePath(centerDest, iconBorderWidth * 2);
				}

				for (var x = 0; x < size + offset; x = x + pixelsPerModule)
				{
					for (var y = 0; y < size + offset; y = y + pixelsPerModule)
					{
						var module = this.QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];

						if (module)
						{
							var r = new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule);

							if (drawIconFlag)
							{
								var region = new Region(r);
								region.Exclude(iconPath);
								gfx.FillRegion(darkBrush, region);
							}
							else
							{
								gfx.FillRectangle(darkBrush, r);
							}
						}
						else
						{
							gfx.FillRectangle(lightBrush, new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
						}
					}
				}

				if (drawIconFlag)
				{
					var iconDestRect = new RectangleF(iconX, iconY, iconDestWidth, iconDestHeight);
					gfx.DrawImage(icon, iconDestRect, new RectangleF(0, 0, icon.Width, icon.Height), GraphicsUnit.Pixel);
				}

				gfx.Save();
			}

			return bmp;
		}

		internal GraphicsPath CreateRoundedRectanglePath(RectangleF rect, int cornerRadius)
		{
			var roundedRect = new GraphicsPath();
			roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
			roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
			roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
			roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
			roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
			roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
			roundedRect.CloseFigure();
			return roundedRect;
		}
	}

	public static class QRCodeHelper
	{
		public static Bitmap GetQRCode(string plainText, int pixelsPerModule, Color darkColor, Color lightColor, QRCodeGenerator.ECCLevel eccLevel)
		{
			Bitmap icon = null;
			int iconSizePercent = 15;
			int iconBorderWidth = 6;
			bool drawQuietZones = true;
			using (var qrGenerator = new QRCodeGenerator())
			using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel))
			using (var qrCode = new QRCode(qrCodeData))
				return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones);
		}
	}

	public abstract class AbstractQRCode
	{
		protected QRCodeData QrCodeData { get; set; }

		protected AbstractQRCode()
		{
		}

		protected AbstractQRCode(QRCodeData data)
		{
			this.QrCodeData = data;
		}

		/// <summary>
		/// Set a QRCodeData object that will be used to generate QR code. Used in COM Objects connections
		/// </summary>
		/// <param name="data">Need a QRCodeData object generated by QRCodeGenerator.CreateQrCode()</param>
		virtual public void SetQRCodeData(QRCodeData data)
		{
			this.QrCodeData = data;
		}

		public void Dispose()
		{
			if (this.QrCodeData != null) this.QrCodeData.Dispose();
			this.QrCodeData = null;
		}
	}
}

namespace QRCoder.Exceptions
{ 
	public class DataTooLongException : Exception
	{
		public DataTooLongException(string eccLevel, string encodingMode, int maxSizeByte) : base(
			"The given payload exceeds the maximum size of the QR code standard. The maximum size allowed for the choosen paramters (ECC level={eccLevel}, EncodingMode={encodingMode}) is {maxSizeByte} byte."
		)
		{ }
	}
}