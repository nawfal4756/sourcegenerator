using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneration
{
    [Generator]
    public class PermissionSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.CompilationProvider;

            context.RegisterSourceOutput(provider, Execute);
        }

        private void Execute(SourceProductionContext context, object source)
        {
            try
            {
                // Your code generation logic
                var classCode = @"
            using System;

            namespace Vidizmo.Permission
            {
                public static class Nawfal 
                {
                    public static string vidizmo = ""VIDIZMO"";
                }
            }
        ";

                context.AddSource("Nawfal.g.cs", classCode);
            }
            catch (Exception ex)
            {
                // Report the exception as a diagnostic
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SG002",
                        "Source Generator Exception",
                        $"An exception occurred in Execute: {ex.Message}",
                        "SourceGenerator",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None));
            }
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0;
        }

        private static MethodDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

            foreach (var attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol methodSymbol)
                    {
                        continue;
                    }
                    
                    if (methodSymbol.ContainingNamespace.Name == "PermissionValidator" &&
                        methodSymbol.Name == "PermissionAttribute")
                    {
                        return methodDeclarationSyntax;
                    }
                }
            }

            return null;
        }

    }
    
}
