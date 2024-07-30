using System;
using System.Reflection;
using MFractor.Configuration.Attributes;

namespace MFractor.Configuration
{
    /// <summary>
    /// A mapping from a property 
    /// </summary>
    public sealed class ConfigurableProperty : IConfigurableProperty
    {
        public ConfigurableProperty(object ownerInstance,
                                    PropertyInfo property,
                                    string description)
        {
            OwnerInstance = ownerInstance;
            Property = property;
            Description = description;
            try
            {
                DefaultValue = property.GetValue(ownerInstance);
            } 
            catch 
            {
                DefaultValue = Activator.CreateInstance(property.PropertyType);
            }
        }

        /// <summary>
        /// The owning object instance of this property
        /// </summary>
        /// <value>The owner instance.</value>
        public object OwnerInstance { get; }

        /// <summary>
        /// A reflection PropertyInfo instance that can be used to mutate the property on the <see cref="P:MFractor.Configuration.IConfigurableProperty.OwnerInstance"/>.
        /// </summary>
        /// <value>The property.</value>
        public PropertyInfo Property { get; }

        /// <summary>
        /// A description of this property.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// The name of this property.
        /// </summary>
        /// <value>The name.</value>
        public string Name => Property.Name;

        /// <summary>
        /// The type that this property is.
        /// </summary>
        /// <value>The property type.</value>
        public Type PropertyType => Property.PropertyType;

        /// <summary>
        /// The properties original value.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; }

        /// <summary>
        /// The properties current value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                return Property.GetValue(OwnerInstance);
            }
            set
            {
                Property.SetValue(OwnerInstance, value);
            }
        }

        /// <example><para>The following example outputs the textual description of
        /// the value of an object of type <see cref="System.Object" /> to the console.</para><code lang="C#">using System;
        /// 
        /// class MyClass {
        /// static void Main() {
        /// object o = new object();
        /// Console.WriteLine (o.ToString());
        /// }
        /// }
        /// </code><para>The output is</para><para><c>System.Object</c></para></example><remarks><attribution license="cc4" from="Microsoft" modified="false" /><para><see cref="M:System.Object.ToString" /> is the major formatting method in the .NET Framework. It converts an object to its string representation so that it is suitable for display. (For information about formatting support in the .NET Framework, see <format type="text/html"><a href="0d1364da-5b30-4d42-8e6b-03378343343f">Formatting Types</a></format>.) </para><para>The default implementation of the <see cref="M:System.Object.ToString" /> method returns the fully qualified name of the type of the <see cref="System.Object" />, as the following example shows.</para><para>code reference: System.Object.ToString#1</para><para>Because <see cref="System.Object" /> is the base class of all reference types in the .NET Framework, this behavior is inherited by reference types that do not override the <see cref="M:System.Object.ToString" /> method. The following example illustrates this. It defines a class named Object1 that accepts the default implementation of all <see cref="System.Object" /> members. Its <see cref="M:System.Object.ToString" /> method returns the object's fully qualified type name.</para><para>code reference: System.Object.ToString#2</para><para>Types commonly override the <see cref="M:System.Object.ToString" /> method to return a string that represents the object instance. For example, the base types such as <see cref="System.Char" />, <see cref="System.Int32" />, and <see cref="System.String" /> provide <see cref="M:System.Object.ToString" /> implementations that return the string form of the value that the object represents. The following example defines a class, Object2, that overrides the <see cref="M:System.Object.ToString" /> method to return the type name along with its value.</para><para>code reference: System.Object.ToString#3</para><format type="text/html"><h2>Notes for the wrt</h2></format><para>When you call the <see cref="M:System.Object.ToString" /> method on a class in the wrt, it provides the default behavior for classes that don’t override <see cref="M:System.Object.ToString" />. This is part of the support that the .NET Framework provides for the wrt (see <format type="text/html"><a href="6fa7d044-ae12-4c54-b8ee-50915607a565">.NET Framework Support for Windows Store Apps and Windows Runtime</a></format>). Classes in the wrt don’t inherit <see cref="System.Object" />, and don’t always implement a <see cref="M:System.Object.ToString" />. However, they always appear to have <see cref="M:System.Object.ToString" />, <see cref="M:System.Object.Equals(System.Object)" />, and <see cref="M:System.Object.GetHashCode" /> methods when you use them in your C# or Visual Basic code, and the .NET Framework provides a default behavior for these methods. </para><para>Starting with the net_v451, the common language runtime will use <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> on a wrt object before falling back to the default implementation of <see cref="M:System.Object.ToString" />. </para><block subset="none" type="note"><para>wrt classes that are written in C# or Visual Basic can override the <see cref="M:System.Object.ToString" /> method. </para></block><format type="text/html"><h2>The wrt and the IStringable Interface</h2></format><para>Starting with win81, the wrt includes an <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> interface whose single method, <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see>, provides basic formatting support comparable to that provided by <see cref="M:System.Object.ToString" />. To prevent ambiguity, you should not implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> on managed types. </para><para>When managed objects are called by native code or by code written in languages such as JavaScript or C++/CX, they appear to implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see>. The common language runtime will automatically route calls from <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> to <see cref="M:System.Object.ToString" /> in the event <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> is not implemented on the managed object. </para><block subset="none" type="note"><para>Because the common language runtime auto-implements <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> for all managed types in win8_appstore_long apps, we recommend that you do not provide your own <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> implementation. Implementing <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> may result in unintended behavior when calling ToString  from the wrt, C++/CX, or JavaScript. </para></block><para>If you do choose to implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> in a public managed type that is exported in a wrt component, the following restrictions apply: </para><list type="bullet"><item><para>You can define the <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> interface only in a "class implements" relationship, such as </para><code>public class NewClass : IStringable</code><para>in C#, or</para><code>Public Class NewClass : Implements IStringable</code><para>in Visual Basic. </para></item><item><para>You cannot implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> on an interface. </para></item><item><para>You cannot declare a parameter to be of type <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see>. </para></item><item><para><see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> cannot be the return type of a method, property, or field. </para></item><item><para>You cannot hide your <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> implementation from base classes by using a method definition such as the following:  </para><code>
        /// public class NewClass : IStringable
        /// {
        /// public new string ToString()
        /// {
        /// return "New ToString in NewClass";
        /// }
        /// }
        /// </code><para>Instead, the <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> implementation must always override the base class implementation. You can hide a ToString implementation only by invoking it on a strongly typed class instance. </para></item></list><para>Note that under a variety of conditions, calls from native code to a managed type that implements <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> or hides its <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">ToString</see> implementation can produce unexpected behavior. </para></remarks><summary><attribution license="cc4" from="Microsoft" modified="false" /><para>Returns a string that represents the current object.</para></summary><returns><attribution license="cc4" from="Microsoft" modified="false" /><para>A string that represents the current object.</para></returns>
        public override string ToString()
        {
            return $"OwnerInstance={OwnerInstance.GetType().Name}, Name={Name}, Type={PropertyType}";
        }
    }
    
}
