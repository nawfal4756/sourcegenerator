using Microsoft.CodeAnalysis;
using System;

namespace IncrementalGenerator
{
    [Generator]
    public class PermissionIncrementalGenerator : IIncrementalGenerator
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

            namespace HuziafaNamespace
            {
                public class MyServiceProxy
                {
                    public string vidizmo = ""VIDIZMO"";

                    public void HelloWorld() {
                        Console.WriteLine(""Incremental"");
                    }
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
    }
}
