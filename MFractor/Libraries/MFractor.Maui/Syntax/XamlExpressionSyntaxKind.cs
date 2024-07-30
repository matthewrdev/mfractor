namespace MFractor.Maui.Syntax
{
    /// <summary>
    ///  namespace      Content                                 Expression
    ///     |              |                         /-----------------------------\   
    /// {local:Translate MyKey, Mode=TwoWay, Source={x:Static local:MyClass.MyMember}}
    ///           |             \_________/                   \____________________/
    ///          Type            Assignment                        Member Access
    ///  \_____________/            |                                    |
    ///       Symbol                V                                    V
    ///            Property - 'Mode'='TwoWay' - Value     Symbol - 'local:MyClass'.'MyMember' - Member
    /// </summary>
    public enum XamlExpressionSyntaxKind
    {
        Expression,
        Symbol,
        Namespace,
        TypeName,
        MemberAccessExpression,
        MemberName,
        Content,
        Assignment,
        Property,
        Value,
        StringValue,

        Error,
    }
}
