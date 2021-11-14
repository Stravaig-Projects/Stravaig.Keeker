using System.Management.Automation;
using Stravaig.Keeker.PowerShellModule.PowerShellObjects;

namespace Stravaig.Keeker.PowerShellModule.Commands;

[Cmdlet(VerbsCommon.Get,"DotNetAssembly")]
[OutputType(typeof(PsDotNetAssembly))]
public class GetDotNetAssemblyCommand : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    public string Path { get; set; }
    
    // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
    protected override void BeginProcessing()
    {
        WriteVerbose("Begin!");
    }

    // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
    protected override void ProcessRecord()
    {
        WriteObject(new PsDotNetAssembly { 
            Path = Path,
        });
    }

    // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
    protected override void EndProcessing()
    {
        WriteVerbose("End!");
    }
}