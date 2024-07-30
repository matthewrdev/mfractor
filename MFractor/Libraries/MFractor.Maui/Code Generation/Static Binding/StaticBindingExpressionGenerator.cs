//using System;
//using System.ComponentModel.Composition;
//using MFractor.Code.CodeGeneration;

//namespace MFractor.Maui.CodeGeneration.StaticBinding
//{
//    [PartCreationPolicy(CreationPolicy.Shared)]
//    [Export(typeof(IStaticBindingExpressionGenerator))]
//    class StaticBindingExpressionGenerator : CodeGenerator, IStaticBindingExpressionGenerator
//    {
//        public override string[] Languages { get; } = new string[] { "XAML" };

//        public override string Identifier => "com.mfractor.code_gen.xaml.x_static_expression";

//        public override string Name => "x:Static Expression Generator";

//        public override string Documentation => "Generates an x:Static expression for a given property or field."
//    }
//}