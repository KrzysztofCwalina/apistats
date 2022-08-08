using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

var cl = Environment.GetCommandLineArgs();
if (cl.Length < 2) {
    Console.WriteLine($"usage: apistats <binary>");
    return;
}

var filename = cl[1];

int typeCount = 0;
int memberCount = 0;

using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using var peReader = new PEReader(fs);

MetadataReader mr = peReader.GetMetadataReader();

foreach (var tdefh in mr.TypeDefinitions)
{
    TypeDefinition tdef = mr.GetTypeDefinition(tdefh);
    if (tdef.Attributes.HasFlag(TypeAttributes.Public))
    {
        typeCount++;
        //string ns = mr.GetString(tdef.Namespace);

        var methods = tdef.GetMethods();
        var properties = tdef.GetProperties();

        foreach(MethodDefinitionHandle methodHandle in methods)
        {
            var method = mr.GetMethodDefinition(methodHandle);
            if (method.Attributes.HasFlag(MethodAttributes.Public))
            {
                memberCount++;
            }
        }

        foreach (PropertyDefinitionHandle propertyHandle in properties)
        {
            PropertyDefinition property = mr.GetPropertyDefinition(propertyHandle);
            var acessors = property.GetAccessors();
            var getterHandle = acessors.Getter;

            if (!getterHandle.IsNil)
            {
                var getter = mr.GetMethodDefinition(getterHandle);
                if (getter.Attributes.HasFlag(MethodAttributes.Public))
                {
                    memberCount++;
                }
            }

            var setterHandle = acessors.Setter;
            if (!setterHandle.IsNil)
            {
                var setter = mr.GetMethodDefinition(setterHandle);
                if (setter.Attributes.HasFlag(MethodAttributes.Public))
                {
                    memberCount++;
                }
            }
        }
    }
}

Console.WriteLine($"Types  : {typeCount}");
Console.WriteLine($"Members: {memberCount}");

