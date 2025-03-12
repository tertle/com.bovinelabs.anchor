// <copyright file="SourceGenHelpers.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SourceGenerator
{
    using System.IO;
    using Microsoft.CodeAnalysis;

    public static class SourceGenHelpers
    {
        public static void Log(string message)
        {
            // Ignore IO exceptions in case there is already a lock, could use a named mutex but don't want to eat the performance cost
            try
            {
                string generatedCodePath = @"Logs\";
                var sourceGenLogPath = Path.Combine(generatedCodePath, "SystemPropertyGenerator.log");
                using var writer = File.AppendText(sourceGenLogPath);
                writer.WriteLine(message);
            }
            catch (IOException)
            {
            }
        }
    }
}
