using System;
using System.Collections;
using System.Collections.Generic;
using MFractor.Xml;

namespace MFractor.Maui.Semantics
{
    class CodeBehindFieldCollection : ICodeBehindFieldCollection
    {
        class CodeBehindField : ICodeBehindField
        {
            public CodeBehindField(string name, XmlNode node)
            {
                Name = name;
                Node = node;
            }

            public string Name { get; }

            public XmlNode Node { get; }
        }

        readonly Lazy<IReadOnlyDictionary<string, ICodeBehindField>> codeBehindFields;
        public IReadOnlyDictionary<string, ICodeBehindField> CodeBehindFields => codeBehindFields.Value;

        public CodeBehindFieldCollection(XmlSyntaxTree syntaxTree)
        {
            codeBehindFields = new Lazy<IReadOnlyDictionary<string, ICodeBehindField>>(() =>
            {
                var fields = new Dictionary<string, ICodeBehindField>();

                DiscoverCodeBehindFields(syntaxTree.Root, fields);

                return fields;
            });

        }

        void DiscoverCodeBehindFields(XmlNode syntax, Dictionary<string, ICodeBehindField> fields)
        {
            if (syntax == null)
            {
                return;
            }

            if (syntax.HasAttribute("x:Name"))
            {
                var xName = syntax.GetAttributeByName("x:Name").Value?.Value;

                if (!fields.ContainsKey(xName))
                {
                    fields[xName] = new CodeBehindField(xName, syntax);
                }
            }

            if (!syntax.HasChildren)
            {
                return;
            }

            foreach (var child in syntax.Children)
            {
                DiscoverCodeBehindFields(child, fields);
            }
        }

        public ICodeBehindField GetCodeBehindField(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }

            if (!CodeBehindFields.ContainsKey(name))
            {
                return default;
            }

            return CodeBehindFields[name];
        }

        public IEnumerator<ICodeBehindField> GetEnumerator()
        {
            return CodeBehindFields.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CodeBehindFields.Values.GetEnumerator();
        }
    }
}