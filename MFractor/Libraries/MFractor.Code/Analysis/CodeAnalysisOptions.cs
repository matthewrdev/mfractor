using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeAnalysisOptions))]
    class CodeAnalysisOptions : ICodeAnalysisOptions
    {
        [ImportingConstructor]
        public CodeAnalysisOptions(Lazy<IUserOptions> userOptions,
                                   Lazy<ICodeAnalyserRepository> codeAnalyserRepository)
        {
            this.userOptions = userOptions;
            this.codeAnalyserRepository = codeAnalyserRepository;

            codeAnalysersMap = new Lazy<IReadOnlyDictionary<Type, IXmlSyntaxCodeAnalyser>>(() =>
            {
                return CodeAnalyserRepository.Analysers.ToDictionary(a => a.GetType(), a => a);
            });

            codeAnalyserEnabledCache = new Lazy<Dictionary<string, bool>>(() =>
            {
                var result = new Dictionary<string, bool>();

                foreach (var analyser in CodeAnalyserRepository.Analysers)
                {
                    var key = GetCodeAnalyserKey(analyser);

                    if (!UserOptions.HasKey(key))
                    {
                        UserOptions.Set(key, true);
                        result[key] = true;
                    }
                    else
                    {
                        result[key] = UserOptions.Get(key, true);
                    }
                }

                return result;
            });
        }

        public event EventHandler<CodeAnalysisStateChangedEventArgs> CodeAnalysisStateChanged;

        readonly Lazy<Dictionary<string, bool>> codeAnalyserEnabledCache;
        public Dictionary<string, bool> CodeAnalyserEnabledCache => codeAnalyserEnabledCache.Value;

        readonly Lazy<IReadOnlyDictionary<Type, IXmlSyntaxCodeAnalyser>> codeAnalysersMap;
        public IReadOnlyDictionary<Type, IXmlSyntaxCodeAnalyser> CodeAnalysersMap => codeAnalysersMap.Value;

        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<ICodeAnalyserRepository> codeAnalyserRepository;
        ICodeAnalyserRepository CodeAnalyserRepository => codeAnalyserRepository.Value;

        public string GetCodeAnalyserKey(IXmlSyntaxCodeAnalyser codeAnalyser)
        {
            return GetCodeAnalyserKey(codeAnalyser?.Identifier);
        }

        public string GetCodeAnalyserKey<TCodeAnalyser>() where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser
        {
            var type = typeof(TCodeAnalyser);

            if (!CodeAnalysersMap.TryGetValue(type, out var analyser))
            {
                return string.Empty;
            }

            return GetCodeAnalyserKey(analyser);
        }

        public string GetCodeAnalyserKey(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return string.Empty;
            }

            return identifier += ".enabled";
        }

        public bool IsEnabled(IXmlSyntaxCodeAnalyser codeAnalyser, bool bypassCache = false)
        {
            return IsEnabled(codeAnalyser?.Identifier, bypassCache);
        }

        public bool IsEnabled<TCodeAnalyser>(bool bypassCache = false) where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser
        {
            var type = typeof(TCodeAnalyser);

            if (!CodeAnalysersMap.TryGetValue(type, out var analyser))
            {
                return false;
            }

            return IsEnabled(analyser, bypassCache);
        }


        public bool IsEnabled(string identifier, bool bypassCache = false)
        {
            var key = GetCodeAnalyserKey(identifier);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!bypassCache && CodeAnalyserEnabledCache.ContainsKey(key))
            {
                return CodeAnalyserEnabledCache[key];
            }

            var result = UserOptions.Get(key, false);
            return result;
        }

        public void ToggleAnalyser(IXmlSyntaxCodeAnalyser codeAnalyser, bool enable)
        {
            ToggleAnalyser(codeAnalyser?.Identifier, enable);
        }

        public void ToggleAnalyser<TCodeAnalyser>(bool enable) where TCodeAnalyser : class, IXmlSyntaxCodeAnalyser
        {
            var type = typeof(TCodeAnalyser);

            if (!CodeAnalysersMap.TryGetValue(type, out var analyser))
            {
                return;
            }

            ToggleAnalyser(analyser, enable);
        }

        public void ToggleAnalyser(string identifier, bool enable)
        {
            var key = GetCodeAnalyserKey(identifier);

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            UserOptions.Set(key, enable);
            CodeAnalyserEnabledCache[key] = enable;

            CodeAnalysisStateChanged?.Invoke(this, new CodeAnalysisStateChangedEventArgs(identifier, enable));
        }

        public void ToggleAnalysers(IReadOnlyDictionary<IXmlSyntaxCodeAnalyser, bool> changes)
        {
            if (changes == null || !changes.Any())
            {
                return;
            }

            ToggleAnalysers(changes.Where(arg => arg.Key != null)
                                   .Select(arg => new KeyValuePair<string, bool>(arg.Key.Identifier, arg.Value))
                                   .ToDictionary(arg => arg.Key, arg => arg.Value));
        }

        public void ToggleAnalysers(IReadOnlyDictionary<string, bool> changes)
        {
            if (changes == null || !changes.Any())
            {
                return;
            }

            var finalChanges = new Dictionary<string, bool>();
            foreach (var kp in changes)
            {
                if (!string.IsNullOrEmpty(kp.Key))
                {
                    UserOptions.Set(kp.Key, kp.Value);
                    finalChanges[kp.Key] = kp.Value;
                }
            }

            CodeAnalysisStateChanged?.Invoke(this, new CodeAnalysisStateChangedEventArgs(finalChanges));
        }
    }
}
