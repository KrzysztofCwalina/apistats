using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

string[] commandLine = Environment.GetCommandLineArgs();
if (commandLine.Length < 2) {
    Console.WriteLine($"usage: apistats <binary>");
    return;
}
string filename = commandLine[1];

using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
using var peReader = new PEReader(fileStream);
MetadataReader mr = peReader.GetMetadataReader();

int typeCount = 0;
int methodCount = 0;
int propertyGetterCount = 0;
int propertySetterCount = 0;
foreach (TypeDefinitionHandle typeHandle in mr.TypeDefinitions)
{
    TypeDefinition type = mr.GetTypeDefinition(typeHandle);
    if (type.Attributes.HasFlag(TypeAttributes.Public))
    {
        typeCount++;

        foreach(MethodDefinitionHandle methodHandle in type.GetMethods())
        {
            MethodDefinition method = mr.GetMethodDefinition(methodHandle);
            if (method.Attributes.HasFlag(MethodAttributes.Public))
            {
                methodCount++;
            }
        }

        foreach (PropertyDefinitionHandle propertyHandle in type.GetProperties())
        {
            PropertyDefinition property = mr.GetPropertyDefinition(propertyHandle);
            PropertyAccessors acessors = property.GetAccessors();
            MethodDefinitionHandle getterHandle = acessors.Getter;

            if (!getterHandle.IsNil)
            {
                MethodDefinition getter = mr.GetMethodDefinition(getterHandle);
                if (getter.Attributes.HasFlag(MethodAttributes.Public))
                {
                    propertyGetterCount++;
                }
            }

            MethodDefinitionHandle setterHandle = acessors.Setter;
            if (!setterHandle.IsNil)
            {
                MethodDefinition setter = mr.GetMethodDefinition(setterHandle);
                if (setter.Attributes.HasFlag(MethodAttributes.Public))
                {
                    propertySetterCount++;
                }
            }
        }
    }
}

Console.WriteLine($"Types      : {typeCount}");
Console.WriteLine($"Methods    : {methodCount}");
Console.WriteLine($"Properties : {propertyGetterCount}");

