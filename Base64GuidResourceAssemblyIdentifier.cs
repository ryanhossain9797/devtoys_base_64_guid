using DevToys.Api;
using System.ComponentModel.Composition;

namespace Base64Guid;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(Base64GuidResourceAssemblyIdentifier))]
internal sealed class Base64GuidResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}