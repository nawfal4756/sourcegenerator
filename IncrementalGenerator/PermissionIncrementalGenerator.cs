using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace IncrementalGenerator
{
    [Generator]
    public class PermissionIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classWithAttributes = context.SyntaxProvider.CreateSyntaxProvider(
                                                            predicate: static (s, _) =>                                                             IsClassWithAttribute(s),
                                                            transform: static (ctx, _)                                                      => GetClassSymbol(ctx))
                                                            .Where(symbol => symbol is not null);

            var compilation = context.CompilationProvider.Combine(classWithAttributes.Collect());

            context.RegisterSourceOutput(compilation, Execute);
        }

   

        private static bool IsClassWithAttribute(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0;
        }

        private static INamedTypeSymbol? GetClassSymbol(GeneratorSyntaxContext context)
        {
            var classSyntax = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

            if(classSymbol?.GetAttributes().Any(ad => ad.AttributeClass?.Name == "PermissionAttribute") == true)
            {
                return classSymbol;
            }

            return null;

        }

        private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<INamedTypeSymbol?> Right) tuple)
        {
            foreach (var classSymbol in tuple.Right)
            {
                if (classSymbol == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "PG001", // Diagnostic ID
                                    "Class Symbol Missing", // Title
                                    "Class symbol is null in the source generator", // Message format
                                    "SourceGenerator", // Category
                                    DiagnosticSeverity.Warning, // Severity
                                    isEnabledByDefault: true),
                                    Location.None));

                    continue;
                }

                // Retrieve the namespace and class name
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var className = classSymbol.Name;

                // Generate proxy class code for this class symbol
                var proxyClassCode = GenerateProxyClass(classSymbol, namespaceName, className);

                

                // Add the generated proxy class to the compilation as a source file
                context.AddSource($"{className}Proxy.g.cs", proxyClassCode);
            }
        }

        private string GenerateProxyClass(INamedTypeSymbol classSymbol, string namespaceName, string className)
        {
            var proxyClassName = $"{className}Proxy";
            var methods = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary && !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {proxyClassName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {className} _inner;");
            sb.AppendLine();
            sb.AppendLine($"        public {proxyClassName}({className} inner)");
            sb.AppendLine("        {");
            sb.AppendLine("            _inner = inner;");
            sb.AppendLine("        }");

            foreach (var method in methods)
            {
                GenerateMethodProxy(sb, method);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateMethodProxy(StringBuilder sb, IMethodSymbol method)
        {
            var methodName = method.Name;
            var returnType = method.ReturnType.ToDisplayString();
            var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));

            var isAsync = method.ReturnType.Name == "Task" || method.ReturnType.Name.StartsWith("Task<");
            var asyncModifier = isAsync ? "async " : "";
            var awaitKeyword = isAsync ? "await " : "";
            var returnKeyword = method.ReturnsVoid ? "" : "return ";

            sb.AppendLine();
            sb.AppendLine($"        public {asyncModifier}{returnType} {methodName}({parameters})");
            sb.AppendLine("        {");
            sb.AppendLine($"            Console.WriteLine(\"Intercepting {methodName}\");");
            sb.AppendLine($"            {returnKeyword}{awaitKeyword}_inner.{methodName}({arguments});");
            sb.AppendLine("        }");
        }




    }


}
