namespace BovineLabs.SystemPropertyGenerator
{
    using System.Collections.Generic;
    using BovineLabs.SystemPropertyGenerator.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public enum FieldMode : byte
    {
        Default,
        Changed,
        NativeList,
        ChangedList,
    }

    public class FieldData
    {
        public FieldData(INamedTypeSymbol typeSymbol, INamedTypeSymbol[] ancestors, IReadOnlyCollection<string> namespaces, FieldDeclarationSyntax field)
        {
            TypeSymbol = typeSymbol;
            Ancestors = ancestors;
            Namespaces = namespaces;
            FieldName = field.GetFieldName();
            PropertyName = FormatPropertyName(FieldName);
            FieldType = field.GetFieldType();
            var typeSyntax = field.Declaration.Type;

            if (typeSyntax is GenericNameSyntax { Identifier: { Text: "Changed" } } changed)
            {
                FieldMode = FieldMode.Changed;
                GenericType = changed.TypeArgumentList.Arguments.First().ToString();

            }
            else if (typeSyntax is GenericNameSyntax { Identifier: { Text: "ChangedList" } } changedList)
            {
                FieldMode = FieldMode.ChangedList;
                GenericType = changedList.TypeArgumentList.Arguments.First().ToString();
            }
            else if (typeSyntax is GenericNameSyntax { Identifier: { Text: "NativeList" } } nativeList)
            {
                FieldMode = FieldMode.NativeList;
                GenericType = nativeList.TypeArgumentList.Arguments.First().ToString();
            }
            else
            {
                FieldMode = FieldMode.Default;
                GenericType = string.Empty;
            }
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public INamedTypeSymbol[] Ancestors { get;}

        public IReadOnlyCollection<string> Namespaces { get; }

        public string FieldName { get; }

        public string PropertyName { get; }

        public string FieldType { get; }

        public FieldMode FieldMode { get; }

        public string GenericType { get; }

        private static string FormatPropertyName(string fieldName)
        {
            // support both common formats, _fieldName and fieldName
            if (fieldName[0] == '_')
            {
                fieldName = fieldName.Substring(1, fieldName.Length-1);
            }

            return $"{char.ToUpper(fieldName[0])}{fieldName.Substring(1)}";
        }
    }
}
