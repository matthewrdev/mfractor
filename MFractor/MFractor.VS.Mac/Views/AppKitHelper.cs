using System;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt;
using Xwt.Drawing;

namespace MFractor.VS.Mac.Views
{
	static class AppKitHelper
	{
		public static NSButton CreateLinkButton(string text, Action clicked)
        {
			var value = new NSMutableAttributedString(text);
			var range = new NSRange(0, value.Length);

			value.AddAttribute(NSStringAttributeKey.ForegroundColor, MonoDevelop.Ide.Gui.Styles.LinkForegroundColor.ToNSColor(), range);
			value.AddAttribute(NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

			var button = new NSButton()
			{
				Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
				AttributedTitle = value,
			};

			void callback(object sender, EventArgs args)
            {
				clicked?.Invoke();
            }

			button.Activated += callback;
			button.WantsLayer = true;
			if (button.Layer != null)
			{
				button.Layer.BackgroundColor = NSColor.Clear.CGColor;
			}
			button.Bordered = false;

			return button;
		}

		public static CGSize GetTextSize(this string text, NSFont font)
		{
			var nsText = new NSString(text);

			return nsText.GetTextSize(font);
		}

		public static CGSize GetTextSize(this NSString text, NSFont font)
		{
			var attribs = new NSStringAttributes { Font = font };
			return text.StringSize(attribs);
		}

		public static void ClearChildren(this NSStackView stackView)
		{
			if (stackView == null)
			{
				return;
			}

			try
			{
				foreach (var view in stackView.Views)
				{
					stackView.RemoveArrangedSubview(view);
					NSLayoutConstraint.DeactivateConstraints(view.Constraints);
					view.RemoveFromSuperview();
				}
			}
			catch (Exception ex)
			{
				Console.Write(ex.ToString());
			}
		}

		public static NSColor ToNSColor(this Color col)
		{
			return NSColor.FromDeviceRgba((float)col.Red, (float)col.Green, (float)col.Blue, (float)col.Alpha);
		}

		//public static NSColor ToNSColor(this Cairo.Color col)
		//{
		//	return NSColor.FromDeviceRgba((float)col.R, (float)col.G, (float)col.B, (float)col.A);
		//}

		public static NSColor ToNSColor(this System.Drawing.Color col)
		{
			return NSColor.FromDeviceRgba(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
		}

		static readonly CoreGraphics.CGColorSpace deviceRgbColorSpace = CoreGraphics.CGColorSpace.CreateDeviceRGB();

		//public static CoreGraphics.CGColor ToCGColor(this Cairo.Color col)
		//{
		//	return new CoreGraphics.CGColor(deviceRgbColorSpace, new NFloat[] {
		//		(NFloat)col.R, (NFloat)col.G, (NFloat)col.B, (NFloat)col.A
		//	});
		//}

		public static CoreGraphics.CGColor ToCGColor(this Color col)
		{
			return new CoreGraphics.CGColor(deviceRgbColorSpace, new NFloat[] {
				(NFloat)col.Red, (NFloat)col.Green, (NFloat)col.Blue, (NFloat)col.Alpha
			});
		}

		public static CoreGraphics.CGColor ToCGColor(this System.Drawing.Color col)
		{
			return new CoreGraphics.CGColor(deviceRgbColorSpace, new NFloat[] {
				(NFloat)(col.R / 255.0f), (NFloat)(col.G / 255.0f), (NFloat)(col.B / 255.0f), (NFloat)(col.A / 255.0f),
		    });
		}

		public static NSAttributedString ToAttributedString(this FormattedText ft)
			=> ToAttributedString(ft, null);

		public static NSAttributedString ToAttributedString(
			this FormattedText ft,
			Action<NSMutableAttributedString, NSRange> beforeAttribution)
		{
			var ns = new NSMutableAttributedString(ft.Text);
			ns.BeginEditing();
			beforeAttribution?.Invoke(ns, new NSRange(0, ns.Length));
			foreach (var att in ft.Attributes)
			{
				var r = new NSRange(att.StartIndex, att.Count);
				if (att is BackgroundTextAttribute)
				{
					var xa = (BackgroundTextAttribute)att;
					ns.AddAttribute(NSStringAttributeKey.BackgroundColor, xa.Color.ToNSColor(), r);
				}
				else if (att is ColorTextAttribute)
				{
					var xa = (ColorTextAttribute)att;
					ns.AddAttribute(NSStringAttributeKey.ForegroundColor, xa.Color.ToNSColor(), r);
				}
				else if (att is UnderlineTextAttribute)
				{
					var xa = (UnderlineTextAttribute)att;
					var style = xa.Underline ? (int)NSUnderlineStyle.Single : 0;
					ns.AddAttribute(NSStringAttributeKey.UnderlineStyle, (NSNumber)style, r);
				}
				else if (att is FontStyleTextAttribute)
				{
					var xa = (FontStyleTextAttribute)att;
					if (xa.Style == FontStyle.Italic)
					{
						ns.ApplyFontTraits(NSFontTraitMask.Italic, r);
					}
					else if (xa.Style == FontStyle.Oblique)
					{
						ns.AddAttribute(NSStringAttributeKey.Obliqueness, (NSNumber)0.2f, r);
					}
					else
					{
						ns.AddAttribute(NSStringAttributeKey.Obliqueness, (NSNumber)0.0f, r);
						ns.ApplyFontTraits(NSFontTraitMask.Unitalic, r);
					}
				}
				else if (att is FontWeightTextAttribute)
				{
					var xa = (FontWeightTextAttribute)att;
					var trait = xa.Weight >= FontWeight.Bold ? NSFontTraitMask.Bold : NSFontTraitMask.Unbold;
					ns.ApplyFontTraits(trait, r);
				}
				else if (att is LinkTextAttribute)
				{
					var xa = (LinkTextAttribute)att;
					ns.AddAttribute(NSStringAttributeKey.Link, new NSUrl(xa.Target.ToString()), r);
					ns.AddAttribute(NSStringAttributeKey.ForegroundColor, MonoDevelop.Ide.Gui.Styles.LinkForegroundColor.ToNSColor(), r);
					ns.AddAttribute(NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), r);
				}
				else if (att is StrikethroughTextAttribute)
				{
					var xa = (StrikethroughTextAttribute)att;
					var style = xa.Strikethrough ? (int)NSUnderlineStyle.Single : 0;
					ns.AddAttribute(NSStringAttributeKey.StrikethroughStyle, (NSNumber)style, r);
				}
				else if (att is FontTextAttribute)
				{
					var xa = (FontTextAttribute)att;
					var nf = (NSFont)Toolkit.GetBackend(xa.Font);
					ns.AddAttribute(NSStringAttributeKey.Font, nf, r);
				}
			}
			ns.EndEditing();
			return ns;
		}
	}
}
