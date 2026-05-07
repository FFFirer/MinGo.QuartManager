#nullable enable
using Xunit;
using MinGo.Qap.Shared.Models;

namespace MinGo.QuartzManager.Agent.Tests;

public class JobTypeQualifiedNameTests
{
    [Fact]
    public void ParseFrom_WithFullAssemblyQualifiedName_ParsesAllParts()
    {
        var aqn = "Sample.Jobs.EchoJob, Sample.Jobs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        var result = JobTypeQualifiedName.ParseFrom(aqn);

        Assert.Equal("Sample.Jobs.EchoJob", result.FullName);
        Assert.Equal("Sample.Jobs", result.Assembly);
        Assert.Equal("1.0.0.0", result.Version);
        Assert.Equal("neutral", result.Culture);
        Assert.Equal("null", result.PublicKeyToken);
    }

    [Fact]
    public void ParseFrom_WithoutVersionCultureToken_ParsesPartial()
    {
        var aqn = "MyApp.Jobs.DataSync, MyApp.Jobs";
        var result = JobTypeQualifiedName.ParseFrom(aqn);

        Assert.Equal("MyApp.Jobs.DataSync", result.FullName);
        Assert.Equal("MyApp.Jobs", result.Assembly);
        Assert.Null(result.Version);
        Assert.Null(result.Culture);
        Assert.Null(result.PublicKeyToken);
    }

    [Fact]
    public void ParseFrom_WithNull_ReturnsUnknown()
    {
        var result = JobTypeQualifiedName.ParseFrom((string)null!);

        Assert.Equal("unknown", result.FullName);
        Assert.Equal(string.Empty, result.Assembly);
    }

    [Fact]
    public void ParseFrom_WithEmptyString_ReturnsUnknown()
    {
        var result = JobTypeQualifiedName.ParseFrom(string.Empty);

        Assert.Equal("unknown", result.FullName);
    }

    [Fact]
    public void ParseFrom_WithoutAssembly_SetsEmptyAssembly()
    {
        var aqn = "Some.Type.Only";
        var result = JobTypeQualifiedName.ParseFrom(aqn);

        Assert.Equal("Some.Type.Only", result.FullName);
        Assert.Equal(string.Empty, result.Assembly);
    }

    [Fact]
    public void ParseFrom_WithGenericType_HandlesBracketsCorrectly()
    {
        // Generic type with nested brackets - commas inside brackets should not split FullName
        var aqn = "Sample.Jobs.GenericJob1[[Sample.Jobs.ParamJob, Sample.Jobs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Sample.Jobs, Version=2.0.0.0";
        var result = JobTypeQualifiedName.ParseFrom(aqn);

        Assert.StartsWith("Sample.Jobs.GenericJob1[[", result.FullName);
        Assert.EndsWith("]]", result.FullName);
        Assert.Equal("Sample.Jobs", result.Assembly);
        Assert.Equal("2.0.0.0", result.Version);
    }

    [Fact]
    public void ToAssemblyQualifiedName_WithAssembly_ReturnsFullNameAssembly()
    {
        var qn = new JobTypeQualifiedName
        {
            FullName = "Sample.Jobs.EchoJob",
            Assembly = "Sample.Jobs",
            Version = "1.0.0.0"
        };

        var result = qn.ToAssemblyQualifiedName();

        // Should NOT include version/culture/token - only FullName, Assembly
        Assert.Equal("Sample.Jobs.EchoJob, Sample.Jobs", result);
    }

    [Fact]
    public void ToAssemblyQualifiedName_WithoutAssembly_ReturnsFullNameOnly()
    {
        var qn = new JobTypeQualifiedName
        {
            FullName = "Some.Type.Only"
        };

        var result = qn.ToAssemblyQualifiedName();

        Assert.Equal("Some.Type.Only", result);
    }

    [Fact]
    public void Equals_SameFullName_ReturnsTrue()
    {
        var a = new JobTypeQualifiedName { FullName = "A.B.C", Assembly = "X" };
        var b = new JobTypeQualifiedName { FullName = "A.B.C", Assembly = "Y" };

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentFullName_ReturnsFalse()
    {
        var a = new JobTypeQualifiedName { FullName = "A.B.C" };
        var b = new JobTypeQualifiedName { FullName = "X.Y.Z" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void GetHashCode_SameFullName_Equal()
    {
        var a = new JobTypeQualifiedName { FullName = "Same.Name" };
        var b = new JobTypeQualifiedName { FullName = "Same.Name" };

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsAssemblyQualifiedName()
    {
        var qn = new JobTypeQualifiedName
        {
            FullName = "Test.Job",
            Assembly = "Test"
        };

        Assert.Equal("Test.Job, Test", qn.ToString());
    }

    [Fact]
    public void ParseFrom_Type_ReturnsExpectedValues()
    {
        // Use a well-known .NET type for testing
        var type = typeof(string);
        var result = JobTypeQualifiedName.ParseFrom(type);

        Assert.Equal("System.String", result.FullName);
        Assert.Equal("System.Private.CoreLib", result.Assembly);
        Assert.NotNull(result.Version);
    }

    [Fact]
    public void RoundTrip_ParseThenToAssemblyQualifiedName_PreservesCoreIdentity()
    {
        var original = "MyApp.Jobs.HelloJob, MyApp.Jobs, Version=2.1.0.0, Culture=en-US, PublicKeyToken=abc123";
        var parsed = JobTypeQualifiedName.ParseFrom(original);
        var reassembled = parsed.ToAssemblyQualifiedName();

        // The version/culture/token are stripped in ToAssemblyQualifiedName()
        Assert.Equal("MyApp.Jobs.HelloJob, MyApp.Jobs", reassembled);

        // Parsing the stripped version should still get core fields right
        var reparsed = JobTypeQualifiedName.ParseFrom(reassembled);
        Assert.Equal("MyApp.Jobs.HelloJob", reparsed.FullName);
        Assert.Equal("MyApp.Jobs", reparsed.Assembly);
    }
}
