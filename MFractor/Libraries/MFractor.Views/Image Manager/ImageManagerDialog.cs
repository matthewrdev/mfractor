using MFractor.Images.ImageManager;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageManager
{
    public class ImageManagerDialog : Dialog
    {
        static ImageManagerDialog instance;

        public static ImageManagerDialog GetOrCreate(Solution solution, IImageManagerOptions options)
        {
            if (instance == null)
            {
                instance = new ImageManagerDialog(solution, options);
            }
            else
            {
                instance.SetSolution(solution, options);
            }

            if (!instance.Visible)
            {
                instance.Show();
            }

            return instance;
        }

        public ImageManagerDialog(Solution solution, IImageManagerOptions options)
        {
            Title = "Image Manager";
            Icon = Image.FromResource("mfractor_logo.png");

            Content = new ImageManagerControl(solution, options);
        }

        public void SetSolution(Solution solution, IImageManagerOptions options)
        {
            (Content as ImageManagerControl).SetSolution(solution, force:true);
            (Content as ImageManagerControl).SetOptions(options);
        }

        public void Select(string imageAsset)
        {
            (Content as ImageManagerControl).Select(imageAsset);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            instance = null;
        }
    }
}
