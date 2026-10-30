namespace Tests.SharpArch.SourceGenerators.DomainSignature.SourceGenerator;

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using global::SharpArch.SourceGenerators.DomainSignature;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;


/// <summary>
///     In-process generator-driver tests that validate the <see cref="DomainSignatureGenerator" />
///     diagnostics and source-emission behaviour (valid input, partial / base-type / namespace /
///     nesting guards, record support and unique hint names) so the behaviour cannot regress.
/// </summary>
/// <remarks>
///     The tests are self-contained: they declare stub <c>BaseObject</c> and attribute types with
///     the exact metadata names the generator matches, and exclude the real SharpArch.* assemblies
///     from the reference set so the stubs are the authoritative definitions. The stubs are parsed
///     as a separate syntax tree so the per-test source files keep their using-directives first.
/// </remarks>
public class DomainSignatureGeneratorDriverTests
{
    const string DomainStubs = @"namespace SharpArch.Domain.DomainModel
{
    public class BaseObject
    {
        public virtual bool HasSameObjectSignatureAs(BaseObject compareTo) => false;
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class GenerateDomainSignatureAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class DomainSignatureAttribute : System.Attribute { }
}";

    static readonly MetadataReference[] References = BuildReferences();

    static MetadataReference[] BuildReferences()
    {
        // Touch these types so the BCL assemblies are loaded before enumerating the AppDomain.
        _ = typeof(object);
        _ = typeof(Enumerable);

        // Exclude the real SharpArch.* assemblies so the stub types declared above are the
        // authoritative definitions and there is no type-name ambiguity.
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && File.Exists(a.Location))
            .Where(a => !a.FullName!.StartsWith("SharpArch", StringComparison.Ordinal))
            .Select(a => a.Location)
            .Distinct()
            .Select(loc => (MetadataReference)MetadataReference.CreateFromFile(loc))
            .ToArray();
    }

    static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<SyntaxTree> Trees) RunGenerator(string source)
    {
        // The stubs and the entity under test are parsed as separate syntax trees so each file
        // is valid on its own (using-directives must precede namespace declarations).
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            new[]
            {
                CSharpSyntaxTree.ParseText(SourceText.From(DomainStubs, Encoding.UTF8)),
                CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8))
            },
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DomainSignatureGenerator());
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();
        return (result.Diagnostics, result.GeneratedTrees);
    }

    static bool HasDiagnostic(ImmutableArray<Diagnostic> diagnostics, string id)
        => diagnostics.Any(d => d.Id == id);

    static bool HasGeneratedSource(ImmutableArray<SyntaxTree> trees, string needle)
        => trees.Any(t => t.ToString().Contains(needle));

    static string GeneratedSource(ImmutableArray<SyntaxTree> trees, string needle)
    {
        var tree = trees.SingleOrDefault(t => t.ToString().Contains(needle));
        tree.ShouldNotBeNull($"expected generated source containing '{needle}'");
        return tree!.ToString();
    }

    [Fact]
    public void ValidPartialBaseObjectEntity_GeneratesSignatureMethods()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

[GenerateDomainSignature]
public partial class Foo : BaseObject
{
    [DomainSignature]
    public int Code { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH001").ShouldBeFalse();
        HasDiagnostic(diagnostics, "SHARPARCH002").ShouldBeFalse();
        HasDiagnostic(diagnostics, "SHARPARCH004").ShouldBeFalse();
        HasDiagnostic(diagnostics, "SHARPARCH005").ShouldBeFalse();

        var code = GeneratedSource(trees, "partial class Foo");
        code.ShouldContain("public override int GetHashCode()");
        code.ShouldContain("HashCode.Combine(GetType()");
        code.ShouldContain("public override bool HasSameObjectSignatureAs(BaseObject compareTo)");
        code.ShouldContain("var that = compareTo as Foo;");
    }

    [Fact]
    public void NonPartialClass_ReportsDiagnosticAndDoesNotEmit()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

[GenerateDomainSignature]
public class NonPartial : BaseObject
{
    [DomainSignature]
    public int Code { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH001").ShouldBeTrue("non-partial class must report SHARPARCH001");
        HasGeneratedSource(trees, "partial class NonPartial").ShouldBeFalse("must not emit for a non-partial class");
    }

    [Fact]
    public void ClassNotDerivingFromBaseObject_ReportsDiagnosticAndDoesNotEmit()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

[GenerateDomainSignature]
public partial class NoBase
{
    [DomainSignature]
    public int Code { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH002").ShouldBeTrue("non-BaseObject class must report SHARPARCH002");
        HasGeneratedSource(trees, "partial class NoBase").ShouldBeFalse("must not emit when the class does not derive from BaseObject");
    }

    [Fact]
    public void ClassWithoutNamespace_ReportsDiagnosticAndDoesNotEmit()
    {
        var source = @"using SharpArch.Domain.DomainModel;

[GenerateDomainSignature]
public partial class GlobalFoo : BaseObject
{
    [DomainSignature]
    public int Code { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH004").ShouldBeTrue("global-namespace class must report SHARPARCH004");
        HasGeneratedSource(trees, "partial class GlobalFoo").ShouldBeFalse("must not emit for a class declared in the global namespace");
    }

    [Fact]
    public void NestedClass_ReportsDiagnosticAndDoesNotEmit()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

public partial class Outer
{
    [GenerateDomainSignature]
    public partial class Nested : BaseObject
    {
        [DomainSignature]
        public int Code { get; set; }
    }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH005").ShouldBeTrue("nested class must report SHARPARCH005");
        HasGeneratedSource(trees, "partial class Nested").ShouldBeFalse("must not emit for a nested class");
    }

    [Fact]
    public void TwoSameNamedClassesInDifferentNamespaces_BothEmitWithoutCollision()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace A
{
    [GenerateDomainSignature]
    public partial class Customer : BaseObject
    {
        [DomainSignature]
        public int Code { get; set; }
    }
}
namespace B
{
    [GenerateDomainSignature]
    public partial class Customer : BaseObject
    {
        [DomainSignature]
        public int Code { get; set; }
    }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH001").ShouldBeFalse();

        var customerTrees = trees.Where(t => t.ToString().Contains("partial class Customer")).ToArray();
        customerTrees.Length.ShouldBe(2, "both same-named classes in different namespaces must emit (unique hint names)");
    }

    [Fact]
    public void RecordDerivingFromBaseObject_GeneratesPartialRecord()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

[GenerateDomainSignature]
public partial record RecEntity : BaseObject
{
    [DomainSignature]
    public int Code { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH001").ShouldBeFalse();
        HasDiagnostic(diagnostics, "SHARPARCH002").ShouldBeFalse();

        var code = GeneratedSource(trees, "RecEntity");
        code.ShouldContain("partial record RecEntity");
        code.ShouldNotContain("partial class RecEntity");
    }

    [Fact]
    public void ClassWithoutDomainSignatureProperties_ReportsWarningButStillEmits()
    {
        var source = @"using SharpArch.Domain.DomainModel;
namespace TestNs;

[GenerateDomainSignature]
public partial class NoSig : BaseObject
{
    public int SomeProp { get; set; }
}";

        var (diagnostics, trees) = RunGenerator(source);

        HasDiagnostic(diagnostics, "SHARPARCH003").ShouldBeTrue("no DomainSignature properties must report SHARPARCH003");

        var code = GeneratedSource(trees, "partial class NoSig");
        code.ShouldContain("return base.GetHashCode()");
        code.ShouldContain("return base.Equals(compareTo)");
    }
}