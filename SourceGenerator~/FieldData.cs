// <copyright file="FieldData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SourceGenerator
{
    using System.Collections.Generic;
    using BovineLabs.SourceGenerator.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class FieldData
    {
        public FieldData(INamedTypeSymbol typeSymbol, INamedTypeSymbol[] ancestors, IReadOnlyCollection<string> namespaces, FieldDeclarationSyntax field, bool isChange)
        {
            this.TypeSymbol = typeSymbol;
            this.Ancestors = ancestors;
            this.Namespaces = namespaces;
            this.FieldName = field.GetFieldName();
            this.PropertyName = FormatPropertyName(this.FieldName);
            this.FieldType = field.GetFieldType();
            this.IsChange = isChange;
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public INamedTypeSymbol[] Ancestors { get;}

        public IReadOnlyCollection<string> Namespaces { get; }

        public string FieldName { get; }

        public string PropertyName { get; }

        public string FieldType { get; }

        public bool IsChange { get; }

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
