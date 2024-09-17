using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

var scriptState = await CSharpScript.RunAsync("Console.WriteLine(\"Hello, World!\");");
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    scriptState = await scriptState.ContinueWithAsync(input);
}