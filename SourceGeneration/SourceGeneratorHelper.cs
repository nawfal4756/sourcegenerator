namespace SourceGeneration;

public static class SourceGeneratorHelper
{
    public const string PermissionAttribute = $"namespace PermissionValidator;" +
                                              "public class PermissionAttribute : Attribute" +
                                              "{}";
}