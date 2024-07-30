using System;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;

namespace MFractor.VS.Mac.Views
{
    public class ClickableLabel : NSTextField
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public ClickableLabel() : base()
        {
            this.BecomeFirstResponder();
        }

        public ClickableLabel(Action onClickedAction) : base()
        {
            this.BecomeFirstResponder();
            OnClickedAction = onClickedAction;
        }

        public ClickableLabel(IntPtr handle) : base(handle)
        {
            this.BecomeFirstResponder();
        }

        [Export("initWithFrame:")]
        public ClickableLabel(CGRect frameRect) : base(frameRect)
        {
            this.BecomeFirstResponder();
        }

        public Action OnClickedAction { get; set; }

        public override void MouseUp(NSEvent theEvent)
        {
            try
            {
                if (OnClickedAction != null)
                {
                    OnClickedAction.Invoke();
                }
                else
                {
                    Debugger.Break();
                    Debug.WriteLine("Unable to invoke action on clickable label: " + StringValue);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                Debugger.Break();
            }

            base.MouseUp(theEvent);
        }
    }
}
