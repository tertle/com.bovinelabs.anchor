// <copyright file="FieldData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SourceGenerator
{
    using System.Collections.Generic;
    using BovineLabs.SourceGenerator.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public enum FieldMode : byte
    {
        Default,
        Changed,
        ChangedList,
    }

    public class FieldData
    {
        public FieldData(INamedTypeSymbol typeSymbol, INamedTypeSymbol[] ancestors, IReadOnlyCollection<string> namespaces, FieldDeclarationSyntax field)
        {
            this.TypeSymbol = typeSymbol;
            this.Ancestors = ancestors;
            this.Namespaces = namespaces;
            this.FieldName = field.GetFieldName();
            this.PropertyName = FormatPropertyName(this.FieldName);
            this.FieldType = field.GetFieldType();
            var typeSyntax = field.Declaration.Type;

            if (typeSyntax is GenericNameSyntax { Identifier: { Text: "Changed" } } changed)
            {
                this.FieldMode = FieldMode.Changed;
                this.ChangedType = changed.TypeArgumentList.Arguments.First().ToString();

            }
            else if (typeSyntax is GenericNameSyntax { Identifier: { Text: "ChangedList" } } changedList)
            {
                this.FieldMode = FieldMode.ChangedList;
                this.ChangedType = changedList.TypeArgumentList.Arguments.First().ToString();
            }
            else
            {
                this.FieldMode = FieldMode.Default;
                this.ChangedType = string.Empty;
            }
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public INamedTypeSymbol[] Ancestors { get;}

        public IReadOnlyCollection<string> Namespaces { get; }

        public string FieldName { get; }

        public string PropertyName { get; }

        public string FieldType { get; }

        public FieldMode FieldMode { get; }

        public string ChangedType { get; }

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
