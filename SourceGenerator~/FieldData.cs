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
        public FieldData(INamedTypeSymbol typeSymbol, INamedTypeSymbol[] ancestors, IReadOnlyCollection<string> namespaces, FieldDeclarationSyntax field)
        {
            this.TypeSymbol = typeSymbol;
            this.Ancestors = ancestors;
            this.Namespaces = namespaces;
            this.FieldName = field.GetFieldName();
            this.PropertyName = $"{char.ToUpper(this.FieldName[0])}{this.FieldName.Substring(1)}";
            this.FieldType = field.GetFieldType();
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public INamedTypeSymbol[] Ancestors { get;}

        public IReadOnlyCollection<string> Namespaces { get; }

        public string FieldName { get; }

        public string PropertyName { get; }

        public string FieldType { get; }
    }
}
