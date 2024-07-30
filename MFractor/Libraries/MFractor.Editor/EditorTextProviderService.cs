using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Text;
using MFractor.Workspace;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITextProviderService))]
    [Export(typeof(ITextViewService))]
    [Export(typeof(IMutableTextViewService))]
    [Export]
    class EditorTextProviderService : TextProviderService, ITextViewService, IMutableTextViewService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IMutableWorkspaceService> mutableWorkspaceService;
        public IMutableWorkspaceService MutableWorkspaceService => mutableWorkspaceService.Value;

        public IReadOnlyList<ITextView> ActiveTextViews => textViewBindings.Values.Select(binding => binding.TextView).ToList();

        [ImportingConstructor]
        public EditorTextProviderService(Lazy<IMutableWorkspaceService> mutableWorkspaceService)
        {
            this.mutableWorkspaceService = mutableWorkspaceService;
        }

        readonly Dictionary<string, TextViewBinding> textViewBindings = new Dictionary<string, TextViewBinding>();

        public event EventHandler<TextViewEventArgs> OnTextViewOpened;
        public event EventHandler<TextViewEventArgs> OnTextViewClosed;

        public void NotifyClosed(string filePath)
        {
            var textView = GetActiveTextView(filePath);

            NotifyClosed(filePath, textView);
        }

        public void NotifyClosed(string filePath, ITextView textView)
        {
            if (string.IsNullOrEmpty(filePath) || textView is null)
            {
                return;
            }

            OnTextViewClosed?.Invoke(this, new TextViewEventArgs(filePath, textView));
        }
        public void NotifyOpened(string filePath)
        {
            var textView = GetActiveTextView(filePath);

            NotifyOpened(filePath, textView);
        }

        public void NotifyOpened(string filePath, ITextView textView)
        {
            if (string.IsNullOrEmpty(filePath) || textView is null)
            {
                return;
            }

            OnTextViewOpened?.Invoke(this, new TextViewEventArgs(filePath, textView));
        }

        public void BindTextView(string filePath, string projectGuid, ITextView textView)
        {
            try
            {
                var binding = new TextViewBinding(filePath, projectGuid, textView, MutableWorkspaceService, this);

                textViewBindings[filePath] = binding;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        protected override bool IsActiveFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return textViewBindings.ContainsKey(filePath);
        }

        protected override ITextProvider GetActiveFileTextProvider(string filePath)
        {
            if (!IsActiveFile(filePath))
            {
                return new CachedFileSystemTextProvider(filePath);
            }

            if (!textViewBindings.TryGetValue(filePath, out var binding))
            {
                return new CachedFileSystemTextProvider(filePath);
            }

            return new StringTextProvider(binding.TextView.TextBuffer.CurrentSnapshot.GetText());
        }

        public bool HasActiveTextView(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return textViewBindings.ContainsKey(filePath);
        }

        public ITextView GetActiveTextView(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return default;
            }

            if (!textViewBindings.TryGetValue(filePath, out var binding))
            {
                return default;
            }

            return binding.TextView;
        }
    }
}
